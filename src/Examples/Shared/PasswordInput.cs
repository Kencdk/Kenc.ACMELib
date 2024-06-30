namespace Kenc.ACMELib.Examples.Shared
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Based on https://stackoverflow.com/questions/3404421/password-masking-console-application
    /// </summary>
    public partial class PasswordInput
    {
        private enum StdHandle
        {
            Input = -10,
            Output = -11,
            Error = -12,
        }

        private enum ConsoleMode
        {
            ENABLE_ECHO_INPUT = 4
        }

        private const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
        private static readonly int[] Filtered = [0, 27 /* escape */, 9 /*tab*/, 10 /* line feed */];

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetStdHandle(StdHandle nStdHandle);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

        public static SecureString ReadPassword()
        {
            var stdInputHandle = GetStdHandle(StdHandle.Input);
            if (stdInputHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("No console input");
            }

            if (!GetConsoleMode(stdInputHandle, out var previousConsoleMode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not get console mode.");
            }

            // disable console input echo
            if (!SetConsoleMode(stdInputHandle, dwMode: previousConsoleMode & ~(int)ConsoleMode.ENABLE_ECHO_INPUT))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not disable console input echo.");
            }

            var secureString = new SecureString();

            char character;
            while ((character = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (((character == BACKSP) || (character == CTRLBACKSP))
                    && (secureString.Length > 0))
                {
                    secureString.RemoveAt(secureString.Length - 1);
                }
                else if (((character == BACKSP) || (character == CTRLBACKSP)) && (secureString.Length == 0))
                {
                }
                else if (Filtered.Contains(character))
                {
                }
                else
                {
                    Console.Write('*');
                    secureString.AppendChar(character);
                }
            }

            // reset console mode to previous
            return !SetConsoleMode(stdInputHandle, previousConsoleMode)
                ? throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not reset console mode.")
                : secureString;
        }
    }
}
