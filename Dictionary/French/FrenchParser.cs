using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static Jmas.FrenchDictionary.FrenchConstant;

namespace Jmas.FrenchDictionary
{
    public class FrenchParser
    {
        enum State { E, V, C, SV, CacheConsonant }
        State st;
        FrenchSyllable syl;
        FrenchWord word;
        LinkedListNode<FrenchCharComb> cacheLast;
        int number;

        public void Parse(FrenchWord word)
        {
            MarkSpecialMute(word);
            SplitBadCharComb(word);
            SplitCharComb(word);
            MarkNasal(word);
            MarkMuteTrail(word);
            MarkNearVowelSchwa(word);
            CutSyllable(word);
            MarkVCECVSchwa(word);
            Emphasize(word);
        }

        protected virtual void MarkNasal(FrenchWord word)
        {
            LinkedListNode<FrenchCharComb> MergeNext(LinkedListNode<FrenchCharComb> comb)
            {
                var t = comb.Next.Value.Comb;
                word.CharCombList.Remove(comb.Next);
                if (comb.Previous != null)
                {
                    var p = comb.Previous;
                    word.CharCombList.Remove(comb);
                    return word.CharCombList.AddAfter(p, new FrenchCharComb(comb.Value.Comb + t, comb.Value.StartPos));
                }
                else
                {
                    word.CharCombList.Remove(comb);
                    return word.CharCombList.AddFirst(new FrenchCharComb(comb.Value.Comb + t, comb.Value.StartPos));
                }
            }
            for (var comb = word.CharCombList.First; comb != null; comb = comb.Next)
            {
                // mm, nn, mn is char comb
                // will deal with e elsewhere
                if (IsNasalizable(comb.Value.Comb) && comb.Next != null && (comb.Next.Value.Comb == "m" || comb.Next.Value.Comb == "n"))
                    if (comb.Next.Next == null || !IsVowelComb(comb.Next.Next.Value))
                    {
                        comb = MergeNext(comb);
                    }
            }
        }

        public static readonly Dictionary<string, (int, bool)> SpecialTails = new Dictionary<string, (int, bool)>();
        static FrenchParser()
        {
            var w = @"
baobab 1
club 1
tub 1
snob 1
nabab 1
longtemps 3 0
amygdale 3 0
ginkgo 4 0
imbroglio 5 0
tabac 0
estomac 0
blanc 0
franc 0
tronc 0
banc 0
jonc 0
ajonc 0
porc 0
croc 0
almanach 0
aspect 0
respect
sud 1
lied
djihad
alfred
david
madrid
æuf 0
bæuf
cerf
nerf
clef
aulne 2 0
fils 2 0
pouls 3 0
cap 1
croup
hop
stop
abrupt 4 1
monsieur 0
fils 1
mars
sens
albatros
albinos
os
lis
cactus
campus
six 1
dix
net 1
dot
chut
abrupt
";

            bool? lastPron = null;
            foreach (var t in w.Split('\n'))
            {
                var a = t.Trim();
                if (a.Length <= 2)
                    continue;
                if (a.Substring(0, 2) == "//")
                    continue;
                var b = a.Split(' ');
                if (b.Length == 2)
                {
                    lastPron = int.Parse(b[1]) == 1;
                    SpecialTails[b[0]] = (-1, lastPron.Value);
                }
                else if (b.Length == 3)
                {
                    SpecialTails[b[0]] = (int.Parse(b[1]), int.Parse(b[2]) == 1);
                    lastPron = null;
                }
                else if (b.Length == 1)
                {
                    SpecialTails[b[0]] = (-1, lastPron.Value);
                }
            }
        }
        protected virtual void MarkSpecialMute(FrenchWord word)
        {
            if (SpecialTails.ContainsKey(word.Content))
            {
                var (sp, read) = SpecialTails[word.Content];
                if (sp == -1)
                    if (read)
                        word.CharCombList.Last.Value.MarkAsNonMutable(true);
                    else
                        word.CharCombList.Last.Value.MarkAsTrailingConsonant();
                else
                    foreach (var a in word.CharCombList)
                        if (a.StartPos == sp)
                        {
                            if (read)
                                a.MarkAsNonMutable(true);
                            else
                                a.MarkAsTrailingConsonant();
                            break;
                        }
            }
        }
        protected virtual void MarkMuteTrail(FrenchWord word)
        {
            if (word.CharCombList.Count == 1)
                return;
            var last = word.CharCombList.Last;
            var lvc = last.Value.Comb;
            if (lvc == "r" && IsVerb(word.OriginString))
            {
                if (word.HasPreviousChars(last.Value, 1) && (word.GetPreviousChars(last.Value, 1)[0] == 'e'))
                    last.Value.MarkAsVerbER();
            }
            if (lvc == "e")
            {
                if (HasVowelBefore(word))
                    last.Value.MarkAsTrailingSchwa();
            }
            else if (lvc == "es")
            {
                if (HasVowelBefore(word))
                    last.Value.MarkAsTrailingConsonant();
            }
            else if (MutedTrail.Contains(last.Value.Comb))
            {
                last.Value.MarkAsTrailingConsonant();
                if (last.Previous != null)
                {
                    var lastp = last.Previous.Value;
                    if (IsConsonantComb(lastp) && MutedTrail.Contains(lastp.Comb))
                        lastp.MarkAsTrailingConsonant();
                }
            }

            bool HasVowelBefore(FrenchWord __word)
            {
                var hasVowelBefore = false;
                for (var n = __word.CharCombList.First; n != __word.CharCombList.Last; n = n.Next)
                {
                    if (IsVowelComb(n.Value))
                    {
                        hasVowelBefore = true;
                        break;
                    }
                }

                return hasVowelBefore;
            }
        }

