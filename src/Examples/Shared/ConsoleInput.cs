namespace Kenc.ACMELib.Examples.Shared
{
    using System;
    using System.Linq;

    public static class ConsoleInput
    {

        public static readonly string[] NegativePromptAnswers = ["n", "no"];
        public static readonly string[] PositivePromptAnswers = ["y", "yes"];
        public static readonly string[] CombinedPromptAnswers = [.. NegativePromptAnswers, .. PositivePromptAnswers];

        public static string Prompt(string text, string[] options)
        {
            Console.WriteLine(text);
            Console.Write($"[{string.Join('/', options)}]");

            while (true)
            {
                var res = Console.ReadLine();
                if (options.Contains(res, StringComparer.OrdinalIgnoreCase))
                {
                    return res;
                }

                Console.WriteLine("Invalid options. Try again or hit ctrl+c to abort.");
            }
        }
    }
}
