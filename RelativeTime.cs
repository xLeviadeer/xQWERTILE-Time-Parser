using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {
    internal sealed record RelativeTime : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        private const int TIME_UPPER_BOUND = int.MaxValue;
        private const int TIME_LOWER_BOUND = 1;
        private static readonly SimpleRange TIME_RANGE = new(
            new(TIME_LOWER_BOUND, true),
            new(TIME_UPPER_BOUND, false)
        );

        private const string NUMBER_GROUP = @"n";
        private const string OTHER_GROUP = @"o";
        private const string RELATIVE_TIME = @$"^(?<{NUMBER_GROUP}>{Chain.TWO_DIGIT_NUM})(?<{OTHER_GROUP}>.*)$";
        private static readonly Regex RE_RELATIVE_TIME = new(RELATIVE_TIME);

        // -- Instance --

        internal uint Duration { get; private set; }

        internal DurationOfTime Incremenet { get; private set; }

        // --- CONSTRUCTOR ---

        private RelativeTime(uint duration, DurationOfTime increment) {
            this.Duration = duration;
            this.Incremenet = increment;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string relativeTimestr,
            [NotNullWhen(true)] out RelativeTime? relativeTime
        ) {
            // try to match
            Match match = RE_RELATIVE_TIME.Match(relativeTimestr);
            if (match.Success) {
                // get duration
                if (!match.Groups[NUMBER_GROUP].Success) { goto failed; }
                uint duration = uint.Parse(match.Groups[NUMBER_GROUP].Value);
                if (!TIME_RANGE.Contains((int)duration)) { goto failed; }

                // get increment
                if (!match.Groups[OTHER_GROUP].Success) { goto failed; }
                if (!DurationOfTime.TryCreate(
                    match.Groups[OTHER_GROUP].Value, 
                    out DurationOfTime? durationOfTime
                )) { goto failed; }

                // success
                relativeTime = new(duration, (DurationOfTime)durationOfTime);
                return true;
            }

        failed:
            relativeTime = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = RelativeTime.TryCreate(inputStr, out RelativeTime? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString()
            => $"{this.Duration}{this.Incremenet}";
    }
}
