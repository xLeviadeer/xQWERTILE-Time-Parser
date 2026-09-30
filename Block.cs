using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using static XQWERTYATimeParserC_.SubBlock;

namespace XQWERTYATimeParserC_ {
    internal sealed record Block {

        // --- VARIABLES ---

        // -- Static --

        internal enum Oper {
            Past,
            Present,
            Future,
            Until,
            Negation
        }

        private static ImmutableDictionary<Oper, string> OPER_TO_STR = new Dictionary<Oper, string> {
            [Oper.Past] = "⋊",
            [Oper.Present] = "⋈",
            [Oper.Future] = "⋉",
            [Oper.Until] = "⑊",
            [Oper.Negation] = "¬"
        }.ToImmutableDictionary();

        private static Dictionary<string, Oper>? __STR_TO_OPER;
        private static ImmutableDictionary<string, Oper> STR_TO_OPER {
            get {
                if (__STR_TO_OPER is null) {
                    __STR_TO_OPER = new();
                    foreach ((Oper oper, string str) in OPER_TO_STR) {
                        __STR_TO_OPER[str] = oper;
                    }
                }
                return __STR_TO_OPER.ToImmutableDictionary();
            }
        }

        private static Regex RE_OPER => new Regex(@$"[{string.Join("", STR_TO_OPER.Keys)}]"); 

        internal enum Jmp {
            None,
            This,
            Next,
            Last
        }

        private static ImmutableDictionary<Jmp, string> JMP_TO_STR = new Dictionary<Jmp, string> {
            [Jmp.None] = "",
            [Jmp.This] = "↡",
            [Jmp.Next] = "↠",
            [Jmp.Last] = "↞"
        }.ToImmutableDictionary();

        private static Dictionary<string, Jmp>? __STR_TO_JMP;
        private static ImmutableDictionary<string, Jmp> STR_TO_JMP {
            get {
                if (__STR_TO_JMP is null) {
                    __STR_TO_JMP = new();
                    foreach ((Jmp jump, string str) in JMP_TO_STR) {
                        __STR_TO_JMP[str] = jump;
                    }
                }
                return __STR_TO_JMP.ToImmutableDictionary();
            }
        }

        private static Regex RE_JMP => new Regex(@$"[{string.Join("", STR_TO_JMP.Keys)}]");

        private const string REPEAT_LAST = "⇠";
        private const string REPEAT_NEXT = "⇢";

        private static Regex RE_REPEAT => new Regex(@$"[{REPEAT_LAST}{REPEAT_NEXT}]");

        private const string NUM_GROUP = @"n";
        private static Regex RE_NUMBER => new Regex($@"^(?<{NUM_GROUP}>{Chain.NUMBER}{{1,10}})");
        
        // -- Instance --

        public bool IsRepeating { get; private set; }
        public Jmp Jump { get; private set; }
        public int JumpCount { get; private set; }
        public Oper Operation { get; private set; }
        public ImmutableList<SubBlock> SubBlocks { get; private set; }

        // --- CONSTRUCTORS ---

        private Block(
            Oper operation,
            Jmp jump,
            int jumpCount,
            bool isRepeating,
            ImmutableList<SubBlock> subBlocks
        ) {
            this.Operation = operation;
            this.Jump = jump;
            this.JumpCount = jumpCount;
            this.IsRepeating = isRepeating;
            this.SubBlocks = subBlocks;
        }

        // --- PARSING ---

        private static bool? _operDirection(Oper operation) 
            => operation switch {
                Oper.Past => false,
                Oper.Present or Oper.Future => true,
                Oper.Until or Oper.Negation => null
            };

        private static Oper? _determineOper(string blockStr, out int index) {
            MatchCollection matches = RE_OPER.Matches(blockStr);
            if (matches.Count != 1) {
                index = -1;
                return null; 
            }
            Match match = matches[0];
            index = match.Index;
            return STR_TO_OPER[match.Value];
        }

