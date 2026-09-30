namespace XQWERTYATimeParserC_ {
    
    class Program {
        private const string INPUT_FILE = "input.txt";
        private static readonly string INPUT_PATH = Path.Combine(".", INPUT_FILE);

        internal static void Main(string[] _) {
            // start
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("press ⦅ctrl + c⦆ to exit");
            Console.WriteLine();

            // loop
            while (true) {
                // get input
                string? input = File.ReadAllText(INPUT_PATH, System.Text.Encoding.UTF8);
                Console.WriteLine($"read ⸉ {input} ⸉ input from {INPUT_FILE}");
                if (input is null) { goto error; }
                if (input == string.Empty) { goto empty; }

                // temp
                if (!Chain.TryCreate(input, out Chain? chain)) { goto error; }
                Console.WriteLine($"echoing {chain.ToString()}");
            pause:
                Console.AskForInput("press anything to read again");
                Console.WriteLine();
                continue;
            error:
                Console.WriteLine("⌃ your input was invalid; try again ⌃");
                goto pause;
            empty:
                Console.WriteLine("⌃ your input was empty; enter something and try again ⌃");
                goto pause;
            }
        }
    }
}
