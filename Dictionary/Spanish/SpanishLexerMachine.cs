using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Linq;
using Jmas.Commons;

namespace Jmas.SpanishDictionary
{
    public class LexerMachine
    {
        protected static string DecreasePriorityMacro = "@DecPriority";
        protected static string SetDefaultOutputMacro = "@Default";
        protected const char ReplaceInputMacro = 'R';
        protected const char MatchEndOfWordMacro = '$';
        protected const char MatchBeginOfWordMacro = '^';
        protected const char NextCharUsedMacro = 'C';
        protected const int MaxWordLength = 32;

        protected List<Dictionary<string, SpanishLexerState>> LexerRules { get; private set; }

        public LexerMachine()
        {

        }

        protected void SetLexerRuleNumber(int num)
        {
            LexerRules = new List<Dictionary<string, SpanishLexerState>>(num);
            for (int i = 0; i < num; i++)
            {
                LexerRules.Add(new Dictionary<string, SpanishLexerState>
                {
                    { "", new SpanishLexerState("", SpanishLexerState.DefaultNextClear) },
                    { "^", new SpanishLexerState("^", new SpanishLexerMachineOutput("", 1, false, 0)) }
                });
            }
        }
        protected SpanishLexerState GetStateOrAddDefault(string cache, int priority)
        {
            var ruleClout = LexerRules[priority];
            if (ruleClout.ContainsKey(cache))
                return ruleClout[cache];
            else
            {
                var nr = new SpanishLexerState(cache, new SpanishLexerMachineOutput("", cache.Length, false, 0));
                ruleClout[cache] = nr;
                return nr;
            }
        }
        protected SpanishLexerState GetStateOrAdd(string cache, int probeMove, int priority)
        {
            var ruleClout = LexerRules[priority];
            if (ruleClout.ContainsKey(cache))
                return ruleClout[cache];
            else
            {
                var nr = new SpanishLexerState(cache, new SpanishLexerMachineOutput("", probeMove, false, 0));
                ruleClout[cache] = nr;
                return nr;
            }
        }

        protected virtual void BuildRules(string rules)
        {
            var s = rules.Split('\n');
            var q = from line in s select line.Trim();
            // Normally, line.Length == 1 is meaningless
            var t = from line in q where line.Length == 1 || line.Length >= 2 && line.Substring(0, 2) != "//" select line.Split('T');
            var priority = 0;

            foreach (var rule in t)
            {
                if (rule[0] == DecreasePriorityMacro)
                {
                    priority++;
                }
                else if (rule.Length == 1)
                {
                    BuildState(rule[0], priority);
                }
                else if (rule.Length == 2)
                {
                    BuildState(rule[0], rule[1], priority);
                }
                else
                {
                    
                }
            }
        }
        protected virtual void BuildState(string singleComb, int priority)
        {
            var sb = new StringBuilder(10);
            for (int i = 0; i < singleComb.Length - 1; i++)
            {
                var state = GetStateOrAddDefault(sb.ToString(), priority);
                sb.Append(singleComb[i]);
                state.AddTransferStateWithoutEmit(singleComb[i], sb.ToString(), 0, LexerRules[priority]);
            }
            var lasts = sb.ToString();
            sb.Append(singleComb[singleComb.Length - 1]);
            GetStateOrAddDefault(lasts, priority).AddTransferStateWithEmit(singleComb[singleComb.Length - 1], "", 0, singleComb.Length, LexerRules[priority]);
        }
        protected virtual void BuildState(string singleComb, string succeededBy, int priority)
        {
            if (succeededBy.Length == 0)
                throw new ArgumentException($"succeededBy cannot be an empty string");

            var sb = new StringBuilder(10);
            for (int i = 0; i < singleComb.Length; i++)
            {
                var state = GetStateOrAddDefault(sb.ToString(), priority);
                sb.Append(singleComb[i]);
                state.AddTransferStateWithoutEmit(singleComb[i], sb.ToString(), 0, LexerRules[priority]);
            }
            for (int i = 0; i < succeededBy.Length; i++)
            {
                var state = GetStateOrAdd(sb.ToString(), singleComb.Length + i, priority);
                sb.Append(succeededBy[i]);
                if (i != succeededBy.Length - 1)
                    state.AddTransferStateWithoutEmit(succeededBy[i], sb.ToString(), 0, LexerRules[priority]);
                else
                    state.AddTransferStateWithEmit(succeededBy[i], "", succeededBy.Length, singleComb.Length, LexerRules[priority]);
            }
        }

