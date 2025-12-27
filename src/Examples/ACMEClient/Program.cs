namespace ACMEClient.Example
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib;
    using Kenc.ACMELib.ACMEObjects;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Examples.Shared;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.Exceptions.API;

    internal class Program
    {
        private static readonly string keyPath = "acmeuser.key";

        private static readonly HttpClient httpClient = new();

        private static async Task Main()
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

            var acmeClient = new ACMEClient(new Uri(ACMEEnvironment.StagingV2), rsaKey, new HttpClient());
            ACMEDirectory directory = await acmeClient.InitializeAsync();

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
                    account = await acmeClient.RegisterAsync(["mailto:" + userContact]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured while registering user. {0}", ex.Message);
                    throw;
                }
            }

            Console.WriteLine("Enter domain(s) to validate and request certificate(s) for (wildcard? add wildcard and tld comma separated):");
            var domainNames = Console.ReadLine().Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (domainNames.Length == 0)
            {
                Console.WriteLine("Invalid domain aborting.");
                return;
            }

            Task domainTask = OrderDomains(acmeClient, domainNames);
            domainTask.Wait();

            // enumerate all certs
            var certificateFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.crt").ToList();
            Console.WriteLine($"Revoking certificates: {string.Join(',', certificateFiles)}");

            var foo = HandleConsoleInput("Continue?", ["y", "yes", "n", "no"], false).ToLower();
            if (foo is "y" or "yes")
            {
                IEnumerable<X509Certificate> certificates = certificateFiles.Select(path => X509CertificateLoader.LoadCertificateFromFile(path));
                foreach (X509Certificate certificate in certificates)
                {
                    try
                    {
                        await acmeClient.RevokeCertificateAsync(certificate, RevocationReason.Superseded);
                        Console.WriteLine($"{certificate.Subject} revoked.");
                    }
                    catch (ACMEException exception) when (exception.Descriptor == "urn:ietf:params:acme:error:alreadyRevoked")
                    {
                        Console.WriteLine(exception.Message);
                    }
                    catch (ACMEException exception)
                    {
                        Console.WriteLine(exception.Message);
                        throw;
                    }
                }
                Console.WriteLine("Completed");
            }
        }

        private static async Task OrderDomains(ACMEClient acmeClient, params string[] domainNames)
        {
            IEnumerable<OrderIdentifier> domains = domainNames.Select(domain => new OrderIdentifier { Type = ChallengeType.DNSChallenge, Value = domain });
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
            IEnumerable<AuthorizationChallengeResponse> auths = await RetrieveAuthz(acmeClient, validations);
            foreach (AuthorizationChallengeResponse item in auths)
            {
                if (item.Status == ACMEStatus.Valid)
                {
                    Console.WriteLine("Domain already validated succesfully.");
                    continue;
                }

                AuthorizationChallenge validChallenge = item.Challenges.Where(challenge => challenge.Status == ACMEStatus.Valid).FirstOrDefault();
                if (validChallenge != null)
                {
                    Console.WriteLine("Found a valid challenge, skipping domain.");
                    Console.WriteLine(validChallenge.Type);
                    continue;
                }

                IEnumerable<AuthorizationChallenge> applicableChallenges = item.Wildcard ? item.Challenges.Where(x => x.Type == "dns-01") :
                    item.Challenges;

                foreach (AuthorizationChallenge challenge in applicableChallenges)
                {
                    Console.WriteLine($"Status: {challenge.Status}");
                    Console.WriteLine($"Challenge: {challenge.Url}");
                    Console.WriteLine($"Type: {challenge.Type}");
                    Console.WriteLine($"Token: {challenge.Token}");
                    Console.WriteLine($"Value: {challenge.AuthorizationToken}");

                    if (challenge.Type == "http-01")
                    {
                        File.WriteAllText(challenge.Token, challenge.AuthorizationToken);
                        Console.WriteLine($"File saved as: {challenge.Token} in working directory.");
                        Console.WriteLine($"Please upload the file to {item.Identifier.Value}/.well-known/acme-challenge/{challenge.Token}");
                    }
                    else if (challenge.Type == "dns-01")
                    {
                        Console.WriteLine($"Please create a text entry for _acme-challenge.{item.Identifier.Value} with value: {challenge.AuthorizationToken}");
                    }
                    else
                    {
                        Console.WriteLine($"Unknown challenge type encountered '{challenge.Type}'. Please handle accourdingly.");
                    }

                    var result = HandleConsoleInput("Challenge completed? [y/n]", ["y", "yes", "n", "no"]);
                    if (result is "y" or "yes")
                    {
                        Console.WriteLine("Validating challenge");
                        var validation = await ValidateChallengeCompletion(challenge, item.Identifier.Value);
                        if (validation)
                        {
                            AuthorizationChallengeResponse c = await CompleteChallenge(acmeClient, challenge, challenge.AuthorizationToken);
                            while (c.Status == ACMEStatus.Pending)
                            {
                                await Task.Delay(5000);
                                c = await acmeClient.GetAuthorizationChallengeAsync(challenge.Url);
                            }

                            Console.WriteLine($"Challenge Status: {c.Status}");
                            if (c.Status == ACMEStatus.Valid)
                            {
                                // no reason to keep going, we have one succesfull challenge!
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Validation failed for {item.Identifier.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Skipping challenge");
                    }
                }
            }

            foreach (Uri challenge in order.Authorizations)
            {
                AuthorizationChallengeResponse c;
                do
                {
                    c = await acmeClient.GetAuthorizationChallengeAsync(challenge);
                }
                while (c == null || c.Status == ACMEStatus.Pending);

                if (c.Status == ACMEStatus.Invalid)
                {
                    Console.WriteLine($"Failed to validate domain {c.Identifier.Value}. Aborting");
                    return;
                }
            }

            order = await acmeClient.UpdateOrderAsync(order);
            Console.WriteLine($"Order status:{order.Status}");

            while (order.Status == ACMEStatus.Processing)
            {
                Thread.Sleep(500);
                Console.WriteLine("Order status = processing; updating..");
                order = await acmeClient.UpdateOrderAsync(order);
            }

            var certKey = new RSACryptoServiceProvider(4096);
            SaveRSAKeyToFile(certKey, $"{FixFilename(order.Identifiers[0].Value)}.key");

            Order certOrder = null;
            try
            {
                certOrder = await acmeClient.RequestCertificateAsync(order, certKey);
                while (certOrder.Status == ACMEStatus.Processing)
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
                return;
            }

            X509Certificate2 cert = await acmeClient.GetCertificateAsync(certOrder);
            var certdata = cert.Export(X509ContentType.Cert);
            var publicKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.crt";
            File.WriteAllBytes(publicKeyFilename, certdata);
            Console.WriteLine($"Public certificate written to file {publicKeyFilename}");

            // combine the two!
            X509Certificate2 properCert = cert.CopyWithPrivateKey(certKey);

            Console.WriteLine("Enter password to secure PFX");
            System.Security.SecureString password = PasswordInput.ReadPassword();
            var pfxData = properCert.Export(X509ContentType.Pfx, password);

            var privateKeyFilename = $"{FixFilename(certOrder.Identifiers[0].Value)}.pfx";
            File.WriteAllBytes(privateKeyFilename, pfxData);
            Console.WriteLine($"Private certificate written to file {privateKeyFilename}");
        }

        private static string FixFilename(string filename)
        {
            return filename.Replace("*", "");
        }

        private static async Task<bool> ValidateChallengeCompletion(AuthorizationChallenge challenge, string domainName)
        {
            if (challenge.Type == "http-01")
            {
                return await ValidateHttpChallenge(challenge.Token, challenge.AuthorizationToken, domainName);
            }
            else if (challenge.Type == "dns-01")
            {
                // can't validate, return success.
                Console.WriteLine($"Please validate a text entry exists for _acme-challenge.{domainName} with value {challenge.AuthorizationToken}");
                return true;
            }
            else
            {
                // can't validate, return success.
                Console.WriteLine($"Unknown challenge type encountered '{challenge.Type}'. Please validate yourself.");
                return true;
            }
        }

        private static async Task<bool> ValidateHttpChallenge(string token, string expectedValue, string domain)
        {
            var domainUrl = domain.Replace("*", "");
            var url = new Uri($"http://{domainUrl}/.well-known/acme-challenge/{token}");
            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{url} Received unexpected status code: {response.StatusCode}");
                    return false;
                }

                var responseStr = await response.Content.ReadAsStringAsync();
                if (string.Compare(responseStr, expectedValue, StringComparison.Ordinal) != 0)
                {
                    Console.WriteLine($"{url} responded with unexpected value.");
                    Console.WriteLine(responseStr);
                    Console.WriteLine(expectedValue);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{url} Failed: {ex.Message}");
                return false;
            }
        }

        private static string HandleConsoleInput(string prompt, string[] acceptedResponses, bool caseSensitive = false)
        {
            while (true)
            {
                Console.WriteLine(prompt);
                var input = Console.ReadLine();

                if (acceptedResponses.Contains(input, caseSensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal))
                {
                    return input;
                }
            }
        }

        private static async Task<Order> NewOrderAsync(ACMEClient acmeClient, IEnumerable<OrderIdentifier> domains)
        {
            try
            {
                Order result = await acmeClient.OrderAsync(domains);
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

        private static async Task<AuthorizationChallengeResponse> CompleteChallenge(ACMEClient acmeClient, AuthorizationChallenge challenge, string value)
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

        private static void SaveRSAKeyToFile(RSACryptoServiceProvider rsaCryptoServiceProvider, string filename)
        {
            var key = rsaCryptoServiceProvider.ExportCspBlob(true);
            var strKey = Convert.ToBase64String(key);
            File.WriteAllText(filename, strKey);
        }
    }
}