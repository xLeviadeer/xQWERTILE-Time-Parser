using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace XQWERTYATimeParserC_ {

    internal enum Meridiem {
        Am,
        Pm
    }

    internal sealed record TimeOfDay : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        // - Timing Info -

        private const uint MILLISECONDS_IN_SECOND = 1000;
        private const uint SECONDS_IN_MINUTE = 60;
        private const uint MINUTES_IN_HOUR = 60;
        private const uint HOURS_IN_MERIDIEM = 12;

        // - Hours/Minutes/Seconds/Milliseconds -

        private const string HOUR_GROUP = @"hr";
        private const string HOUR = @$"(?<{HOUR_GROUP}>{Chain.TWO_DIGIT_NUM})";

        private const string MINUTE_GROUP = @"min";
        private const string MINUTE = @$"(?<{MINUTE_GROUP}>{Chain.TWO_DIGIT_NUM})";

        private const string SECOND_GROUP = @"s";
        private const string SECOND = @$"(?<{SECOND_GROUP}>{Chain.TWO_DIGIT_NUM})";

        private const string MILLISECOND_GROUP = @"ms";
        private const string MILLISECOND = @$"(?<{MILLISECOND_GROUP}>{Chain.FOUR_DIGIT_NUM})";

        // - AM / PM -

        // gets a group of name ⟨Am⟩ 𝓇 ⟨Pm⟩ depending on which is present
        private const string AM_GROUP = @"Am";
        private const string PM_GROUP = @"Pm";
        [StringSyntax(StringSyntaxAttribute.Regex)]
        private const string AMPM = @$"(?:(?<{AM_GROUP}>[ᵃᴬ])|(?<{PM_GROUP}>[ᵖᴾ]))[ᵐᴹ]";

        // - Full - 

        private const string TIME_SPLIT = @":";
        internal static readonly Regex RE_TIME_OF_DAY = new(
            @$"
                {HOUR}
                (?:{TIME_SPLIT}{MINUTE})?
                (?:{TIME_SPLIT}{SECOND})?
                (?:{TIME_SPLIT}{MILLISECOND})?
                {AMPM}
            ", 
            RegexOptions.IgnorePatternWhitespace
        );

        // - Ranges -

        private static readonly SimpleRange MILLISECONDS_RANGE = new(
            new(0, true),
            new((int)MILLISECONDS_IN_SECOND, false)
        );
        private static readonly SimpleRange SECONDS_RANGE = new(
            new(0, true),
            new((int)SECONDS_IN_MINUTE, false)
        );
        private static readonly SimpleRange MINUTES_RANGE = new(
            new(0, true),
            new((int)MINUTES_IN_HOUR, false)
        );
        private static readonly SimpleRange HOURS_RANGE = new(
            new(0, false),
            new((int)HOURS_IN_MERIDIEM, true)
        );

        // -- Instance --

        internal Meridiem Meridiem { get; private set; }

        public uint MillisecondsTotal => (uint)Math.Round(SecondsTotal * MILLISECONDS_IN_SECOND);
        public double SecondsTotal { get; private set; } = 0;
        public uint MinutesTotal => (uint)(SecondsTotal / SECONDS_IN_MINUTE);
        public uint HoursTotal => (uint)(MinutesTotal / MINUTES_IN_HOUR);

        public uint Milliseconds => MillisecondsTotal % MILLISECONDS_IN_SECOND;
        public uint Seconds => (uint)Math.Floor(SecondsTotal % SECONDS_IN_MINUTE);
        public uint Minutes => MinutesTotal % SECONDS_IN_MINUTE;
        public uint Hours => HoursTotal % MINUTES_IN_HOUR;
        public uint HoursMeridiem {
            get {
                uint hoursMod = Hours % HOURS_IN_MERIDIEM;
                if (hoursMod == 0) {
                    return HOURS_IN_MERIDIEM;
                }
                return hoursMod;
            }
        }


        // --- CONSTRUCTOR ---

        private TimeOfDay(
            Meridiem meridiem,
            uint hours = 0,
            uint minutes = 0,
            uint seconds = 0,
            uint milliseconds = 0
        ) {
            this.Meridiem = meridiem;
            this.SecondsTotal += hours * MINUTES_IN_HOUR * SECONDS_IN_MINUTE;
            this.SecondsTotal += minutes * SECONDS_IN_MINUTE;
            this.SecondsTotal += seconds;
            this.SecondsTotal += (double)milliseconds / MILLISECONDS_IN_SECOND;
        }

        private TimeOfDay(Meridiem meridiem, uint seconds) {
            this.Meridiem = meridiem;
            this.SecondsTotal = seconds;
        }

        // --- METHODS ---

        private static void _ifPmOr12Inverse(
            Meridiem meridiem, 
            uint hour, 
            Action? trueCallback = null, 
            Action? falseCallback = null
        ) {
            if (
                (trueCallback is not null)
                && (
                    (meridiem == Meridiem.Pm)
                    && (hour != HOURS_IN_MERIDIEM)
                    || (
                        (meridiem == Meridiem.Am)
                        && (hour == HOURS_IN_MERIDIEM)
                    )
                )
            ) {
                trueCallback.Invoke();
            } else if (falseCallback is not null) {
                falseCallback.Invoke();
            }
        }
        private void _ifPmOr12Inverse(Action? trueCallback = null, Action? falseCallback = null)
            => TimeOfDay._ifPmOr12Inverse(this.Meridiem, this.Hours, trueCallback, falseCallback);

        // --- PARSING ---

        internal static bool TryCreate(
            string exactTimeStr,
            [NotNullWhen(true)] out TimeOfDay? timeOfDay
        ) {
            // check regex
            Match match = RE_TIME_OF_DAY.Match(exactTimeStr);
            if (!match.Success) {
                goto failed;
            }

            // get AM/PM
            Meridiem meridiem;
            if (match.Groups[AM_GROUP].Success) {
                meridiem = Meridiem.Am;
            } else if (match.Groups[PM_GROUP].Success) {
                meridiem = Meridiem.Pm;
            } else {
                throw new InvalidOperationException("meridiem founnd was not AM or PM");
            }

            // check hours
            uint hours = uint.Parse(match.Groups[HOUR_GROUP].Value);
            if (!HOURS_RANGE.Contains((int)hours)) { goto failed; }
            _ifPmOr12Inverse(meridiem, hours, () => hours += HOURS_IN_MERIDIEM);

            // check minutes
            uint minutes = 0;
            if (match.Groups[MINUTE_GROUP].Success) {
                minutes =uint.Parse(match.Groups[MINUTE_GROUP].Value);
            }
            if (!MINUTES_RANGE.Contains((int)minutes)) { goto failed; }

            // check seconds
            uint seconds = 0;
            if (match.Groups[SECOND_GROUP].Success) {
                seconds = uint.Parse(match.Groups[SECOND_GROUP].Value);
            }
            if (!SECONDS_RANGE.Contains((int)seconds)) { goto failed; }

            // check milliseconds
            uint milliseconds = 0;
            if (match.Groups[MILLISECOND_GROUP].Success) {
                milliseconds = uint.Parse(match.Groups[MILLISECOND_GROUP].Value);
            }
            if (!MILLISECONDS_RANGE.Contains((int)milliseconds)) { goto failed; }

            // return
            timeOfDay = new(
                meridiem,
                hours,
                minutes,
                seconds,
                milliseconds
            );
            return true;
        failed:
            timeOfDay = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = TimeOfDay.TryCreate(inputStr, out TimeOfDay? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString() {
            StringBuilder time = new();

            // hours
            time.Append(this.HoursMeridiem);

            // minutes
            if (this.Minutes != 0) {
                time.Append(TIME_SPLIT);
                time.Append(this.Minutes);
            }

            // seconds
            if (this.Seconds != 0) {
                time.Append(TIME_SPLIT);
                time.Append(this.Seconds);
            }

            // milliseconds
            if (this.Milliseconds != 0) {
                time.Append(TIME_SPLIT);
                time.Append(this.Milliseconds);
            }

            // meridiem
            time.Append(this.Meridiem switch {
                Meridiem.Am => "am",
                Meridiem.Pm => "pm"
            });

            // return
            return time.ToString();
        }
    }
}
