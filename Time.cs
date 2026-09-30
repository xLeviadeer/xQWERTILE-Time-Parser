using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace XQWERTYATimeParserC_ {

    internal sealed record Time : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        private const char SPAN_SYMBOL = '〜';

        // -- Instance --

        internal PointTime StartTime { get; private init; }
        internal PointTime Value => StartTime;

        internal bool IsSpan => EndTime is not null;
        internal PointTime? EndTime { get; private init; }

        // --- CONSTRUCTOR ---

        private Time(PointTime point) {
            this.StartTime = point;
        }

        private Time(PointTime start, PointTime end) {
            this.StartTime = start;
            this.EndTime = end;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string timeStr,
            [NotNullWhen(true)] out Time? time
        ) {
            // check count of span symbols and assign time strings
            var timeStrs = new string?[2];
            int spanCount = timeStr.Count(c => c == SPAN_SYMBOL);
            if (spanCount == 0) {
                timeStrs[0] = timeStr;
            } else if (spanCount == 1) {
                timeStrs = timeStr.Split(SPAN_SYMBOL);
            } else { goto failed; }

            // try to create start
            if (timeStrs[0] is null) { goto failed; } // must exist
            if (!PointTime.TryCreate(timeStrs[0]!, out PointTime? startPoint)) { goto failed; }

            // try to create end & assign
            if (timeStrs[1] is not null) {
                // create span
                if (!PointTime.TryCreate(timeStrs[1]!, out PointTime? endPoint)) { goto failed; }
                time = new(startPoint, endPoint);
            } else {
                // create point
                time = new(startPoint);
            }

            return true;
        failed:
            time = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = Time.TryCreate(inputStr, out Time? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString() {
            var timeStr = new StringBuilder(this.StartTime.ToString());
            if (this.IsSpan) {
                timeStr.Append(SPAN_SYMBOL);
                timeStr.Append(this.EndTime!.ToString());
            }
            return timeStr.ToString();
        }

    }
}
