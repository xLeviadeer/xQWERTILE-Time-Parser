using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {
    internal sealed record DayOfMonth : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        private const uint DAYS_IN_MONTH = 31;
        private static readonly SimpleRange DAYS_RANGE = new(
            new(1, true),
            new((int)DAYS_IN_MONTH, true)
        );

        private const string DAY_GROUP = "da";
        private const string DAY = @$"(?<{DAY_GROUP}>{Chain.TWO_DIGIT_NUM})";

        private const string MONTHS_GROUP = "mo";
        private static string? __MONTHS;
        private static string MONTHS {
            get {
                if (__MONTHS is null) {
                    var months = new string[MonthOfYear.OPT_TO_STR.Count];
                    int i = 0;
                    foreach ((MonthOpt _, string str) in MonthOfYear.OPT_TO_STR) {
                        months[i] = $"(?:{str})";
                        i += 1;
                    }
                    __MONTHS = $"(?:{string.Join("|", months)})";
                }
                return __MONTHS;
            }
        }

        private const string FIRST = "ˢᵗ";
        private const string SECOND = "ⁿᵈ";
        private const string THIRD = "ʳᵈ";
        private const string ENTH = "ᵗʰ";
        private static readonly string[] ORDINAL_SUFFIXES = [
            FIRST,
            SECOND,
            THIRD,
            ENTH
        ];
 
        private const string SUFFIXES_GROUP = "su";
        private static string? __SUFFIXES;
        private static string SUFFIXES {
            get {
                if (__SUFFIXES is null) {
                    var suffixes = new string[ORDINAL_SUFFIXES.Length];
                    int i = 0;
                    foreach (string suffix in ORDINAL_SUFFIXES) {
                        suffixes[i] = $"(?:{suffix})";
                        i += 1;
                    }
                    __SUFFIXES = $"(?:{string.Join("|", suffixes)})";
                }
                return __SUFFIXES;
            }
        }

        private static readonly string MONTHS_PART = @$"(?<{MONTHS_GROUP}>{MONTHS})";
        private static readonly string DAYS_PART = @$"(?:{DAY}(?<{SUFFIXES_GROUP}>{SUFFIXES}))";

        // pattern does ⊰not⊱ enforce using the correct suffix
        // pattern does ⊰not⊱ enforce that at least days or months exists
        // pattern will always match since both are optional
        internal static readonly Regex RE_DAY_OF_MONTH = new(
            $@"(?:{MONTHS_PART}{DAYS_PART})|{MONTHS_PART}|{DAYS_PART}",
            RegexOptions.IgnorePatternWhitespace
        );

        // -- Instance --

        internal MonthOfYear? Month { get; private set; }

        internal uint? Day { get; private set; }

        internal string? OrdinalSuffix {
            get {
                // get ending
                if (this.Day is not null) {
                    return _getSuffix((uint)this.Day);
                }
                return null;
            }
        }

        // --- CONSTRUCTOR ---

        private DayOfMonth(
            MonthOfYear? month = null,
            uint? day = null
        ) {
            this.Month = month;
            this.Day = day;
        }

        // --- METHODS ---

        // --- SUFFIX --- 

        private static string _getSuffix(uint day) {
            string dayStr = day.ToString();
            string ending;
            if (dayStr.Length > 1) {
                ending = dayStr[^2..];
            } else {
                ending = dayStr[^1..];
            }

            // filter for -teenths
            switch (ending) {
                case "11":
                case "12":
                case "13":
                    return ENTH;
            }

            // filter for first digits
            ending = dayStr[^1..];
            switch (ending) {
                case "1": return FIRST;
                case "2": return SECOND;
                case "3": return THIRD;
            }

            // default
            return ENTH;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string dayOfMonthStr,
            [NotNullWhen(true)] out DayOfMonth? dayOfMonth
        ) {
            // match
            Match match = RE_DAY_OF_MONTH.Match(dayOfMonthStr);
            bool isDayIncluded = match.Groups[DAY_GROUP].Success;
            bool isMonthIncluded = match.Groups[MONTHS_GROUP].Success;

            // just months
            uint? day = null;
            if (isDayIncluded) {
                // get day
                string dayStr = match.Groups[DAY_GROUP].Value;
                day = uint.Parse(dayStr);
                if (!DAYS_RANGE.Contains((int)day)) { goto failed; }

                // check suffix if exists and is correct
                bool isDaySuffixed = match.Groups[SUFFIXES_GROUP].Success;
                if (isDaySuffixed) {
                    string suffixStr = match.Groups[SUFFIXES_GROUP].Value;
                    if (suffixStr != _getSuffix((uint)day)) { goto failed; }
                }
            }

            // just day
            MonthOfYear? month = null;
            if (isMonthIncluded) {
                string monthStr = match.Groups[MONTHS_GROUP].Value;
                if (MonthOfYear.TryCreate(monthStr, out MonthOfYear? out_month)) {
                    month = (MonthOfYear)out_month;
                }
            }

            // if at least one found
            if (
                (month is not null)
                || (day is not null)
            ) {
                dayOfMonth = new(month, day);
                return true;
            }

            // no success
        failed:
            dayOfMonth = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = DayOfMonth.TryCreate(inputStr, out DayOfMonth? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---
        public override string ToString()
            => $"{this.Month}{this.Day}{this.OrdinalSuffix}";
    }
}
