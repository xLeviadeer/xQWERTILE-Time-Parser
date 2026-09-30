using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace XQWERTYATimeParserC_ {

    /// <summary>
    /// Describes the state of an invalid set of bounds 𝓇ᵃ᚜ valid ᚛ᵇ᚜ negative ᚛ᶜ᚜ one ᚛ᵈ᚜ zero ᚛.
    /// ᵃ bounds are valid / the bounds create a rnage which has 2 or more value which produce true.
    /// ᵇ lower bound is above the upper | upper bound is below the lower.
    /// ᶜ bounds are the same number / the bounds create a range which is only true when the value is exactly 1 number.
    /// ᵈ the bounds are the same number / the bounds create a range which is never true.
    /// </summary>
    internal enum SimpleRangeValidity {
        Valid,
        One,
        Zero,
        Invalid
    }

    /// <summary>
    /// a range alongside an enum flag of the range's relative capabilities
    /// </summary>
    internal readonly ref struct RangeBoundFlag {

        // --- VARIABLES ---

        /// <summary>
        /// the range of possible values
        /// </summary>
        internal required int Range { get; init; }

        /// <summary>
        /// the relative capabilities of this range
        /// </summary>
        internal required SimpleRangeValidity Validity { get; init; }

        // --- CONSTRUCTOR ---

        /// <summary>
        /// a range alongside an enum flag of the range's relative capabilities
        /// </summary>
        /// <param name="range"> the range of possible values </param>
        /// <param name="validity"> the relative capabilities of this range </param>
        [SetsRequiredMembers]
        internal RangeBoundFlag(int range, SimpleRangeValidity validity) {
            this.Range = range;
            this.Validity = validity;
        }
    }

    /// <summary>
    /// a bound in a range
    /// </summary>
    internal readonly struct RangeBound {

        // --- Variables ---

        /// <summary>
        /// the value of this range
        /// </summary>
        internal required int Num { get; init; }

        /// <summary>
        /// whether or not this bound is inclusive
        /// </summary>
        internal bool IsInclusive { get; init; } = false;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// a bound in a range
        /// </summary>
        /// <param name="num"> the value of this range </param>
        /// <param name="isInclusive"> whether or not this bound is inclusive </param>
        [SetsRequiredMembers]
        internal RangeBound(int num, bool isInclusive = false) {
            this.Num = num;
            this.IsInclusive = isInclusive;
        }

        // --- METHODS ---

        internal static uint CountInclusive(params ReadOnlySpan<RangeBound> bounds) {
            uint count = 0;
            for (int i = 0; i < bounds.Length; i++) {
                if (bounds[i].IsInclusive) {
                    count += 1;
                }
            }
            return count;
        }

        // -- Equals --

        // - Equal Values -

        /// <summary>
        /// if two bounds have equal values
        /// </summary>
        internal static bool EqualValues(RangeBound left, RangeBound right)
            => (left.Num == right.Num);
        /// <summary>
        /// if two bounds have equal values
        /// </summary>
        internal bool EqualValues(RangeBound other)
            => (this.Num == other.Num);

        // - Equal Inclusivity -

        /// <summary>
        /// if two bounds have equal inclusivity
        /// </summary>
        internal static bool EqualInclusivity(RangeBound left, RangeBound right)
            => (left.IsInclusive == right.IsInclusive);
        /// <summary>
        /// if two bounds have equal inclusivity
        /// </summary>
        internal bool EqualInclusivity(RangeBound other) 
            => (this.IsInclusive == other.IsInclusive);

        // - Equal -

        /// <summary>
        /// if two bounds have equal values and inclusivity
        /// </summary>
        internal static bool Equals(RangeBound left, RangeBound right) 
            => (
                EqualValues(left, right)
                && EqualInclusivity(left, right)
            );
        /// <summary>
        /// if two bounds have equal values and inclusivity
        /// </summary>
        internal bool Equals(RangeBound other)
            => Equals(this, other);


        // --- OPERATIONS ---

        public override int GetHashCode()
            => HashCode.Combine(this.Num, this.IsInclusive);
        public override bool Equals(object? obj) {
            if (obj is RangeBound other) {
                return this.Equals(other);
            }
            return false;
        }
        /// <summary>
        /// if two bounds have equal values and inclusivity
        /// </summary>
        public static bool operator ==(RangeBound left, RangeBound right)
            => left.Equals(right);
        /// <summary>
        /// if two bounds have equal values and inclusivity
        /// </summary>
        public static bool operator !=(RangeBound left, RangeBound right)
            => !(left == right);
    }

    internal readonly record struct SimpleRange {

        // --- VARIABLES ---

        /// <summary>
        /// the lower bound of this range
        /// </summary>
        internal RangeBound Lower { get; }

        /// <summary>
        /// the upper bound of this range
        /// </summary>
        internal RangeBound Upper { get; }

        // --- CONSTRUCTORS ---

        /// <summary>
        /// a simple range between two values which can be inclusive
        /// </summary>
        /// <param name="lower"> the lower bound of this range </param>
        /// <param name="upper"> the upper bound of this range </param>
        /// <exception cref="ArgumentException"> thrown when the bounds cannot produce any values </exception>
        internal SimpleRange(RangeBound lower, RangeBound upper) {
            if (!IsValidBounds(lower, upper)) {
                throw new ArgumentException($"{nameof(lower)} and {nameof(upper)} form a range which cannot ever contain a value");
            }
            this.Lower = lower;
            this.Upper = upper;
        }

        // --- METHODS ---

        // -- Range --

        /// <summary>
        /// adds if the number is positive 𝓇 subtracts if the number is negative
        /// </summary>
        /// <param name="numWithSign"> mutates numWithSign to be added/subtracted to/from </param>
        /// <param name="numToAddOrSub"> the number to add/subtract </param>
        internal static void AddPosOrSubNeg(ref int numWithSign, int numToAddOrSub) {
            int posOrNegAdd = Math.Sign(numWithSign) * numToAddOrSub;
            numWithSign += posOrNegAdd;
        }

        /// <summary>
        /// gets a range for the lower and upper bounds
        /// </summary>
        /// <param name="lower"> lower range bound </param>
        /// <param name="upper"> upper range bound </param>
        /// <returns> the range </returns>
        internal static int Range(RangeBound lower, RangeBound upper) {
            // if both values are the same (0)
            //  if either is inclusive 
            //      1 number 
            //  neither inclusive
            //      0

            // values are one or more different (1)
            //  if both are inclusive
            //      2 
            //  if either is inclusive (exclusive)
            //      1
            //  neither inclusive
            //      0

            // value are one or more different (-1)
            //  if both values are inclusive 
            //      -2
            // if either is inclusive
            //      -1
            // neither inclusive
            //      0

            // get unadjusted range
            int range = upper.Num - lower.Num;

            // get inclusivity count
            uint inclusivityCount = RangeBound.CountInclusive(lower, upper);

            // check equal values
            if (lower.EqualValues(upper)) {
                // add 1 for either or both inclusive
                if (inclusivityCount >= 1) {
                    range += 1;
                }

                // do nothing for neither inclusive
            }

            // values not equal
            else {
                // add 1 for both
                if (inclusivityCount == 2) {
                    AddPosOrSubNeg(ref range, 1);
                }

                // do nothing for 1 inclusive

                // sub 1 for neither inclusive
                else if (inclusivityCount == 0) {
                    AddPosOrSubNeg(ref range, -1);
                }
            }

            // return
            return range;
        }

        /// <summary>
        /// gets a range with a validity flag for the lower and upper bounds
        /// </summary>
        /// <param name="lower"> lower range bound </param>
        /// <param name="upper"> upper range bound </param>
        /// <returns> range with a validity flag </returns>
        internal static RangeBoundFlag RangeWithFlag(RangeBound lower, RangeBound upper) {
            // get range
            int range = SimpleRange.Range(lower, upper);

            // get flag
            int rangeSign = Math.Sign(range);
            SimpleRangeValidity flag = range switch {
                0 => SimpleRangeValidity.Zero,
                1 => SimpleRangeValidity.One,
                // the following can't be 0 𝓇 1 so inclusivity wont matter
                _ when lower.Num < upper.Num => SimpleRangeValidity.Valid,
                _ when lower.Num > upper.Num => SimpleRangeValidity.Invalid
            };

            // return
            return new RangeBoundFlag(range, flag);
        }

        /// <summary>
        /// geta a validity flag for this lower and upper bounds
        /// </summary>
        /// <param name="lower"> lower range bound </param>
        /// <param name="upper"> upper range bound </param>
        /// <returns> a validity flag </returns>
        internal static SimpleRangeValidity Flag(RangeBound lower, RangeBound upper)
            => SimpleRange.RangeWithFlag(lower, upper).Validity;

        // -- Valid Bounds --

        /// <summary>
        /// checks if the bounds can produce any values
        /// </summary>
        /// <param name="lower"> lower range bound </param>
        /// <param name="upper"> upper range bound </param>
        /// <returns> a boolean, true when bounds are valid </returns>
        internal static bool IsValidBounds(RangeBound lower, RangeBound upper) {
            switch (SimpleRange.Flag(lower, upper)) {
                case SimpleRangeValidity.Zero:
                case SimpleRangeValidity.Invalid:
                    return false;
                default:
                    return true;
            }
        }

        // -- Between Check --

        /// <summary>
        /// determines if a number is in the range
        /// </summary>
        /// <param name="num"> the number to check if in the range </param>
        /// <returns> a boolean, true if in the range </returns>
        internal bool Contains(int num) {
            // lower
            bool lowerValid = (this.Lower.IsInclusive)
                ? (num >= this.Lower.Num)
                : (num > this.Lower.Num);

            // upper
            bool upperValid = (this.Upper.IsInclusive)
                ? (num <= this.Upper.Num)
                : (num < this.Upper.Num);

            // return condition check
            return lowerValid && upperValid;
        }
    }
}
