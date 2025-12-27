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
    using System.Text;
    using System.Threading.Tasks;
    using DnsClient;
    using DnsClient.Protocol;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEObjects;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Examples.Shared;
    using Kenc.ACMELib.Exceptions.API;
    using Kenc.Cloudflare.Core;
    using Kenc.Cloudflare.Core.Clients;
    using Kenc.Cloudflare.Core.Clients.Enums;
    using Kenc.Cloudflare.Core.Entities;
    using Kenc.Cloudflare.Core.Exceptions;
    using Microsoft.Extensions.Configuration;
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
            var cfgBuilder = new ConfigurationBuilder();
            cfgBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Username"] = options.Username,
                ["ApiKey"] = options.ApiKey,
                ["Endpoint"] = CloudflareAPIEndpoint.V4Endpoint.ToString(),
            });
            IConfiguration cfg = cfgBuilder.Build();

            this.options = options;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            serviceCollection.AddCloudflareClient(cfg);
            ServiceProvider services = serviceCollection.BuildServiceProvider();
            cloudflareClient = services.GetRequiredService<ICloudflareClient>();

            // RSA service provider
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048);
            if (string.IsNullOrEmpty(options.Key))
            {
                Program.LogLine("Generating new key for ACME.");
                var exportKey = rsaCryptoServiceProvider.ExportCspBlob(true);
                var strKey = Convert.ToBase64String(exportKey);

                if (File.Exists("acmekey.key"))
                {

                    var result = ConsoleInput.Prompt("WARNING - acmekey.key already exists - do you wish to overwrite?", ["y", "yes", "n", "no"]);
                    if (!ConsoleInput.PositivePromptAnswers.Contains(result, StringComparer.OrdinalIgnoreCase))
                    {
                        // user doesn't wish to overwrite the key. Abort
                        throw new Exception("User aborted");
                    }
                }

                File.WriteAllText("acmekey.key", strKey);
            }
            else
            {
                var key = Convert.FromBase64String(File.ReadAllText(options.Key));
                rsaCryptoServiceProvider.ImportCspBlob(key);
            }

            var rsaKey = RSA.Create(rsaCryptoServiceProvider.ExportParameters(true));
            acmeClient = new ACMEClient(
                new Uri(options.Environment == AcmeEnvironment.ProductionV2 ? ACMEEnvironment.ProductionV2 : ACMEEnvironment.StagingV2),
                rsaKey,
                new HttpClient());
        }

        public async Task ValidateCloudflareConnection()
        {
            var cleanedupDomains = options.Domains.Select(GetRootDomain)
                .Distinct()
                .ToList();

            // list all zones.
            try
            {
                IList<Zone> cloudflareZones = await cloudflareClient.Zones.ListAsync();

                CloudflareZones = cloudflareZones.Where(x => cleanedupDomains.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(x => x.Name, x => x);

                var missingDomains = cleanedupDomains.Except(CloudflareZones.Keys, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (missingDomains.Count != 0)
                {
                    throw new Exception($"The following domains are not accesible in the cloudflare account. {string.Join(',', missingDomains)}");
                }
            }
            catch (CloudflareException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task ValidateACMEConnection()
        {
            acmeDirectory = await acmeClient.InitializeAsync();
            Kenc.ACMELib.ACMEObjects.Account account = null;
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
                    account = await acmeClient.RegisterAsync(["mailto:" + userContact]);
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
            if (acmeDirectory.NewAuthz != null)
            {
                Console.WriteLine("Target CA supports NewAuthz");
                Task<AuthorizationChallengeResponse>[] preAuthorizationChallenges = options.Domains.Where(x => !x.StartsWith('*'))
                    .Select(x => NewAuthorizationAsync(acmeClient, x))
                    .ToArray();

                AuthorizationChallengeResponse[] challengeResponses = await Task.WhenAll(preAuthorizationChallenges);
            }

            var domains = options.Domains.Select(domain => new OrderIdentifier { Type = ChallengeType.DNSChallenge, Value = domain }).ToList();
            Order order = await NewOrderAsync(acmeClient, domains);

            // todo: save order identifier
            Uri location = order.Location;
            Program.LogLine($"Order location: {order.Location}");


            // get challenge results.
            Uri[] validations = order.Authorizations;
            var dnsRecords = new List<string>(order.Authorizations.Length);
            List<AuthorizationChallengeResponse> auths = await RetrieveAuthz(acmeClient, validations);

            // now that we have all the authz responses - add challenges for those that needs it.
            Task<(AuthorizationChallenge dnsChallenge, string Value)>[] dnsChallenges = auths.Select(async item =>
            {
                if (item.Status == ACMEStatus.Valid)
                {
                    Program.LogLine($"{item.Identifier} already validated succesfully.");
                    return (null, null);
                }

                AuthorizationChallenge validChallenge = item.Challenges.Where(challenge => challenge.Status == ACMEStatus.Valid).FirstOrDefault();
                if (validChallenge != null)
                {
                    Program.LogLine("Found a valid challenge, skipping domain.", true);
                    Program.LogLine(validChallenge.Type, true);
                    return (null, null);
                }

                // limit to DNS challenges, as we can handle them with Cloudflare.
                AuthorizationChallenge dnsChallenge = item.Challenges.FirstOrDefault(x => x.Type == "dns-01");
                Program.LogLine($"Adding DNS entry for {item.Identifier.Value}");
                var recordId = await AddDNSEntry(item.Identifier.Value, dnsChallenge.AuthorizationToken);
                dnsRecords.Add(recordId);

                return (dnsChallenge, item.Identifier.Value);
            }).ToArray();

            (AuthorizationChallenge dnsChallenge, string Domain)[] challenges = (await Task.WhenAll(dnsChallenges))
                .Where(x => x.dnsChallenge is not null).ToArray();

            // foreach of the challenges - let's check if we can resolve them with DNS.
            Task[] completionTasks = challenges.Select(async x =>
            {
                await ValidateChallengeCompletion(x.dnsChallenge, x.Domain);
                await CompleteChallenge(acmeClient, x.dnsChallenge);
            }).ToArray();
            await Task.WhenAll(completionTasks);

            // now that we have validated that all DNS records exists AND completed the challenges - let's check if they completed succesfully
            Task<AuthorizationChallengeResponse>[] authorizationTasks = order.Authorizations.Select(async challenge =>
            {
                AuthorizationChallengeResponse c;
                do
                {
                    c = await acmeClient.GetAuthorizationChallengeAsync(challenge);

                }
                while (c == null || c.Status == ACMEStatus.Pending);

                return c;
            }).ToArray();

            AuthorizationChallengeResponse[] authorizationResults = await Task.WhenAll(authorizationTasks);
            IEnumerable<IGrouping<ACMEStatus, AuthorizationChallengeResponse>> groupedByStatus = authorizationResults.GroupBy(x => x.Status);
            foreach (IGrouping<ACMEStatus, AuthorizationChallengeResponse> group in groupedByStatus)
            {
                Console.WriteLine(group.Key);
                foreach (AuthorizationChallengeResponse item in group)
                {
                    Console.WriteLine(item.Identifier.Value);
                }
            }

            IEnumerable<IGrouping<ACMEStatus, AuthorizationChallengeResponse>> failedAuthorizations = groupedByStatus.Where(x => x.Key == ACMEStatus.Invalid);
            if (failedAuthorizations.Any())
            {
                throw new Exception($"Failed to authorize the following domains {string.Join(',', failedAuthorizations)}.");
            }
            ;

            if (order.Location == null)
            {
                // sometimes, ACME doesn't respond with the location -.-
                order.Location = location;
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

            // do we export as PFX or PEM/KEY format?
            if (options.ExportFormat == CertificateExportFormat.PFX)
            {
                Program.LogLine("Enter password to secure PFX");
                System.Security.SecureString password = PasswordInput.ReadPassword();
                var pfxData = properCert.Export(X509ContentType.Pfx, password);

                var privateKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.pfx";
                await File.WriteAllBytesAsync(privateKeyFilename, pfxData);
                Program.LogLine($"Private certificate written to file {privateKeyFilename}");
            }
            else if (options.ExportFormat == CertificateExportFormat.PEM)
            {
                var certificateBytes = cert.RawData;
                var certificatePem = PemEncoding.Write("CERTIFICATE", certificateBytes);
                var privKeyBytes = certKey.ExportPkcs8PrivateKey();
                var privKeyPem = PemEncoding.Write("PRIVATE KEY", privKeyBytes);

                var privateKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.key.pem";
                await File.WriteAllBytesAsync(privateKeyFilename, Encoding.ASCII.GetBytes(privKeyPem));

                var certificateFileName = $"{FixFilename(certOrder.Identifiers[0].Value)}.cert.pem";
                await File.WriteAllBytesAsync(certificateFileName, Encoding.ASCII.GetBytes(certificatePem));

                // build up the certificate chain and export as well 
                X509Chain ch = new();
                ch.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                ch.Build(cert);

                var stringBuilder = new StringBuilder();
                foreach (X509ChainElement element in ch.ChainElements)
                {
                    if (element.Certificate.Thumbprint.Equals(cert.Thumbprint, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var elementPem = PemEncoding.Write("CERTIFICATE", element.Certificate.RawData);
                    stringBuilder.Append(elementPem);
                    stringBuilder.AppendLine();
                }

                var certificateChainFileName = $"{FixFilename(certOrder.Identifiers[0].Value)}.chain.pem";
                await File.WriteAllTextAsync(certificateChainFileName, stringBuilder.ToString());
            }
            else
            {
                throw new InvalidOperationException("Unsupported export format defined.");
            }
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
            filename = filename.Replace("*", "");
            if (filename.StartsWith('.'))
            {
                filename = filename[1..];
            }

            return filename;
        }

        private static async Task<Order> NewOrderAsync(ACMEClient acmeClient, IEnumerable<OrderIdentifier> domains)
        {
            Order result = await acmeClient.OrderAsync(domains);
            return result;
        }

        private static async Task<AuthorizationChallengeResponse> NewAuthorizationAsync(ACMEClient acmeClient, string domain)
        {
            return await acmeClient.NewAuthorizationAsync(domain);
        }

        private static async Task<List<AuthorizationChallengeResponse>> RetrieveAuthz(ACMEClient acmeClient, Uri[] uris)
        {
            var tasks = uris.Select(x => acmeClient.GetAuthorizationChallengeAsync(x)).ToList();
            return [.. (await Task.WhenAll(tasks))];
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
            catch (CloudflareException existsAlreadyException) when (existsAlreadyException.Errors[0].Code == "81058")
            {
                // record already exists.
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
                IDnsQueryResponse result;
                try
                {
                    result = await lookup.QueryAsync($"_acme-challenge.{domainName}", QueryType.TXT);

                }
                catch (DnsResponseException dnsResponseException)
                {
                    Program.LogLine($"Exception while querying DNS: {dnsResponseException.Message}");
                    continue;
                }

                TxtRecord record = result.Answers.TxtRecords().Where(txt => txt.Text.Contains(challenge.AuthorizationToken)).FirstOrDefault();
                if (record != null)
                {
                    Program.LogLine($"Succesfully validated a DNS entry exists for {domainName}.", true);
                    return;
                }

                await Task.Delay(delay);
            }

            throw new TimeoutException($"Failed to validate {domainName}");
        }
    }
}
