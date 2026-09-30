using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace XQWERTYATimeParserC_ {

    internal enum DurationOpt {
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days,
        Weeks,
        Months,
        Years,
        Decades,
        Centuries,
        Millenniums
    }

    internal sealed record DurationOfTime : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        // - Duration Options -

        private static ImmutableDictionary<DurationOpt, string> OPT_TO_STR = new Dictionary<DurationOpt, string> {
            [DurationOpt.Milliseconds] = "𝓂𝓈",
            [DurationOpt.Seconds] = "𝓈",
            [DurationOpt.Minutes] = "𝓂",
            [DurationOpt.Hours] = "𝒽",
            [DurationOpt.Days] = "𝒹",
            [DurationOpt.Weeks] = "𝓌",
            [DurationOpt.Months] = "𝓸",
            [DurationOpt.Years] = "𝓎",
            [DurationOpt.Decades] = "𝒹𝒹",
            [DurationOpt.Centuries] = "𝒸𝒸",
            [DurationOpt.Millenniums] = "𝓂𝓂"
        }.ToImmutableDictionary();

        private static Dictionary<string, DurationOpt>? __STR_TO_OPT;
        private static ImmutableDictionary<string, DurationOpt> STR_TO_OPT {
            get {
                if (__STR_TO_OPT is null) {
                    __STR_TO_OPT = new();
                    foreach ((DurationOpt opt, string str) in OPT_TO_STR) {
                        __STR_TO_OPT[str] = opt;
                    }
                }
                return __STR_TO_OPT.ToImmutableDictionary();
            }
        }

        // -- Instance --

        internal DurationOpt Duration { get; private set; }

        // --- CONSTRUCTOR ---

        private DurationOfTime(DurationOpt duration) {
            this.Duration = duration;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string durationOfTimeStr,
            [NotNullWhen(true)] out DurationOfTime? durationOfTime
        ) {
            string key = durationOfTimeStr
                .Trim();
            if (DurationOfTime.STR_TO_OPT.ContainsKey(key)) {
                durationOfTime = new(STR_TO_OPT[key]);
                return true;
            }
            durationOfTime = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = DurationOfTime.TryCreate(inputStr, out DurationOfTime? out_obj);
            obj = out_obj;
            return output;
        }

        // - TO STRING -

        public override string ToString()
            => OPT_TO_STR[this.Duration];
    }
}
