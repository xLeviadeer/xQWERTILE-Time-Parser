using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace XQWERTYATimeParserC_ {
    internal interface IParsingObject {

        // --- PARSING ---

        public delegate bool ParseMethod(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        );

        public static abstract bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        );
    }
}