        // convert all states to a string, perfect for testing
        public virtual string GetStatesString()
        {
            var a = new StringBuilder(50 * 20);
            for (int priority = 0; priority < LexerRules.Count; priority++)
            {
                var ruleClout = LexerRules[priority];
                a.AppendLine($"Priority {priority}:");
                foreach (var state in ruleClout.Values)
                {
                    var p = state.State;
                    var ruleStrings = from pair in state.Next select $"{p} + {pair.Item1} => {pair.Item2.State}, {pair.Item2.ProbeMove}, {pair.Item2.EmitComb}, {pair.Item2.Length}";
                    foreach (var s in ruleStrings)
                        a.AppendLine(s);
                    var defaultRule = state.DefaultNext;
                    if (defaultRule != SpanishLexerState.DefaultNextHold && (defaultRule.EmitComb || (defaultRule.ProbeMove != state.State.Length && defaultRule.ProbeMove != 0)))
                    a.AppendLine($"{p}.Default => {defaultRule.State}, {defaultRule.ProbeMove}, {defaultRule.EmitComb}, {defaultRule.Length}");
                }
            }
            return a.ToString();
        }

        // free the memory of this very long string when quitting this function
        // It is less convenient to maintain all these states built by direct codes,
        // rather than states build by an algorithm and a long literal string.
        public virtual void Init(string rule)
        {
            int num = 1;
            foreach (var line in rule.Split('\n'))
                if (line.Trim() == DecreasePriorityMacro)
                    num++;
            SetLexerRuleNumber(num);
            BuildRules(rule);
        }

        public virtual List<(int, int)> FindAllCombs(SpanishWord word)
        {
            var letterUsedFlags = 0;
            var w = "^" + word.Content + '$';
            var ans = new List<(int, int)>();
            foreach (var ruleClout in LexerRules)
            {
                var cache = "";
                var state = ruleClout[cache];
                for (int i = 0; i < w.Length;)
                {
                    char c = w[i];
                    //Console.WriteLine($"state {(cache == "" ? "@Empty" : cache)} Reading {c}");
                    var output = state.FindNext(c);
                    startEmit:
                    var nextCache = output.State.Replace(ReplaceInputMacro, c);
                    if (output.EmitComb)
                    {
                        var startPos = i - output.Length - output.ProbeMove; // +1 -1 done here
                        var outLen = output.Length;
                        if (startPos == -1)
                        {
                            startPos += 1;
                            outLen -= 1;
                        }
                        else if (startPos + outLen == word.Content.Length + 1)
                        {
                            outLen -= 1;
                        }

                        var mask = BitMask.GetMask(startPos, outLen);
                        if ((mask & letterUsedFlags) != 0) // there is a conflict, go to default next
                        {
                            output = state.DefaultNext;
                            goto startEmit;
                        }
                        else
                        {
                            ans.Add((startPos, outLen));
                            letterUsedFlags |= mask;
                        }
                    }
                    cache = nextCache;
                    state = ruleClout[cache];
                    if (output.ProbeMove != 0)
                        i -= output.ProbeMove;
                    i++;
                }
            }
            ans.Sort((a, b) => a.Item1 - b.Item1);
#if DEBUG
            foreach (var ccc in ans) Console.Write(ccc);
            Console.WriteLine();
#endif
            return ans;
        }
    }

}