        internal static bool TryCreate(
            string blocksStr,
            [NotNullWhen(true)] out List<Block>? blocks
        ) {
            // compose possible blocks
            var blocksStrSplitFinal = new List<(string str, Oper operation, int index)>();
            {
                string[] blocksStrSplit = blocksStr.Split(" ");
                bool building = false;
                StringBuilder currBuildBlock = new();
                Oper? currBuildOper = null;
                int? currBuildIndex = null;
                for (int i = 0; i < blocksStrSplit.Length; i++) {
                    string currBlock = blocksStrSplit[i];

                    // get operation
                    Oper? operation = _determineOper(currBlock, out int operIndex);

                    // check building
                    if (operation is not null) {
                        // check finished
                        if (building == true) {
                            blocksStrSplitFinal.Add((
                                currBuildBlock.ToString(),
                                (Oper)currBuildOper!,
                                (int)currBuildIndex!
                            ));
                        } else {
                            building = true;
                        }
                        currBuildBlock = new(currBlock);
                        currBuildOper = operation;
                        currBuildIndex = operIndex;
                    } else {
                        currBuildBlock.Append(" ");
                        currBuildBlock.Append(currBlock);
                    }
                }

                // check missing oper or index
                if (
                    (currBuildOper is null)
                    || (currBuildIndex is null)
                ) {
                    goto failed;
                }

                // add whatever remaining
                blocksStrSplitFinal.Add((
                    currBuildBlock.ToString(),
                    (Oper)currBuildOper,
                    (int)currBuildIndex
                ));
            }

            // split by spaces and run for each 
            blocks = new();
            foreach ((string blockStr, Oper operation, int operIndex) in blocksStrSplitFinal) {
                // check side of arrow (by adjusting position) based on operation
                bool? isDirectionForward = _operDirection(operation);
                int indexOfArrow = operIndex;
                int indexOfCount = operIndex;
                int rightmost = operIndex + 1;
                if (isDirectionForward is not null) {
                    if (isDirectionForward == true) {
                        indexOfArrow += 1;
                        indexOfCount += 2;
                    } else {
                        indexOfArrow -= 1;
                        indexOfCount -= 2;
                    }
                }

                // check arrows
                bool isRepeating = false;
                Jmp jump = Jmp.None;
                if (
                    (indexOfArrow > -1)
                    && (indexOfArrow < blockStr.Length)
                ) {
                    string targetChar = blockStr[indexOfArrow].ToString();

                    // check for repeating
                    if (RE_REPEAT.IsMatch(targetChar)) {
                        isRepeating = true;
                        if (isDirectionForward == true) { rightmost += 1; }
                    }

                    // check for jump
                    else if (RE_JMP.IsMatch(targetChar)) {
                        jump = STR_TO_JMP[targetChar];
                        if (isDirectionForward == true) { rightmost += 1; }
                    }
                }

                // check counts
                bool hasArrow = (
                    isRepeating
                    || (jump != Jmp.None)
                );
                int count = 1;
                if (
                    hasArrow
                    && (indexOfCount > -1)
                    && (indexOfCount < blockStr.Length)
                ) {
                    string targetStr = (isDirectionForward == true)
                        ? blockStr[indexOfCount..]
                        : blockStr[..(indexOfCount + 1)];

                    // check for count
                    Match match = RE_NUMBER.Match(targetStr);
                    if (match.Success) {
                        string numStr = match.Groups[NUM_GROUP].Value;
                        count = int.Parse(numStr);
                        if (isDirectionForward == true) { rightmost += numStr.Length + 1; }
                    }
                }

                // get everything to the rightmost side as try to create a block with it
                string remaining = blockStr[rightmost..];
                if (!SubBlock.TryCreate(remaining, out List<SubBlock>? subBlocks)) { goto failed; }

                // create and add
                blocks.Add(new(
                    (Oper)operation,
                    jump,
                    count,
                    isRepeating,
                    subBlocks.ToImmutableList()
                ));
            }
            return true;
        failed:
            blocks = null;
            return false;
        }

        public static bool TryCreate(
            string inputStr,
            [NotNullWhen(true)] out object? obj
        ) {
            bool output = Block.TryCreate(inputStr, out List<Block>? out_obj);
            obj = out_obj;
            return output;
        }

        // --- TO STRING ---

        public override string ToString() {
            var str = new StringBuilder();
            bool? operDir = _operDirection(this.Operation);

            // place arrow and count for past
            if (operDir == false) {
                if (this.IsRepeating) {
                    str.Append(Block.REPEAT_LAST);
                } 

                else if (this.Jump != Jmp.None) {
                    if (this.JumpCount > 1) {
                        str.Append(this.JumpCount);
                    }
                    str.Append(Block.JMP_TO_STR[this.Jump]);
                }
            }

            // add operation
            str.Append(OPER_TO_STR[this.Operation]);

            // place arrow and count for future
            if (operDir == true) {
                if (this.IsRepeating) {
                    str.Append(Block.REPEAT_NEXT);
                } 
                
                else if (this.Jump != Jmp.None) {
                    str.Append(Block.JMP_TO_STR[this.Jump]);
                    if (this.JumpCount > 1) {
                        str.Append(this.JumpCount);
                    }
                }
            }

            // add subblocks
            string subBlocksStr = this.SubBlocks.ListToString();
            if (RE_NUMBER.IsMatch(subBlocksStr)) { str.Append(" "); } // add space if starts with number
            str.Append(subBlocksStr);

            // return
            return str.ToString();
        }

        internal static string ListToString(IEnumerable<Block> lst) {
            var str = new StringBuilder();
            bool isFirst = true;
            foreach (Block subBlock in lst) {
                if (!isFirst) { str.Append(" "); } else { isFirst = false; }
                str.Append(subBlock.ToString());
            }
            return str.ToString();
        }
    }

    internal static class BlockListExtensions {
        extension(IEnumerable<Block> lst) {
            internal string ListToString()
                => Block.ListToString(lst);
        }
    }

    internal static class BlockImmutableListExtensions {
        extension(IReadOnlyList<Block> lst) {
            internal string ListToString()
                => Block.ListToString(lst);
        }
    }
}
