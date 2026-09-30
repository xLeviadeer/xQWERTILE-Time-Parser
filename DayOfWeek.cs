using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {

    internal enum DayOpt {
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
        Sunday,
        Week,
        WeekDay,
        WeekMain,
        WeekEnd,
        SaturdaySunday,
    }

    internal sealed record DaysSet(
        ImmutableHashSet<DayOpt> List,
        bool IsExclusive
    );

    internal sealed record DayOfWeek : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        // - Day Options -

        private static ImmutableHashSet<DayOpt> PRIMARY_DAYS => [
            DayOpt.Monday,
            DayOpt.Tuesday,
            DayOpt.Wednesday,
            DayOpt.Thursday,
            DayOpt.Friday,
            DayOpt.Saturday,
            DayOpt.Sunday
        ];

        internal bool IsPrimary => PRIMARY_DAYS.Contains(this.Day);

        private static ImmutableDictionary<DayOpt, DaysSet> NON_PRIMARY_DAY_MAP = new Dictionary<DayOpt, DaysSet> {
            [DayOpt.Week] = new([DayOpt.Monday, DayOpt.Tuesday, DayOpt.Wednesday, DayOpt.Thursday, DayOpt.Friday, DayOpt.Saturday, DayOpt.Sunday], false),
            [DayOpt.WeekDay] = new([DayOpt.Monday, DayOpt.Tuesday, DayOpt.Wednesday, DayOpt.Thursday, DayOpt.Friday], false),
            [DayOpt.WeekMain] = new([DayOpt.Monday, DayOpt.Tuesday, DayOpt.Wednesday, DayOpt.Thursday], false),
            [DayOpt.WeekEnd] = new([DayOpt.Saturday, DayOpt.Sunday], false),
            [DayOpt.SaturdaySunday] = new([DayOpt.Saturday, DayOpt.Sunday], true)
        }.ToImmutableDictionary();

        private static ImmutableDictionary<DayOpt, string> OPT_TO_STR = new Dictionary<DayOpt, string> {
            [DayOpt.Monday] = "Mo",
            [DayOpt.Tuesday] = "Tu",
            [DayOpt.Wednesday] = "We",
            [DayOpt.Thursday] = "Th",
            [DayOpt.Friday] = "Fr",
            [DayOpt.Saturday] = "Sa",
            [DayOpt.Sunday] = "Su",
            [DayOpt.Week] = "Wk",
            [DayOpt.WeekDay] = "Wd",
            [DayOpt.WeekMain] = "Wm",
            [DayOpt.WeekEnd] = "Wn",
            [DayOpt.SaturdaySunday] = "Ss"
        }.ToImmutableDictionary();

        private static Dictionary<string, DayOpt>? __STR_TO_OPT;
        private static ImmutableDictionary<string, DayOpt> STR_TO_OPT {
            get {
                if (__STR_TO_OPT is null) {
                    __STR_TO_OPT = new();
                    foreach ((DayOpt opt, string str) in OPT_TO_STR) {
                        __STR_TO_OPT[str] = opt;
                    }
                }
                return __STR_TO_OPT.ToImmutableDictionary();
            }
        }

        private static Regex? __RE_DAY_OF_WEEK;
        internal static Regex RE_DAY_OF_WEEK {
            get {
                if (__RE_DAY_OF_WEEK is null) {
                    var weeks = new string[OPT_TO_STR.Count];
                    int i = 0;
                    foreach (string str in OPT_TO_STR.Values) {
                        weeks[i] = str;
                        i += 1;
                    }
                    __RE_DAY_OF_WEEK = new(@$"(?:{string.Join("|", weeks)})");
                }
                return __RE_DAY_OF_WEEK;
            }
        }

        // -- Instance --

        internal DayOpt Day { get; private set; }
        internal DaysSet Days {
            get {
                // primary check
                if (!this.IsPrimary) {
                    return DayOfWeek.NON_PRIMARY_DAY_MAP[this.Day];
                }
                return new([this.Day], false);
            }
        }

        // --- CONSTRUCTOR ---

        private DayOfWeek(DayOpt day) {
            this.Day = day;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string dayOfWeekStr,
            [NotNullWhen(true)] out DayOfWeek? dayOfWeek
        ) {
            string key = dayOfWeekStr
                .Trim();
            if (DayOfWeek.STR_TO_OPT.ContainsKey(key)) {
                dayOfWeek = new(STR_TO_OPT[key]);
                return true;
            }
            dayOfWeek = null;
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

        // - TO STRING -

        public override string ToString()
            => OPT_TO_STR[this.Day];
    }
}
