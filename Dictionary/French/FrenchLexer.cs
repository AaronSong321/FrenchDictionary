using Jmas.Commons;
using Jmas.SpanishDictionary;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jmas.FrenchDictionary
{
    public class FrenchLexerMachine : LexerMachine
    {
        protected const string MatchAnyVowelMacro = "V";
        protected const string MatchAnyConsonantMacro = "O";
        protected const string MatchAnySoftVowelMacro = "@IE";
        protected const string MatchAnyNonSoftVowelMacro = "@^IE";
        // most speical char combinations only exists once per word. If not, use the other dictionary instead
        private static readonly Dictionary<string, (int, int)> combInSpecialWords = new Dictionary<string, (int, int)>
        {
            {"il",(0,1) },{"fil",(1,1) },{"cil",(1,1) }, // il
            {"moelle", (1, 2) },{"moellon",(1,2) },{"boette",(1,2) },{"poêlle",(1,2)}, // oe
            {"ischion",(2,2) },{"schizophrénie",(1,2) }, // sch

        };
        private static readonly List<(Regex, int, int)> combInSpecialRegex = new List<(Regex, int, int)>
        {
            {(new Regex(@"compt\w+"),3,2) },// pt
        };
        private static readonly Dictionary<string, (int, int)[]> multiCombInSpecialWords = new Dictionary<string, (int, int)[]>
        {
            
        };
        public static void AddSpecialWord(string word, int startPos, int length)
        {
            if (!combInSpecialWords.ContainsKey(word))
                combInSpecialWords.Add(word, (startPos, length));
        }
        public FrenchLexerMachine(): base()
        {
            
        }

        protected override void BuildState(string singleComb, int priority)
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
        protected override void BuildState(string singleComb, string succeededBy, int priority)
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

            succeededBy = succeededBy
                .Replace(MatchAnyVowelMacro, $"[{FrenchConstant.VowelChars}]")
                .Replace(MatchAnyConsonantMacro, $"[^{FrenchConstant.VowelChars}]")
                .Replace(MatchAnySoftVowelMacro, $"[{FrenchConstant.SoftVowelChars}]")
                .Replace(MatchAnyNonSoftVowelMacro, $"[{FrenchConstant.NonSoftVowelChars}]");
            if (succeededBy[0] == '[' && succeededBy[succeededBy.Length - 1] == ']')
            {
                if (succeededBy[1] != '^')
                {
                    var state = GetStateOrAdd(sb.ToString(), singleComb.Length, priority);
                    for (int i = 1; i < succeededBy.Length - 1; i++)
                    {
                        state.AddTransferStateWithEmit(succeededBy[i], "", 1, singleComb.Length, LexerRules[priority]);
                    }
                }
                else
                {
                    var state = GetStateOrAdd(sb.ToString(), singleComb.Length, priority);
                    for (int i = 2; i < succeededBy.Length - 1; i++)
                    {
                        state.AddTransferStateWithoutEmit(succeededBy[i], "", singleComb.Length, LexerRules[priority]);
                    }
                    var curState = GetStateOrAddDefault(sb.ToString(), priority);
                    curState.DefaultNext = new SpanishLexerMachineOutput("", 1, true, singleComb.Length);
                }
            }
            else
            {
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
        }
        private void FindOccasionalCombs(FrenchWord word, List<(int, int)> list, int mask)
        {
            var s = word.Content;
            var ccl = word.CharCombList;
            var node = ccl.First?.Next?.Next;
            while (node != null)
            {
                if (node.Value.Comb == "i" && node.Next?.Value?.Comb == "e" && node.Next?.Next != null
                    && FrenchConstant.IsConsonantComb(node.Previous.Value) && FrenchConstant.IsConsonantComb(node.Previous.Previous.Value))
                {
                    var p = node.Previous;
                    ccl.Remove(node.Next);
                    ccl.Remove(node);
                    node = ccl.AddAfter(p, new FrenchCharComb("ie", node.Value.StartPos)).Next.Next?.Next;
                }
                else
                    node = node.Next;
            }

            var ueindex = -3;
            node = ccl.First;
            while ((ueindex = s.IndexOf("ueil", ueindex + 4)) != -1)
            {
                if (ueindex == 0)
                    continue;
                if ((mask & BitMask.GetMask(ueindex, 2)) != 0)
                    continue;
                if (s[ueindex - 1] == 'c' || s[ueindex - 1] == 'g')
                {
                    while (node.Next != null)
                    {
                        node = node.Next;
                        if (node.Value.StartPos == ueindex)
                        {
                            ccl.Remove(node.Next);
                            var p = node.Previous;
                            ccl.Remove(node);
                            node = ccl.AddAfter(p, new FrenchCharComb("ue", ueindex));
                            break;
                        }
                    }
                }
            }
        }
        public List<(int, int)> FindAllCombs(FrenchWord word)
        {
            var letterUsedFlags = 0;
            var ans = new List<(int, int)>();

            if (combInSpecialWords.ContainsKey(word.Content))
            {
                var k = combInSpecialWords[word.Content];
                ans.Add(k);
                letterUsedFlags |= BitMask.GetMask(k.Item1, k.Item2);
            }
            foreach (var (a, b, c) in combInSpecialRegex)
            {
                if (a.IsMatch(word.Content))
                {
                    ans.Add((b, c));
                    letterUsedFlags |= BitMask.GetMask(b, c);
                }
            }
            if (multiCombInSpecialWords.ContainsKey(word.Content))
            {
                foreach (var specialComb in multiCombInSpecialWords[word.Content])
                {
                    ans.Add(specialComb);
                    letterUsedFlags |= BitMask.GetMask(specialComb.Item1, specialComb.Item2);
                }
            }
            if (letterUsedFlags != 0)
                word.ContainsSpecialPron();

            var w = "^" + word.Content + '$';
            foreach (var ruleClout in LexerRules)
            {
                var cache = "";
                var state = ruleClout[cache];
                for (int i = 0; i < w.Length;)
                {
                    char c = w[i];
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
                            if (output == state.DefaultNext) // 
                            {
                                i = startPos + 2;
                                cache = "";
                                state = ruleClout[cache];
                                continue;
                            }
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

            var usedLetters = -1;
            w = word.Content;
            foreach (var (a, b) in ans)
            {
                for (int i = usedLetters + 1; i < a; i++)
                {
                    word.CharCombList.AddLast(new FrenchCharComb(w.Substring(i, 1), i));
                }
                word.CharCombList.AddLast(new FrenchCharComb(w.Substring(a, b), a));
                usedLetters = a + b - 1;
            }
            for (int i = usedLetters + 1; i < w.Length; i++)
                word.CharCombList.AddLast(new FrenchCharComb(w.Substring(i, 1), i));
            FindOccasionalCombs(word, ans, letterUsedFlags);
            return ans;
        }

        public void Fixup()
        {
            LexerRules[0]["illi"].DefaultNext = new SpanishLexerMachineOutput("", 2, true, 3);
        }
    }

    public class FrenchLexer
    {
        public FrenchLexerMachine Machine { get; }

        public FrenchLexer()
        {
            Machine = new FrenchLexerMachine();
            Machine.Init(@"
aan
aa
ae
aon
aou
au
ay
bb
ccT@^IE
ch
ck
cqu
ct$
dd
eau
ed$
eds$
es$
//er$
//ent$
ez$
eu
^exc
^exs
^ex
ey
ff
guT@IE
gn
ggT@^IE
illiTV
il$
ill
ll
mm
mn
œu
nn
ng$
oo
ou
oy
ph
pp
qu
rr
sch
scT@IE
sç
ss
tch
tt
tz
//ueTil
uy
vr$
zz

@DecPriority
ai
aî
ei
eî
oi
oî

");
            Machine.Fixup();
            var t = @"
arguer 2 1
tramway 2 1
wolfram
hamster
bayadère 1 1
mayonnaise
papaye
cayenne
ayen
bayard 1 1
bayonne
bayonette
cayenne 1 1
himalaya 5 1
fayette
mayence
totem 3 1
stagnant 3 1
agnostique
buggy 2 2
gentilhomme 4 2
mille 2 2
ville
village
tranquille
distiller
osciller
bacille
séquoia 4 1
sept 2 2
septième
baptême
compter
compteur
dompter
forsythia 5 2
asthme
isthme
gruyère 2 1

";
            string lastComb = null;
            foreach (var line in t.Split('\n'))
            {
                var lineq = line.Trim();
                if (lineq.Length <= 2 || lineq[0] == lineq[1] && lineq[0] == '/')
                    continue;
                var s = lineq.Split(' ');
                if (s.Length == 3)
                {
                    var sp = int.Parse(s[1]);
                    var len = int.Parse(s[2]);
                    FrenchLexerMachine.AddSpecialWord(s[0], sp, len);
                    lastComb = s[0].Substring(sp, len);
                }
                else if (s.Length == 1)
                {
                    FrenchLexerMachine.AddSpecialWord(s[0], s[0].IndexOf(lastComb), lastComb.Length);
                }
            }

        }
        public void FindAllCombs(FrenchWord word)
        {
            Machine.FindAllCombs(word);
        }
    }
}
