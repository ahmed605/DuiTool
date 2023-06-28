using System;
using System.Collections.Generic;
using System.Text;

namespace DuiTool.Helpers
{
    public static class ConsoleEx
    {
        public static void WriteError(string message = "Invalid input")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[x] " + message);
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[!] " + message);
            Console.ResetColor();
        }

        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[!] " + message);
            Console.ResetColor();
        }

        public static void WriteMessage(string value)
        {
            Console.WriteLine("[*] " + value);
        }

        public static void WriteEmptyLine()
        {
            Console.WriteLine(String.Empty);
        }

        public static void WriteLine(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
        }

        public static void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }
}
