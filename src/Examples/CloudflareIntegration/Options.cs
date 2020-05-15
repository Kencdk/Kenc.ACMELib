namespace CloudflareIntegration
{
    using System.Collections.Generic;
    using CommandLine;

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('d', "domains", Required = true, HelpText = "Domains to request.")]
        public IEnumerable<string> Domains { get; set; }

        [Option('a', "apikey", Required = true, HelpText = "Api key for Cloudflare.")]
        public string ApiKey { get; set; }

        [Option('u', "username", Required = true, HelpText = "Username for Cloudflare.")]
        public string Username { get; set; }

        [Option('e', "Environment", Required = false, HelpText = "ACME Environment")]
        public AcmeEnvironment Environment { get; set; } = AcmeEnvironment.StagingV2;

        [Option('k', "key", Required = false, HelpText = "RSA key to use for authenticating with ACME")]
        public string Key { get; set; }
    }
}
