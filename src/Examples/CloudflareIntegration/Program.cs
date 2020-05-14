namespace CloudflareIntegration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using CommandLine;

    class Program
    {
        static Options options;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Kenc.ACMEClient example");
            Console.WriteLine("USE AT OWN RISK");
            Console.WriteLine("This program uses the LetsEncrypt STAGING environment by default");
            Console.WriteLine("WARNING: Encryption key is stored UNPROTECTED in bin folder");
            Console.WriteLine("============================================================\n\n");

            (await Parser.Default.ParseArguments<Options>(args)
              .WithParsedAsync(RunOptions))
              .WithNotParsed(HandleParseError);
        }

        static async Task RunOptions(Options options)
        {
            if (!string.IsNullOrEmpty(options.Key) && !File.Exists(options.Key))
            {
                throw new FileNotFoundException($"{options.Key} couldn't be found.");
            }

            Program.options = options;

            var orderDomains = new OrderDomains(options);
            await orderDomains.ValidateCloudflareConnection();
            await orderDomains.ValidateACMEConnection();
            var order = await orderDomains.ValidateDomains();
            order = await orderDomains.ValidateOrder(order);
            await orderDomains.RetrieveCertificates(order);

        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            //handle errors
        }

        public static void Log(string message, bool verbose = false)
        {
            if (verbose && !options.Verbose)
            {
                return;
            }

            Console.Write(message);
        }

        public static void LogLine(string message, bool verbose = false)
        {
            if (verbose && !options.Verbose)
            {
                return;
            }

            Console.WriteLine(message);
        }
    }
}
