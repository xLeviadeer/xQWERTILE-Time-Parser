using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {

    internal enum MonthOpt {
        January,
        Febuary,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December,
        Spring,
        Summer,
        Fall,
        Winter,
        Q1,
        Q2,
        Q3,
        Q4
    }

    internal sealed record MonthSet(
        ImmutableHashSet<MonthOpt> List
    );

    internal sealed record MonthOfYear : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        // -- Month Options --

        private static ImmutableHashSet<MonthOpt> PRIMARY_MONTHS => [
            MonthOpt.January,
            MonthOpt.Febuary,
            MonthOpt.March,
            MonthOpt.April,
            MonthOpt.May,
            MonthOpt.June,
            MonthOpt.July,
            MonthOpt.August,
            MonthOpt.September,
            MonthOpt.October,
            MonthOpt.November,
            MonthOpt.December
        ];

        internal bool IsPrimary => PRIMARY_MONTHS.Contains(this.Month);

        private static ImmutableDictionary<MonthOpt, MonthSet> NON_PRIMARY_MONTH_MAP = new Dictionary<MonthOpt, MonthSet> {
            [MonthOpt.Spring] = new([MonthOpt.March, MonthOpt.April, MonthOpt.May]),
            [MonthOpt.Summer] = new([MonthOpt.June, MonthOpt.July, MonthOpt.August]),
            [MonthOpt.Fall] = new([MonthOpt.September, MonthOpt.October, MonthOpt.November]),
            [MonthOpt.Winter] = new([MonthOpt.December, MonthOpt.January, MonthOpt.Febuary]),
            [MonthOpt.Q1] = new([MonthOpt.January, MonthOpt.Febuary, MonthOpt.March]),
            [MonthOpt.Q2] = new([MonthOpt.April, MonthOpt.May, MonthOpt.June]),
            [MonthOpt.Q3] = new([MonthOpt.July, MonthOpt.August, MonthOpt.September]),
            [MonthOpt.Q4] = new([MonthOpt.October, MonthOpt.November, MonthOpt.December])
        }.ToImmutableDictionary();

        internal static ImmutableDictionary<MonthOpt, string> OPT_TO_STR = new Dictionary<MonthOpt, string> {
            [MonthOpt.January] = "Jan",
            [MonthOpt.Febuary] = "Feb",
            [MonthOpt.March] = "Mar",
            [MonthOpt.April] = "Apr",
            [MonthOpt.May] = "May",
            [MonthOpt.June] = "June",
            [MonthOpt.July] = "July",
            [MonthOpt.August] = "Aug",
            [MonthOpt.September] = "Sep",
            [MonthOpt.October] = "Oct",
            [MonthOpt.November] = "Nov",
            [MonthOpt.December] = "Dec",
            [MonthOpt.Spring] = "Spr",
            [MonthOpt.Summer] = "Sum",
            [MonthOpt.Fall] = "Fal",
            [MonthOpt.Winter] = "Win",
            [MonthOpt.Q1] = "Q1",
            [MonthOpt.Q2] = "Q2",
            [MonthOpt.Q3] = "Q3",
            [MonthOpt.Q4] = "Q4"
        }.ToImmutableDictionary();

        private static Dictionary<string, MonthOpt>? __STR_TO_OPT;
        private static ImmutableDictionary<string, MonthOpt> STR_TO_OPT {
            get {
                if (__STR_TO_OPT is null) {
                    __STR_TO_OPT = new();
                    foreach ((MonthOpt opt, string str) in OPT_TO_STR) {
                        __STR_TO_OPT[str] = opt;
                    }
                    __STR_TO_OPT["fall"] = MonthOpt.Fall;
                }
                return __STR_TO_OPT.ToImmutableDictionary();
            }
        }

        // -- Instance --

        internal MonthOpt Month { get; private set; }
        internal MonthSet Months {
            get {
                // primary check
                if (!this.IsPrimary) {
                    return MonthOfYear.NON_PRIMARY_MONTH_MAP[this.Month];
                }
                return new([this.Month]);
            }
        }

        // --- CONSTRUCTOR ---

        private MonthOfYear(MonthOpt month) {
            this.Month = month;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string monthOfYearStr,
            [NotNullWhen(true)] out MonthOfYear? monthOfYear
        ) {
            string key = monthOfYearStr
                .Trim();
            if (MonthOfYear.STR_TO_OPT.ContainsKey(key)) {
                monthOfYear = new(STR_TO_OPT[key]);
                return true;
            }
            monthOfYear = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = MonthOfYear.TryCreate(inputStr, out MonthOfYear? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString()
            => OPT_TO_STR[this.Month];
    }
}
