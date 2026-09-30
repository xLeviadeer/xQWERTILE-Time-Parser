using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XQWERTYATimeParserC_ {
    internal static class ConsoleExtension {

        // static Console extension
        extension(Console) {

            // - Ask for Input -

            public static string? AskForInput(string message) {
                Console.Write(message);
                return Console.ReadLine();
            }

            // - If Input Yes/No -

            public static void If_InputYes(string? input, Action callback) {
                switch (input) {
                    case "y":
                    case "yes":
                        callback.Invoke();
                        break;
                }
            }

            public static void If_InputNo(string? input, Action callback) {
                switch (input) {
                    case "n":
                    case "no":
                        callback.Invoke();
                        break;
                }
            }
        }
    }
}