        protected void Dissemble(FrenchWord word, LinkedListNode<FrenchCharComb> cc, int cutPos)
        {
            var ccl = word.CharCombList;
            var ccc = cc.Value.Comb;
            ccl.AddAfter(cc, new FrenchCharComb(ccc.Substring(cutPos, ccc.Length-cutPos), cc.Value.StartPos + cutPos));
            ccl.AddAfter(cc, new FrenchCharComb(ccc.Substring(0, cutPos), cc.Value.StartPos));
            ccl.Remove(cc);
        }
        protected virtual void SplitBadCharComb(FrenchWord word)
        {
            var ccl = word.CharCombList;
            var fvc = ccl.First.Value.Comb;
            if (fvc == "ill")
            {
                Dissemble(word, ccl.First, 1);
            }
            if (ccl.Last.Value.Comb == "er")
            {
                if (!(IsVerb(word.Content)))
                    Dissemble(word, ccl.Last, 1);
                if (word.Content == "premier")
                {
                    Dissemble(word, ccl.Last, 1);
                    word.ContainsSpecialPron();
                }
            }
        }
        protected virtual void SplitCharComb(FrenchWord word)
        {
            LinkedListNode<FrenchCharComb> Split2(LinkedListNode<FrenchCharComb> __n, string __pron1, string __pron2, bool __isSpecial = false)
            {
                var __li = __n.List;
                var (__t1, __t2) = __n.Value.Split2(__pron1, __pron2, __isSpecial);
                var __p1 = __n.Next;
                __li.AddAfter(__n, __t2);
                __li.AddAfter(__n, __t1);
                __li.Remove(__n);
                return __p1;
            }
            LinkedListNode<FrenchCharComb> Split3(LinkedListNode<FrenchCharComb> __n, string __pron1, string __pron2, string __pron3, bool __isSpecial = false)
            {
                var __li = __n.List;
                var (__t1, __t2, __t3) = __n.Value.Split3(__pron1, __pron2, __pron3, __isSpecial);
                var __p1 = __n.Next;
                __li.AddAfter(__n, __t3);
                __li.AddAfter(__n, __t2);
                __li.AddAfter(__n, __t1);
                __li.Remove(__n);
                return __p1;
            }

            //var a = "pbtdkgfvlszʃʒmnɲŋRɥwjieɛayøœuoɔɑəɛ̃œ̃ɔ̃ɑ̃";
            var node = word.CharCombList.First;
            var wc = word.OriginString;
            while (node != null)
            {
                var nvc = node.Value.Comb;
                var didSplit = false;
                if (nvc == "ex")
                {
                    var nodeContainingX = DissembleEx(word, node);
                    var nextComb = nodeContainingX.Next.Value;
                    if (nextComb != null && IsVowelComb(nextComb))
                        node = Split2(nodeContainingX, "g", "z");
                    else
                        node = Split2(nodeContainingX, "k", "s");
                    didSplit = true;
                }
                else if (nvc == "exc" || nvc == "exs")
                {
                    var nodeContainingX = DissembleEx(word, node);
                    node = Split2(nodeContainingX, "k", "s");
                    didSplit = true;
                }
                else if (nvc == "ie")
                {
                    node = Split3(node, "i", "j", "e");
                    didSplit = true;
                }
                else if (nvc == "ill" || nvc == "illi")
                {
                    if (node.Previous != null && IsConsonantComb(node.Previous.Value))
                    {
                        node = Split2(node, "i", "j");
                        didSplit = true;
                    }
                }
                else if (nvc == "ay")
                {
                    if (node.Next == null)
                        node.Value.SetPron("ɛ");
                    else if (node.Next.Value.IsSchwa())
                    {
                        node = Split2(node, "ɛ", "j");
                        didSplit = true;
                    }
                    else if (IsVowelComb(node.Next.Value))
                    {
                        node = Split2(node, "e", "j");
                        didSplit = true;
                    }
                    else if (wc == "pays" || wc == "paysage" || wc == "paysan" || wc == "abbaye")
                    {
                        node = Split2(node, "e", "i", true);
                        didSplit = true;
                    }
                    else
                    {
                        throw new ParserPanic($"Unrecognized ay");
                    }
                }
                else if (nvc == "ey")
                {
                    if (node.Next == null)
                        node.Value.SetPron("ɛ");
                    else if (node.Next.Value.IsSchwa())
                    {
                        node = Split2(node, "ɛ", "j");
                        didSplit = true;
                    }
                    else if (IsVowelComb(node.Next.Value))
                    {
                        node = Split2(node, "e", "j");
                        didSplit = true;
                    }
                    else
                        node.Value.SetPron("e");
                }
                else if (nvc == "il")
                {
                    if (wc == "gentilhomme")
                    {
                        node = Split2(node, "i", "j", true);
                        didSplit = true;
                    }
                }
                else if (nvc == "ll")
                {
                    if (wc == "llanos")
                    {
                        node = Split2(node, "l", "j", true);
                        didSplit = true;
                    }
                }
                else if (nvc == "mn")
                {
                    if (word.Content != "automne" && word.Content != "condamner")
                    {
                        node = Split2(node, "m", "n");
                        didSplit = true;
                    }
                }
                else if (nvc == "oo")
                {
                    if (wc == "alcool")
                    {
                        node = Split2(node, "ɔ", "ɔ", true);
                        didSplit = true;
                    }
                    else if (wc == "zoo" || wc == "zoologie")
                    {
                        node = Split2(node, "o", "o", true);
                        didSplit = true;
                    }
                }
                else if (nvc == "oy")
                {
                    if (node.Next != null && IsVowelComb(node.Next.Value))
                    {
                        node = Split3(node, "w", "a", "j");
                        didSplit = true;
                    }
                }
                else if (nvc == "qu")
                {
                    if (wc == "quatrilatère" || wc == "quark")
                    {
                        node = Split2(node, "k", "w", true);
                        didSplit = true;
                    }
                }
                else if (nvc == "um")
                {
                    if (wc == "kumquat")
                    {
                        node = Split2(node, "u", "m", true);
                        didSplit = true;
                    }
                }
                else if (nvc == "uy")
                {
                    node = Split3(node, "ɥ", "i", "j");
                    didSplit = true;
                }
                else if (nvc == "x")
                {
                    node = Split2(node, "k", "s");
                    didSplit = true;
                }
                else if (nvc == "y")
                {
                    if (node.Previous?.Previous != null)
                    {
                        var nppv = node.Previous.Previous.Value;
                        if (IsConsonantComb(nppv) && node.Previous.Value.Comb == "l" || node.Previous.Value.Comb == "r")
                            if (node.Next != null && IsVowelComb(node.Next.Value))
                            {
                                node = Split2(node, "i", "j");
                                didSplit = true;
                            }
                    }
                }
                if (!didSplit)
                    node = node.Next;
            }

            LinkedListNode<FrenchCharComb> DissembleEx(FrenchWord __word, LinkedListNode<FrenchCharComb> __node)
            {
                Dissemble(__word, __node, 1);
                word.CharCombList.First.Value.SetPron("ɛ");
                return word.CharCombList.First.Next;
            }
        }
        protected void MarkNearVowelSchwa(FrenchWord word)
        {
            var node = word.CharCombList.First;
            while (node != null)
            {
                if (node.Value.Comb != "e")
                {
                    node = node.Next;
                    continue;
                }
                if (node.Previous != null && IsVowelComb(node.Previous.Value))
                {
                    var npvc = node.Previous.Value.Comb;
                    if (npvc != "o" && !IsSemiVowelComb(npvc)
                        && node.Next != null && IsVowelComb(node.Next.Value))
                        node.Value.MarkAsSchwaNearVowel();
                }
                else
                    if (node.Next != null && IsVowelComb(node.Next.Value))
                    node.Value.MarkAsSchwaNearVowel();
                node = node.Next?.Next;
            }
        }
        protected void MarkVCECVSchwa(FrenchWord word)
        {
            var node = word.CharCombList.First.Next?.Next;
            while (node != null)
            {
                if (node.Value.Comb != "e")
                {
                    node = node.Next;
                    continue;
                }
                if (node.Next?.Next == null)
                    break;
                else
                {
                    var nv = node.Next.Value;
                    if (IsConsonantComb(nv))
                    {
                        var nvc = nv.Comb;
                        if (nvc.Length == 2 && IsConsonantChar(nvc[0]) && nvc[0] == nvc[1])
                        {
                            node = node.Next.Next;
                            continue;
                        }
                    }
                }
                if (IsVowelComb(node.Previous.Previous.Value) && IsConsonantComb(node.Previous.Value) && IsConsonantComb(node.Next.Value) && IsVowelComb(node.Next.Next.Value))
                {
                    node.Value.MarkAsSchwaVCECV();
                    node = node.Next.Next.Next?.Next;
                }
                else
                    node = node.Next.Next;
            }
        }
        protected virtual void Read(LinkedListNode<FrenchCharComb> comb)
        {
            if (comb.Value.IsMuted)
            {
                var mutedReason = comb.Value.MuteReason;
                if (mutedReason == FrenchCharCombMuteReason.SchwaVCECV || mutedReason == FrenchCharCombMuteReason.SchwaNearVowel)
                {
                    if (st == State.CacheConsonant)
                    {
                        syl.LastComb = cacheLast;
                        cacheLast.Value.SetSyllable(syl);
                        cacheLast = null;
                        syl = new FrenchSyllable();
                        word.SyllableList.AddLast(syl);
                        st = State.E;
                    }
                    else if (st == State.SV)
                    {
                        st = State.V;
                    }
                }
                else if (mutedReason == FrenchCharCombMuteReason.TailingConsonant || mutedReason == FrenchCharCombMuteReason.TailingSchwa)
                {
                    if (st == State.CacheConsonant)
                    {
                        syl.LastComb = cacheLast;
                        cacheLast.Value.SetSyllable(syl);
                        cacheLast = null;
                    }

                    if (comb.Next != null && !comb.Next.Value.IsMuted)
                    {
                        syl = new FrenchSyllable();
                        word.SyllableList.AddLast(syl);
                    }
                    if (st != State.C) // to be done in function ReadEndOfWord
                        st = State.E;
                }
                return;
            }

            if (st == State.E || st == State.C)
            {
                if (st == State.E)
                    syl.FirstComb = comb;
                if (IsSemiVowelComb(comb.Value))
                {
                    st = State.SV;
                    syl.VowelComb = comb;
                }
                else if (IsVowelComb(comb.Value))
                {
                    st = State.V;
                    syl.VowelComb = comb;
                }
                else
                {
                    st = State.C;
                }
                comb.Value.SetSyllable(syl);
                syl.LastComb = comb;
            }
            else if (st == State.V)
            {
                if (IsVowelComb(comb.Value) || IsSemiVowelComb(comb.Value))
                {
                    //var sp = comb.Value.StartPos;
                    //if (word.OriginString[sp] == 'ï' && (word.Content[sp - 1] == 'a' || word.Content[sp - 1] == 'o'))

                    syl = new FrenchSyllable()
                    {
                        FirstComb = comb,
                        VowelComb = comb,
                        LastComb = comb,
                        Number = number++
                    };
                    word.SyllableList.AddLast(syl);
                    comb.Value.SetSyllable(syl);
                    if (IsSemiVowelComb(comb.Value))
                        st = State.SV;
                }
                else
                {
                    st = State.CacheConsonant;
                    cacheLast = comb;
                }
            }
            else if (st == State.SV)
            {
                if (IsSemiVowelComb(comb.Value))
                {
                    comb.Value.SetSyllable(syl);
                    syl.VowelComb = comb;
                    syl.LastComb = comb;
                }
                else if (IsVowelComb(comb.Value))
                {
                    comb.Value.SetSyllable(syl);
                    st = State.V;
                    syl.LastComb = comb;
                    syl.VowelComb = comb;
                }
                else
                {
                    st = State.CacheConsonant;
                    cacheLast = comb;
                }
            }
            else if (st == State.CacheConsonant)
            {
                if (IsSemiVowelComb(comb.Value))
                {
                    syl = new FrenchSyllable()
                    {
                        FirstComb = cacheLast,
                        VowelComb = comb,
                        LastComb = comb,
                        Number = number++
                    };
                    word.SyllableList.AddLast(syl);
                    cacheLast.Value.SetSyllable(syl);
                    comb.Value.SetSyllable(syl);
                    st = State.SV;
                }
                else if (IsVowelComb(comb.Value))
                {
                    syl = new FrenchSyllable()
                    {
                        FirstComb = cacheLast,
                        VowelComb = comb,
                        LastComb = comb,
                        Number = number++
                    };
                    word.SyllableList.AddLast(syl);
                    cacheLast.Value.SetSyllable(syl);
                    comb.Value.SetSyllable(syl);
                    st = State.V;
                }
                else
                {
                    syl.LastComb = cacheLast;
                    cacheLast.Value.SetSyllable(syl);
                    syl = new FrenchSyllable()
                    {
                        FirstComb = comb,
                        LastComb = comb,
                        Number = number++
                    };
                    word.SyllableList.AddLast(syl);
                    comb.Value.SetSyllable(syl);
                    st = State.C;
                }
            }
            if (st != State.CacheConsonant)
                cacheLast = null;
        }
        protected void CutSyllable(FrenchWord word)
        {
            number = 0; st = State.E; syl = new FrenchSyllable() { Number = number++ }; cacheLast = null; this.word = word; word.SyllableList.AddLast(syl);
            for (var comb = word.CharCombList.First; comb != null; comb = comb.Next)
            {
                Read(comb);
            }
            ReadEndOfWord();
        }
        protected virtual void ReadEndOfWord()
        {
            if (st == State.CacheConsonant)
            {
                syl.LastComb = cacheLast;
                cacheLast.Value.SetSyllable(syl);
            }
            else if (st == State.C)
            {
                var toBeLast = word.SyllableList.Last.Previous.Value;
                for (var cons = syl.FirstComb; cons != null && cons.Value.Syll == syl; cons = cons.Next)
                {
                    cons.Value.ChangeSyllable(toBeLast);
                }
                toBeLast.LastComb = syl.LastComb;
                word.SyllableList.RemoveLast();
            }
        }

        protected void Emphasize(FrenchWord word)
        {
            // special emphasize ??
            word.SyllableList.Last.Value.Emphasized = true;
        }
    }
}
