using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {
    
    internal sealed record SubBlock : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        public enum Cont {
            None,
            Also,
            Or
        }

        private static ImmutableDictionary<Cont, string> CONT_TO_STR = new Dictionary<Cont, string> {
            [Cont.None] = "",
            [Cont.Also] = "⩇",
            [Cont.Or] = "⊻",
        }.ToImmutableDictionary();

        private static Dictionary<string, Cont>? __STR_TO_CONT;
        private static ImmutableDictionary<string, Cont> STR_TO_CONT {
            get {
                if (__STR_TO_CONT is null) {
                    __STR_TO_CONT = new();
                    foreach ((Cont cont, string str) in CONT_TO_STR) {
                        __STR_TO_CONT[str] = cont;
                    }
                }
                return __STR_TO_CONT.ToImmutableDictionary();
            }
        }

        internal const string OPTIONAL_LEADING_SPACE = @"(?<!\s)\s?(?!\s)";

        private static string ALSO => CONT_TO_STR[Cont.Also];
        private static string OR => CONT_TO_STR[Cont.Or];
        private static string CONT_MARKER => @$"[{ALSO}{OR}]";
        private static string NOT_CONT_SPACE_MARKER => $@"[^{ALSO}{OR}\s]";
        private const string CONT_GROUP = @"c";
        private const string REM_GROUP = @"r";
        private static string SUB_BLOCKS => @$"{OPTIONAL_LEADING_SPACE}(?<{CONT_GROUP}>{CONT_MARKER}?)(?<{REM_GROUP}>{NOT_CONT_SPACE_MARKER}+)";
        private static Regex RE_SUB_BLOCKS => new Regex(SubBlock.SUB_BLOCKS);

        // -- Instance --

        internal Cont Continuation { get; private set; }
        internal Time Time { get; private set; }

        // --- CONSTRUCTORS ---

        private SubBlock(
            Cont continuation,
            Time time
        ) {
            this.Continuation = continuation;
            this.Time = time;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string subBlocksStr,
            [NotNullWhen(true)] out List<SubBlock>? subBlocks
        ) {
            subBlocks = new();
            MatchCollection matches = RE_SUB_BLOCKS.Matches(subBlocksStr);
            bool ranAtLeastOnce = false;
            for (int i = 0; i < matches.Count; i++) {
                ranAtLeastOnce = true;
                Match match = matches[i];
                string contStr = match.Groups[SubBlock.CONT_GROUP].Value;
                string remStr = match.Groups[SubBlock.REM_GROUP].Value;

                // try to get cont
                Cont cont = SubBlock.STR_TO_CONT[contStr];

                // if the first one is not Cont.None
                if (
                    (i == 0)
                    && (cont != Cont.None)
                ) { goto failed; }
                // if following ones don't have cont markers
                else if (
                    (i != 0)
                    && (cont == Cont.None)
                ) { goto failed; }

                // try to get time
                if (!Time.TryCreate(remStr, out Time? time)) { goto failed; }

                // add to blocks
                subBlocks.Add(new(cont, time));
            }
            if (!ranAtLeastOnce) { goto failed; }
            return true;
        failed:
            subBlocks = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = SubBlock.TryCreate(inputStr, out List<SubBlock>? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString()
            => $"{SubBlock.CONT_TO_STR[this.Continuation]}{this.Time.ToString()}";
    
        internal static string ListToString(IEnumerable<SubBlock> lst) {
            var str = new StringBuilder();
            bool isFirst = true;
            foreach (SubBlock subBlock in lst) {
                if (!isFirst) { str.Append(" "); } else { isFirst = false; }
                str.Append(subBlock.ToString());
            }
            return str.ToString();
        }
    }

    internal static class SubBlockListExtensions {
        extension(IEnumerable<SubBlock> lst) {
            internal string ListToString()
                => SubBlock.ListToString(lst);
        }
    }

    internal static class SubBlockImmutableListExtensions {
        extension(IReadOnlyList<SubBlock> lst) {
            internal string ListToString()
                => SubBlock.ListToString(lst);
        }
    }
}
