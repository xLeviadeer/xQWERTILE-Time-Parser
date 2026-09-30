using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {

    internal sealed record RegexCompile {

        // --- VARIABLES ---

        internal string? Name { get; init; } = null;

        internal required Regex Regex { get; init; }

        // --- CONSTRUCTOR ---

        [SetsRequiredMembers]
        internal RegexCompile(string name, Regex regex) {
            this.Name = name;
            this.Regex = regex;
        }

        [SetsRequiredMembers]
        internal RegexCompile(Regex regex) {
            this.Regex = regex;
        }

        // --- METHODS ---

        internal static Regex Compile(
            ReadOnlySpan<RegexCompile> compiles,
            string? joinWith = null,
            bool optional = false,
            bool anchored = false,
            bool atomic = false
        ) {
            var compiled = new string[compiles.Length];
            int i = 0;
            foreach (RegexCompile regex in compiles) {
                var currCompile = new StringBuilder();
                if (atomic) { currCompile.Append("(?>"); }
                if (regex.Name is null) {
                    currCompile.Append($"(?:{regex.Regex.ToString()})");
                } else {
                    currCompile.Append($"(?<{regex.Name}>{regex.Regex.ToString()})");
                }
                if (atomic) { currCompile.Append(")"); }
                if (optional) { currCompile.Append("?"); }
                compiled[i] = currCompile.ToString();
                i += 1;
            }
            var compiledStr = new StringBuilder();
            if (anchored) { compiledStr.Append("^"); }
            compiledStr.Append(string.Join(joinWith, compiled));
            if (anchored) { compiledStr.Append("$"); }
            return new Regex(compiledStr.ToString(), RegexOptions.IgnorePatternWhitespace);
        }
    }

    internal record struct ExactTime : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        private const string DAY_OF_WEEK_GROUP = "week";
        private const string DAY_OF_MONTH_GROUP = "day";
        private const string TIME_OF_DAY_GROUP = "time";

        private static Regex? __RE_EXACT_TIME_SECOND;
        public static Regex RE_EXACT_TIME_SECOND {
            get {
                if (__RE_EXACT_TIME_SECOND is null) {
                    __RE_EXACT_TIME_SECOND = RegexCompile.Compile(
                        joinWith: "|",
                        compiles: [
                            new(
                                RegexCompile.Compile(
                                    compiles: [
                                        new(DAY_OF_MONTH_GROUP, DayOfMonth.RE_DAY_OF_MONTH),
                                        new(TIME_OF_DAY_GROUP, TimeOfDay.RE_TIME_OF_DAY)
                                    ]
                                )
                            ),
                            new(DAY_OF_MONTH_GROUP, DayOfMonth.RE_DAY_OF_MONTH),
                            new(TIME_OF_DAY_GROUP, TimeOfDay.RE_TIME_OF_DAY)
                        ]
                    );
                }
                return __RE_EXACT_TIME_SECOND;
            }
        }

        // -- Instance --

        internal DayOfWeek? DayOfWeek_ { get; private set; }

        internal DayOfMonth? DayOfMonth_ { get; private set; }

        internal TimeOfDay? TimeOfDay_ { get; private set; }

        // --- CONSTRUCTOR ---

        private ExactTime(
            DayOfWeek? dayOfWeek = null,
            DayOfMonth? dayOfMonth = null,
            TimeOfDay? timeOfDay = null
        ) {
            this.DayOfWeek_ = dayOfWeek;
            this.DayOfMonth_ = dayOfMonth;
            this.TimeOfDay_ = timeOfDay;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string exactTimeStr,
            [NotNullWhen(true)] out ExactTime? exactTime
        ) {
            // flag for finding at least one
            bool atLeastOneFound = false;

            // create dictionary to collect results from
            Dictionary<string, object?> results = new() {
                [DAY_OF_WEEK_GROUP] = null,
                [DAY_OF_MONTH_GROUP] = null,
                [TIME_OF_DAY_GROUP] = null,
            };

            // check if there's at least 2 chars
            if (exactTimeStr.Length >= 2) {
                // check for week from first 2 chars
                string first2 = exactTimeStr[0..2];
                if (DayOfWeek.TryCreate(first2, out DayOfWeek? dayOfWeek)) {
                    // remove first 2 chars
                    exactTimeStr = exactTimeStr[2..];
                    results[DAY_OF_WEEK_GROUP] = dayOfWeek;
                    atLeastOneFound = true;
                }
            }

            
            // try to match second portion
            Match match = RE_EXACT_TIME_SECOND.Match(exactTimeStr);
            uint matchedLength = 0;
            if (match.Success) {
                // for every property we need from the regex as paired by name and create method
                foreach (
                    (var groupName, var parseMethod)
                    in new (string, IParsingObject.ParseMethod)[] {
                        (DAY_OF_MONTH_GROUP, DayOfMonth.TryCreate),
                        (TIME_OF_DAY_GROUP, TimeOfDay.TryCreate)
                    }
                ) {
                    // get group and check success
                    Group group = match.Groups[groupName];
                    if (group.Success) {
                        if (parseMethod.Invoke(group.Value, out object? out_obj)) {
                            results[groupName] = out_obj;
                            matchedLength += (uint)group.Value.Length;
                            atLeastOneFound = true;
                        } else if (group.Value != string.Empty) { goto failed; } // there was something to find that was missed
                    }
                }
            }

            // check matching length
            if (matchedLength != exactTimeStr.Length) { goto failed; }

            // check sucess and return
            if (atLeastOneFound) {
                // success
                exactTime = new(
                    (DayOfWeek?)results[DAY_OF_WEEK_GROUP],
                    (DayOfMonth?)results[DAY_OF_MONTH_GROUP],
                    (TimeOfDay?)results[TIME_OF_DAY_GROUP]
                );
                return true;
            }

        failed:
            exactTime = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = ExactTime.TryCreate(inputStr, out ExactTime? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString() 
            => $"{this.DayOfWeek_}{this.DayOfMonth_}{this.TimeOfDay_}";
    }
}
