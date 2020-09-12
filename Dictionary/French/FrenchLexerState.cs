using Jmas.SpanishDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jmas.FrenchDictionary
{
    public class FrenchLexerState
    {
        public string State { get; }
        public List<(char, SpanishLexerMachineOutput)> Next { get; }
        public SpanishLexerMachineOutput DefaultNext { get; set; }

        public static SpanishLexerMachineOutput DefaultNextHold { get; } = new SpanishLexerMachineOutput("R", 0, false, 0);
        public static SpanishLexerMachineOutput DefaultNextClear { get; } = new SpanishLexerMachineOutput("", 0, false, 0);

        public FrenchLexerState(string s, SpanishLexerMachineOutput defaultNext)
        {
            State = s;
            Next = new List<(char, SpanishLexerMachineOutput)>();
            DefaultNext = defaultNext;
        }
        public void AddTransferStateWithEmit(char a, string st, int probeMove, int length, Dictionary<string, SpanishLexerState> dictionary)
        {
            var find = Next.FindIndex(pair => pair.Item1 == a);
            if (find != -1)
            {
                var t = Next[find].Item2;
                if (!t.EmitComb && !DefaultNext.EmitComb)
                {
                    var overrideState = dictionary[State + a];
                    overrideState.DefaultNext = new SpanishLexerMachineOutput(st, 1, true, length);
                }
                else if (t.State != st || t.ProbeMove != probeMove || t.Length != length)
                    throw new LexerStateConflict(this, a);
            }
            else
                Next.Add((a, new SpanishLexerMachineOutput(st, probeMove, true, length)));
        }
        public void AddTransferStateWithoutEmit(char a, string st, int probeMove, Dictionary<string, SpanishLexerState> dictionary)
        {
            var find = Next.FindIndex(pair => pair.Item1 == a);
            if (find != -1)
            {
                var t = Next[find].Item2;
                if (t.State != st || t.ProbeMove != probeMove || t.EmitComb)
                    throw new LexerStateConflict(this, a);
            }
            else
                Next.Add((a, new SpanishLexerMachineOutput(st, probeMove, false, 0)));
        }
        //public void AddTransferState(char a, SpanishLexerMachineOutput b)
        //{
        //    Next.Add((a, b));
        //}
        public SpanishLexerMachineOutput FindNext(char probe)
        {
            var match = Next.FindIndex(pair => pair.Item1 == probe);
            return match != -1 ? Next[match].Item2 : DefaultNext;
        }
    }
}
