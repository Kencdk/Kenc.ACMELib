namespace CloudflareIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using DnsClient;
    using DnsClient.Protocol;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEEntities;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions.API;
    using Kenc.Cloudflare.Core.Clients;
    using Kenc.Cloudflare.Core.Clients.Enums;
    using Kenc.Cloudflare.Core.Entities;
    using Kenc.Cloudflare.Core.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///  Class containing logic to order domains.
    /// </summary>
    public class OrderDomains
    {
        private readonly Options options;
        private readonly ICloudflareClient cloudflareClient;
        private readonly ACMEClient acmeClient;
        private ACMEDirectory acmeDirectory;

        private Dictionary<string, Zone> CloudflareZones;
        public OrderDomains(Options options)
        {
            this.options = options;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            ServiceProvider services = serviceCollection.BuildServiceProvider();
            IHttpClientFactory httpClientFactory = services.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            cloudflareClient = new CloudflareClientFactory(options.Username, options.ApiKey, new CloudflareRestClientFactory(httpClientFactory), CloudflareAPIEndpoint.V4Endpoint)
                .Create();

            // RSA service provider
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048);
            if (string.IsNullOrEmpty(options.Key))
            {
                Program.LogLine("Generating new key for ACME.");
                var exportKey = rsaCryptoServiceProvider.ExportCspBlob(true);
                var strKey = Convert.ToBase64String(exportKey);

                File.WriteAllText("acmekey.key", strKey);
            }
            else
            {
                var key = Convert.FromBase64String(File.ReadAllText(options.Key));
                rsaCryptoServiceProvider.ImportCspBlob(key);
            }

            var rsaKey = RSA.Create(rsaCryptoServiceProvider.ExportParameters(true));
            acmeClient = new ACMEClient(
                options.Environment == AcmeEnvironment.ProductionV2 ? ACMEEnvironment.ProductionV2 : ACMEEnvironment.StagingV2,
                rsaKey,
                new RestClientFactory());
        }

        public async Task ValidateCloudflareConnection()
        {
            var cleanedupDomains = options.Domains.Select(GetRootDomain)
                .Distinct()
                .ToList();

            // list all zones.
            IList<Zone> cloudflareZones = await cloudflareClient.Zones.ListAsync();

            CloudflareZones = cloudflareZones.Where(x => cleanedupDomains.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(x => x.Name, x => x);

            var missingDomains = cleanedupDomains.Except(CloudflareZones.Keys, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (missingDomains.Any())
            {
                throw new Exception($"The following domains are not accesible in the cloudflare account. {string.Join(',', missingDomains)}");
            }
        }

        public async Task ValidateACMEConnection()
        {
            acmeDirectory = await acmeClient.InitializeAsync();
            Kenc.ACMELib.ACMEEntities.Account account = null;
            if (!string.IsNullOrEmpty(options.Key))
            {
                Console.WriteLine("Validating if user exists with existing key");
                try
                {
                    account = await acmeClient.GetAccountAsync();
                }
                catch (AccountDoesNotExistException exception)
                {
                    Program.LogLine(exception.Message);
                }

                if (account != null)
                {
                    Program.LogLine($"Using previously created account {account.Id}");
                }
                else
                {
                    Program.LogLine("Couldn't retrieve existing account. Creating new account.");
                }
            }

            if (account == null)
            {
                Program.LogLine("Creating user..");
                Program.LogLine($"By creating this user, you acknowledge the terms of service: {acmeDirectory.Meta.TermsOfService}");
                Program.Log("Enter email address for user: ");
                var userContact = Console.ReadLine();
                try
                {
                    account = await acmeClient.RegisterAsync(new[] { "mailto:" + userContact });
                }
                catch (Exception ex)
                {
                    Program.LogLine($"An error occured while registering user. {ex.Message}");
                    throw;
                }
            }
        }

        public async Task<Order> ValidateDomains()
        {
            IEnumerable<OrderIdentifier> domains = options.Domains.Select(domain => new OrderIdentifier { Type = ChallengeType.DNSChallenge, Value = domain });
            Order order = await NewOrderAsync(acmeClient, domains);

            // todo: save order identifier
            Program.LogLine($"Order location: {order.Location}");

            Uri[] validations = order.Authorizations;
            var dnsRecords = new List<string>(order.Authorizations.Length);
            IEnumerable<AuthorizationChallengeResponse> auths = await RetrieveAuthz(acmeClient, validations);
            foreach (AuthorizationChallengeResponse item in auths)
            {
                Program.LogLine($"Processing validations for {item.Identifier.Value}");

                if (item.Status == ACMEStatus.Valid)
                {
                    Program.LogLine("Domain already validated succesfully.");
                    continue;
                }

                AuthorizationChallenge validChallenge = item.Challenges.Where(challenge => challenge.Status == ACMEStatus.Valid).FirstOrDefault();
                if (validChallenge != null)
                {
                    Program.LogLine("Found a valid challenge, skipping domain.", true);
                    Program.LogLine(validChallenge.Type, true);
                    continue;
                }

                // limit to DNS challenges, as we can handle them with Cloudflare.
                AuthorizationChallenge dnsChallenge = item.Challenges.FirstOrDefault(x => x.Type == "dns-01");
                Program.LogLine($"Got challenge for {item.Identifier.Value}");
                var recordId = await AddDNSEntry(item.Identifier.Value, dnsChallenge.AuthorizationToken);
                dnsRecords.Add(recordId);

                // validate the DNS record is accessible.
                await ValidateChallengeCompletion(dnsChallenge, item.Identifier.Value);
                AuthorizationChallengeResponse c = await CompleteChallenge(acmeClient, dnsChallenge);

                while (c.Status == ACMEStatus.Pending)
                {
                    await Task.Delay(5000);
                    c = await acmeClient.GetAuthorizationChallengeAsync(dnsChallenge.Url);
                }

                if (c.Status == ACMEStatus.Valid)
                {
                    // no reason to keep going, we have one succesfull challenge!
                    continue;
                }
            }

            var failedAuthorizations = new List<string>();
            foreach (Uri challenge in order.Authorizations)
            {
                AuthorizationChallengeResponse c;
                do
                {
                    await Task.Delay(5000);
                    c = await acmeClient.GetAuthorizationChallengeAsync(challenge);
                }
                while (c == null || c.Status == ACMEStatus.Pending);

                if (c.Status == ACMEStatus.Invalid)
                {
                    failedAuthorizations.Add(c.Identifier.Value);
                }
            }

            if (failedAuthorizations.Any())
            {
                throw new Exception($"Failed to authorize the following domains {string.Join(',', failedAuthorizations)}.");
            }

            return order;
        }

        public async Task<Order> ValidateOrder(Order order)
        {
            order = await acmeClient.UpdateOrderAsync(order);
            Program.LogLine($"Order status:{order.Status}", true);

            while (order.Status == ACMEStatus.Processing)
            {
                await Task.Delay(500);
                Program.LogLine("Order status = processing; updating..", true);
                order = await acmeClient.UpdateOrderAsync(order);
            }

            if (order.Status == ACMEStatus.Ready)
            {
                Program.LogLine("Order succesfully processed.");
                return order;
            }

            throw new Exception($"Order failed to process {order.Status}, {order.Error.Detail}");
        }

        public async Task RetrieveCertificates(Order order)
        {
            var certKey = new RSACryptoServiceProvider(4096);
            Order certOrder = await acmeClient.RequestCertificateAsync(order, certKey);
            while (certOrder.Status == ACMEStatus.Processing)
            {
                await Task.Delay(500);
                Program.LogLine("Order status = processing; updating..", true);
                certOrder = await acmeClient.UpdateOrderAsync(certOrder);
            }

            if (certOrder.Status != ACMEStatus.Valid)
            {
                throw new Exception($"Failed to order certificates with {certOrder.Status}, {certOrder.Error.Detail}");
            }

            X509Certificate2 cert = await acmeClient.GetCertificateAsync(certOrder);
            var certdata = cert.Export(X509ContentType.Cert);
            var publicKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.crt";
            File.WriteAllBytes(publicKeyFilename, certdata);
            Program.LogLine($"Public certificate written to file {publicKeyFilename}");

            // combine the two!
            X509Certificate2 properCert = cert.CopyWithPrivateKey(certKey);

            Program.LogLine("Enter password to secure PFX");
            System.Security.SecureString password = PasswordInput.ReadPassword();
            var pfxData = properCert.Export(X509ContentType.Pfx, password);

            var privateKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.pfx";
            File.WriteAllBytes(privateKeyFilename, pfxData);
            Program.LogLine($"Private certificate written to file {privateKeyFilename}");
        }

        /// <summary>
        /// Get the root (domain.tld) from a given domain (eg: subdomain.domain.tld)
        /// </summary>
        /// <param name="domain">Target domain</param>
        /// <returns>A string with the root domain incl the top-level-domain</returns>
        private static string GetRootDomain(string domain)
        {
            var parts = domain.ToLowerInvariant().Split('.');
            return string.Join('.', parts.TakeLast(2));
        }

        private static string FixFilename(string filename)
        {
            return filename.Replace("*", "");
        }

        private static async Task<Order> NewOrderAsync(ACMEClient acmeClient, IEnumerable<OrderIdentifier> domains)
        {
            Order result = await acmeClient.OrderAsync(domains);
            return result;
        }

        private static async Task<IEnumerable<AuthorizationChallengeResponse>> RetrieveAuthz(ACMEClient acmeClient, Uri[] uris)
        {
            var challenges = new List<AuthorizationChallengeResponse>();
            foreach (Uri uri in uris)
            {
                try
                {
                    AuthorizationChallengeResponse result = await acmeClient.GetAuthorizationChallengeAsync(uri);
                    challenges.Add(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return challenges;
        }

        private static async Task<AuthorizationChallengeResponse> CompleteChallenge(ACMEClient acmeClient, AuthorizationChallenge challenge)
        {
            return await acmeClient.CompleteChallengeAsync(challenge.Url, challenge.Token, challenge.AuthorizationToken);
        }

        private async Task<string> AddDNSEntry(string domain, string value)
        {
            var dnsRecordName = $"_acme-challenge.{domain}";

            Program.LogLine($"Adding TXT entry {dnsRecordName} with value {value}", true);
            Zone cloudflareZone = CloudflareZones.GetValueOrDefault(GetRootDomain(domain));

            DNSRecord dnsRecord;
            try
            {
                dnsRecord = await cloudflareClient.ZoneDNSSettingsClient.CreateRecordAsync(cloudflareZone.Id, dnsRecordName, DNSRecordType.TXT, value, 3600);
                Program.LogLine($"Added entry has id {dnsRecord.Id}", true);
                return dnsRecord.Id;
            }
            catch (CloudflareException exception) when (exception.Errors[0].Code == "81057")
            {
                Program.LogLine($"The DNS entry already exists. Ignoring {domain}/{value}");
                return string.Empty;
            }
        }

        private static async Task ValidateChallengeCompletion(AuthorizationChallenge challenge, string domainName)
        {
            var maxRetries = 50;
            var delay = TimeSpan.FromSeconds(5);
            if (challenge.Type != "dns-01")
            {
                throw new Exception("Invalid challenge type.");
            }

            Program.LogLine($"Trying to validate an entry exists for _acme-challenge.{domainName} every {delay} for a maximum of {maxRetries * delay}", true);
            for (var i = 0; i < maxRetries; i++)
            {
                var lookup = new LookupClient(IPAddress.Parse("1.1.1.1"));
                IDnsQueryResponse result = await lookup.QueryAsync($"_acme-challenge.{domainName}", QueryType.TXT);
                TxtRecord record = result.Answers.TxtRecords().Where(txt => txt.Text.Contains(challenge.AuthorizationToken)).FirstOrDefault();
                if (record != null)
                {
                    Program.LogLine($"Succesfully validated a DNS entry exists for {domainName}.", true);
                    return;
                }

                await Task.Delay(delay);
            }

            throw new Exception($"Failed to validate {domainName}");
        }
    }
}
