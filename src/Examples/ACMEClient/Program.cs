namespace ACMEClient.Example
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEEntities;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.Exceptions.API;

    class Program
    {
        private static readonly string keyPath = "acmeuser.key";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Kenc.ACMEClient example");
            Console.WriteLine("USE AT OWN RISK");
            Console.WriteLine("This program uses the LetsEncrypt STAGING environment by default");
            Console.WriteLine("WARNING: Encryption key is stored UNPROTECTED in bin folder");
            Console.WriteLine("============================================================\n\n");

            // RSA service provider
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048);

            Console.WriteLine($"Looking for key {keyPath}");
            var existingKey = File.Exists(keyPath);
            if (!existingKey)
            {
                Console.WriteLine("Key not found - generating");
                var exportKey = rsaCryptoServiceProvider.ExportCspBlob(true);
                var strKey = Convert.ToBase64String(exportKey);

                File.WriteAllText(keyPath, strKey);
            }

            var key = Convert.FromBase64String(File.ReadAllText(keyPath));
            rsaCryptoServiceProvider.ImportCspBlob(key);

            var rsaKey = RSA.Create(rsaCryptoServiceProvider.ExportParameters(true));

            var acmeClient = new ACMEClient(ACMEEnvironment.StagingV2, rsaKey, new RestClientFactory());
            var directory = await acmeClient.InitializeAsync();

            Account account = null;
            if (existingKey)
            {
                Console.WriteLine("Validating if user exists with existing key");
                try
                {
                    account = await acmeClient.GetAccountAsync();
                }
                catch (AccountDoesNotExistException exception)
                {
                    Console.WriteLine(exception.Message);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Exception encounted while looking up client: {exception.Message}");
                }

                if (account != null)
                {
                    Console.WriteLine($"Using previously created account {account.Id}");
                }
                else
                {
                    Console.WriteLine("Couldn't retrieve existing account. Creating new account.");
                }
            }

            if (account == null)
            {
                Console.WriteLine("Creating user..");
                Console.WriteLine("By creating this user, you acknowledge the terms of service: {0}", directory.Meta.TermsOfService);
                Console.Write("Enter email address for user: ");
                var userContact = Console.ReadLine();
                try
                {
                    account = await acmeClient.RegisterAsync(new[] { "mailto:" + userContact });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured while registering user. {0}", ex.Message);
                    throw;
                }
            }

            Console.WriteLine("Enter domain to validate and request certificate for:");
            var domainName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(domainName))
            {
                Console.WriteLine("Invalid domain aborting.");
                return;
            }

            var domainTask = OrderDomains(acmeClient, domainName);
            domainTask.Wait();

            // enumerate all certs
            var certificateFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.crt").ToList();
            Console.WriteLine($"Revoking certificates: {string.Join(',', certificateFiles)}");

            var foo = HandleConsoleInput("Continue?", new[] { "y", "yes", "n", "no" }, false).ToLower();
            if (foo == "y" || foo == "yes")
            {
                var certificates = certificateFiles.Select(path => X509Certificate2.CreateFromCertFile(path));
                var revocationTasks = RevokeCertificates(acmeClient, certificates, RevocationReason.Superseded).ToArray();
                Task.WaitAll(revocationTasks);
                Console.WriteLine("Completed");
            }
        }

        static IEnumerable<Task> RevokeCertificates(ACMEClient acmeClient, IEnumerable<X509Certificate> certificates, RevocationReason reason)
        {
            foreach (var certificate in certificates)
            {
                yield return acmeClient.RevokeCertificateAsync(certificate, RevocationReason.Superseded);
            }
        }

        static async Task OrderDomains(ACMEClient acmeClient, params string[] domainNames)
        {
            var domains = domainNames.Select(domain => new OrderIdentifier { Type = ChallengeType.DNSChallenge, Value = domain });
            Uri[] validations = null;
            Order order = null;
            while (order == null)
            {
                order = await NewOrderAsync(acmeClient, domains);
                if (order == null)
                {
                    Console.WriteLine("Failed.. retrying");
                    await Task.Delay(5000);
                }
            }

            // todo: save order identifier
            Console.WriteLine($"Order location: {order.Location}");

            validations = order.Authorizations;
            var auths = await RetrieveAuthz(acmeClient, validations);
            foreach (var item in auths)
            {
                foreach (var challenge in item.Challenges)
                {
                    Console.WriteLine($"Challenge: {challenge.Url}");
                    Console.WriteLine($"Type: {challenge.Type}");
                    Console.WriteLine($"Token: {challenge.Token}");
                    Console.WriteLine($"Value: {challenge.AuthorizationToken}");

                    if (challenge.Type == "http-01")
                    {
                        File.WriteAllText(challenge.Token, challenge.AuthorizationToken);
                        Console.WriteLine($"File saved as: {challenge.Token} in working directory.");
                        Console.WriteLine($"Please upload the file to {domainNames.First()}/.well-known/acme-challenge/{challenge.Token}");
                    }
                    else if (challenge.Type == "dns-01")
                    {
                        Console.WriteLine($"Please create a text entry in the DNS records for each domain in {string.Join(',', domainNames)} using {challenge.Token}");
                    }
                    else
                    {
                        Console.WriteLine($"Unknown challenge type encountered '{challenge.Type}'. Please handle accourdingly.");
                    }

                    var result = HandleConsoleInput("Challenge completed? [y/n]", new[] { "y", "yes", "n", "no" });
                    if (result == "y" || result == "yes")
                    {
                        Console.WriteLine("Validating challenge");
                        var validation = await ValidateChallengeCompletion(challenge, domainNames);
                        if (validation.Any(validationItem => !validationItem.Value))
                        {
                            Console.WriteLine($"The following domains failed validation: " +
                                $"{string.Join(',', validation.Where(vItem => !vItem.Value).Select(vItem => vItem.Key))}");
                        }

                        if (validation.Any(validationItem => validationItem.Value))
                        {
                            var c = await CompleteChallenge(acmeClient, challenge, challenge.AuthorizationToken);
                            if (c != null)
                            {
                                Console.WriteLine(c.Status);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Skipping challenge");
                    }
                }
            }

            // todo: add a proper check to see if all validations passed.

            order = await acmeClient.UpdateOrderAsync(order);
            Console.WriteLine($"Order status:{order.Status}");

            // todo: if(order.Status == failed...)

            while (order.Status == Order.Processing)
            {
                Thread.Sleep(500);
                Console.WriteLine("Order status = processing; updating..");
                order = await acmeClient.UpdateOrderAsync(order);
            }

            var certKey = new RSACryptoServiceProvider(4096);
            SaveRSAKeyToFile(certKey, $"{order.Identifiers[0].Value}.key");

            Order certOrder = null;
            try
            {

                certOrder = await acmeClient.RequestCertificateAsync(order, certKey);
                while (certOrder.Status == Order.Processing)
                {
                    Thread.Sleep(500);
                    Console.WriteLine("Order status = processing; updating..");
                    certOrder = await acmeClient.UpdateOrderAsync(certOrder);
                }

                Console.WriteLine(certOrder.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var cert = await acmeClient.GetCertificateAsync(certOrder);
            var certdata = cert.Export(X509ContentType.Cert);
            var publicKeyFilename = $"{certOrder.Identifiers[0].Value}.crt";
            File.WriteAllBytes(publicKeyFilename, certdata);
            Console.WriteLine($"Public certificate written to file {publicKeyFilename}");

            // combine the two!
            var properCert = cert.CopyWithPrivateKey(certKey);
            var pfxData = cert.Export(X509ContentType.Pfx);

            var privateKeyFilename = $"{certOrder.Identifiers[0].Value}.pfx";
            File.WriteAllBytes(privateKeyFilename, pfxData);
            Console.WriteLine($"Private certificate written to file {privateKeyFilename}");
        }

        static async Task<Dictionary<string, bool>> ValidateChallengeCompletion(AuthorizationChallenge challenge, IEnumerable<string> domainNames)
        {
            if (challenge.Type == "http-01")
            {
                return await ValidateHttpChallenge(challenge.Token, challenge.AuthorizationToken, domainNames);

            }
            else if (challenge.Type == "dns-01")
            {
                Console.WriteLine($"Please create a text entry in the DNS records for each domain in {string.Join(',', domainNames)} using {challenge.Token}");
                return domainNames.ToDictionary(domain => domain, domain => false);
            }
            else
            {
                Console.WriteLine($"Unknown challenge type encountered '{challenge.Type}'. Please validate yourself.");
                // can't validate, return success.
                return domainNames.ToDictionary(domain => domain, domain => true);
            }
        }

        static async Task<Dictionary<string, bool>> ValidateChallenges(IEnumerable<AuthorizationChallengeResponse> challenges, IEnumerable<string> domains)
        {
            var challengeValidation = domains.ToDictionary(domain => domain, domain => false);
            //validate challenges
            Console.WriteLine("Validating challenge completion");
            foreach (var item in challenges)
            {
                foreach (var challenge in item.Challenges)
                {

                    if (challenge.Type == "http-01")
                    {
                        // do a get request against url of each domain
                        var validationResults = await ValidateHttpChallenge(challenge.Token, challenge.AuthorizationToken, domains);
                        foreach (var domainResult in validationResults)
                        {
                            if (domainResult.Value)
                            {
                                // update all successfull validations
                                challengeValidation[domainResult.Key] = domainResult.Value;
                            }
                        }

                        if (validationResults.Any(domainResult => !domainResult.Value))
                        {
                            Console.WriteLine("Failed to succesfully validate all domains.");
                        }

                        Console.WriteLine($"Succesfully validated all domains fo challenge {challenge.Url}");
                    }
                    else if (challenge.Type == "dns-01")
                    {
                        // validate 
                    }
                }
            }

            return challengeValidation;
        }

        static async Task<Dictionary<string, bool>> ValidateHttpChallenge(string token, string expectedValue, IEnumerable<string> domains)
        {
            var result = domains.ToDictionary(item => item, item => false);
            foreach (var domain in domains)
            {
                // use https?
                var url = $"http://{domain}/.well-known/acme-challenge/{token}";
                var httpRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                try
                {
                    var response = (HttpWebResponse)(await httpRequest.GetResponseAsync());
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"{url} Received unexpected status code: {response.StatusCode}");
                        continue;
                    }

                    using (var stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var received = streamReader.ReadToEnd();
                            if (string.Compare(received, expectedValue, StringComparison.Ordinal) != 0)
                            {
                                Console.WriteLine($"{url} responded with unexpected value.");
                                Console.WriteLine(received);
                                Console.WriteLine(expectedValue);
                            }
                            else
                            {
                                result[domain] = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{url} Failed: {ex.Message}");

                }
            }
            return result;
        }

        static string HandleConsoleInput(string prompt, string[] acceptedResponses, bool caseSensitive = false)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var input = Console.ReadLine();
                if (!caseSensitive)
                {
                    input = input.ToLowerInvariant();
                }

                if (acceptedResponses.Contains(input))
                {
                    return input;
                }
            }
        }

        private static async Task<Order> NewOrderAsync(Kenc.ACMELib.ACMEClient acmeClient, IEnumerable<OrderIdentifier> domains)
        {
            try
            {
                var result = await acmeClient.OrderAsync(domains);
                return result;
            }
            catch (ACMEException ex)
            {
                Console.WriteLine(ex.Descriptor);
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType());
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static async Task<IEnumerable<AuthorizationChallengeResponse>> RetrieveAuthz(Kenc.ACMELib.ACMEClient acmeClient, Uri[] uris)
        {
            var challenges = new List<AuthorizationChallengeResponse>();
            foreach (var uri in uris)
            {
                try
                {
                    var result = await acmeClient.GetAuthorizationChallengeAsync(uri);
                    challenges.Add(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return challenges;
        }

        private static async Task<AuthorizationChallengeResponse> CompleteChallenge(Kenc.ACMELib.ACMEClient acmeClient, AuthorizationChallenge challenge, string value)
        {
            try
            {
                return await acmeClient.CompleteChallengeAsync(challenge.Url, challenge.Token, value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }

        private static async Task<string> DownloadContentAsync(Uri url)
        {
            using (WebClient client = new WebClient())
            {
                return await client.DownloadStringTaskAsync(url);
            }
        }

        private static void SaveRSAKeyToFile(RSACryptoServiceProvider rsaCryptoServiceProvider, string filename)
        {
            byte[] key = rsaCryptoServiceProvider.ExportCspBlob(true);
            var strKey = Convert.ToBase64String(key);
            File.WriteAllText(filename, strKey);
        }
    }
}