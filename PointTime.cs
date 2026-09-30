using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace XQWERTYATimeParserC_ {
    internal sealed record PointTime : IParsingObject {

        // --- VARIABLES ---

        // -- Static --

        internal enum Types {
            SomeTime,
            Relative,
            Exact
        }

        private const string SOME_TIME = "^";

        // -- Instance --

        internal Types Type {
            get {
                if (IsSomeTime) { return Types.SomeTime; } else if (IsRelative) { return Types.Relative; } else if (IsExact) { return Types.Exact; } else { throw new ArgumentException("this class is impossibly no type"); }
            }
        }

        internal bool IsSomeTime { get; private init; }

        internal bool IsRelative => Relative is not null;
        internal RelativeTime? Relative { get; private init; }

        internal bool IsExact => Exact is not null;
        internal ExactTime? Exact { get; private init; }

        // --- CONSTRUCTORS ---

        private PointTime() {
            this.IsSomeTime = true;
        }

        private PointTime(RelativeTime relative) {
            this.Relative = relative;
        }

        private PointTime(ExactTime exact) {
            this.Exact = exact;
        }

        // --- PARSING ---

        internal static bool TryCreate(
            string pointTimeStr,
            [NotNullWhen(true)] out PointTime? pointTime
        ) {
            // check for some time
            if (pointTimeStr == SOME_TIME) {
                pointTime = new();
                return true;
            }

            // check for relative
            if (RelativeTime.TryCreate(pointTimeStr, out RelativeTime? relative)) {
                pointTime = new((RelativeTime)relative);
                return true;
            }
            
            // check for exact
            if (ExactTime.TryCreate(pointTimeStr, out ExactTime? exact)) {
                pointTime = new((ExactTime)exact);
                return true;
            }

            pointTime = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = PointTime.TryCreate(inputStr, out PointTime? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString()
            => this.Type switch {
                Types.SomeTime => "^",
                Types.Relative => this.Relative!.ToString(),
                Types.Exact => this.Exact.ToString()!,
                _ => throw new ArgumentException("this class is impossibly no type")
            };
    }
}
