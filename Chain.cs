using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using static XQWERTYATimeParserC_.Block;

namespace XQWERTYATimeParserC_ {
    internal sealed record Chain : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        [StringSyntax(StringSyntaxAttribute.Regex)]
        internal const string NUMBER = @"[0-9]";
        internal const string TWO_DIGITS = @"{1,2}";
        internal const string FOUR_DIGITS = @"{1,4}";
        internal const string TWO_DIGIT_NUM = @$"{NUMBER}{TWO_DIGITS}";
        internal const string FOUR_DIGIT_NUM = @$"{NUMBER}{FOUR_DIGITS}";

        internal enum Availability {
            None,
            Unavailable,
            Available
        }

        private static ImmutableDictionary<Availability, string> AVAILABILITY_TO_STR = new Dictionary<Availability, string> {
            [Availability.None] = "",
            [Availability.Unavailable] = "⧗",
            [Availability.Available] = "⧖"
        }.ToImmutableDictionary();

        private static Dictionary<string, Availability>? __STR_TO_AVAILABILITY;
        private static ImmutableDictionary<string, Availability> STR_TO_AVAILABILITY {
            get {
                if (__STR_TO_AVAILABILITY is null) {
                    __STR_TO_AVAILABILITY = new();
                    foreach ((Availability avail, string str) in AVAILABILITY_TO_STR) {
                        __STR_TO_AVAILABILITY[str] = avail;
                    }
                }
                return __STR_TO_AVAILABILITY.ToImmutableDictionary();
            }
        }

        private static Regex RE_AVAILABILITY => new Regex(@$"[{string.Join("", STR_TO_AVAILABILITY.Keys)}]");

        private const char INQUIRY = '⧓';

        // -- Instance --

        internal Availability Avail { get; private set; }
        internal bool IsInquiry { get; private set; }
        internal ImmutableList<Block> Blocks { get; private set; }

        // --- CONSTRUCTORS ---

        private Chain(
            Availability avail,
            bool isInquiry,
            ImmutableList<Block> blocks
        ) {
            this.Avail = avail;
            this.IsInquiry = isInquiry;
            this.Blocks = blocks;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string chainStr,
            [NotNullWhen(true)] out Chain? chain
        ) {
            // traverse the string by character
            int i = 0;

            // check for inquiry
            bool isInquiry = false;
            if (chainStr[i] == Chain.INQUIRY) {
                isInquiry = true;
                i += 1;
            }

            // check for availability
            Availability avail = Availability.None;
            Match match = RE_AVAILABILITY.Match(chainStr[i].ToString());
            if (match.Success) {
                avail = STR_TO_AVAILABILITY[chainStr[i].ToString()];
                i += 1;
            }

            // get remaining and try to parse
            string remaining = chainStr[i..];
            if (!Block.TryCreate(remaining, out List<Block>? blocks)) {
                chain = null;
                return false;
            }

            // return
            chain = new(
                avail,
                isInquiry,
                blocks.ToImmutableList()
            );
            return true;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = Chain.TryCreate(inputStr, out Chain? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString() {
            var str = new StringBuilder();

            // inquiry
            if (this.IsInquiry) {
                str.Append(Chain.INQUIRY);
            }

            // availability
            if (this.Avail != Availability.None) {
                str.Append(AVAILABILITY_TO_STR[this.Avail]);
            }

            // blocks
            str.Append(this.Blocks.ListToString());

            // return
            return str.ToString();
        }
    }
}
