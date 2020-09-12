using Jmas.SpanishDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public enum FrenchCharCombMuteReason
    {
        NonMuted,
        TailingConsonant,
        TailingSchwa,
        SchwaNearVowel,
        SchwaVCECV,
        VerbER,
        Conjugation,
    }

    public class FrenchCharComb
    {
        public string Comb { get; }
        public bool PronSet { get; private set; }
        private string pron;
        public int StartPos { get; }
        public FrenchSyllable Syll { get; private set; }
        public bool SpecialCombOrPron { get; private set; }


        public void SetSyllable(FrenchSyllable syl)
        {
#if DEBUG
            if (Syll != null)
            {
                throw new FrenchWordException("Syllable double set");
            }
            Syll = syl;
#endif
        }
        public void ChangeSyllable(FrenchSyllable syl)
        {
#if DEBUG
            if (Syll == null)
            {
                throw new FrenchWordException("A char comb without a syllable set to needs not change a syllable");
            }
            if (Syll == syl)
            {
                throw new FrenchWordException("Change to the same syllable");
            }
#endif
            Syll = syl;
        }
        public override string ToString()
        {
            return Comb;
        }
        public bool IsSplit { get; }
        public bool DeterminedDuringSplit { get; }
        public int SplitIndex { get; }
        public bool NonMutable { get; private set; }
        public bool Elongated { get; private set; }

        public FrenchCharCombMuteReason MuteReason { get; private set; } = FrenchCharCombMuteReason.NonMuted;
        public bool IsMuted => MuteReason != FrenchCharCombMuteReason.NonMuted;

        public FrenchCharComb(string comb, int startPos, bool isSpecial = false)
        {
            Comb = Reduce(comb);
            StartPos = startPos;
            SpecialCombOrPron = isSpecial;
        }
        public FrenchCharComb(string comb, int startPos, int splitIndex, bool isSpecial = false) : this(comb, startPos, isSpecial)
        {
            IsSplit = true;
            SplitIndex = splitIndex;
        }
        public FrenchCharComb(string comb, int startPos, int splitIndex, string splitDeterminedPron, bool isSpecial = false) : this(comb, startPos, splitIndex, isSpecial)
        {
            DeterminedDuringSplit = true;
            pron = splitDeterminedPron;
            PronSet = true;
        }

        string Reduce(string origin)
        {
            var t = origin.Replace('ä', 'a').Replace('ï', 'i').Replace('ö', 'o');
            if (t.Length > 1) t = t.Replace('î', 'i');
            return t != "gü" ? origin.Replace('ü', 'u') : t;
        }
        public string GetPron()
        {
#if DEBUG
            if (!PronSet)
                throw new FrenchWordException($"FrenchCharComb {Comb} has not been set a pronunciation.");
#endif
            return Elongated ? pron + ':' : pron;
        }
        public void SetPron(string pron)
        {

#if DEBUG
            if (IsMuted)
                throw new FrenchWordException($"FrenchCharComb {Comb} is muted and cannot be set a pronunciation.");
            if (PronSet)
                throw new FrenchWordException($"FrenchCharComb {Comb} has been set a pronunciation.");
#endif
            PronSet = true;
            this.pron = pron;
        }
        public void SetSpecialPron(string pron)
        {
#if DEBUG
            if (PronSet)
            {
                throw new FrenchWordException($"FrenchCharComb {Comb} has been set a pronunciation.");
            }
#endif
            SpecialCombOrPron = true;
            PronSet = true;
            this.pron = pron;
        }

        public (FrenchCharComb, FrenchCharComb) Split2(string pron1 = null, string pron2 = null, bool isSpecial = false)
        {
#if DEBUG
            if (PronSet)
                throw new FrenchWordException("A determined char comb cannot be split.");
#endif
            var fcc1 = pron1 == null ? new FrenchCharComb(Comb, StartPos, 1, isSpecial) : new FrenchCharComb(Comb, StartPos, 1, pron1, isSpecial);
            var fcc2 = pron2 == null ? new FrenchCharComb(Comb, StartPos, 2, isSpecial) : new FrenchCharComb(Comb, StartPos, 2, pron2, isSpecial);
            return (fcc1, fcc2);
        }
        public (FrenchCharComb, FrenchCharComb, FrenchCharComb) Split3(string pron1 = null, string pron2 = null, string pron3 = null, bool isSpecial = false)
        {
#if DEBUG
            if (PronSet)
                throw new FrenchWordException("A determined char comb cannot be split.");
#endif
            var fcc1 = pron1 == null ? new FrenchCharComb(Comb, StartPos, 1, isSpecial) : new FrenchCharComb(Comb, StartPos, 1, pron1, isSpecial);
            var fcc2 = pron2 == null ? new FrenchCharComb(Comb, StartPos, 2, isSpecial) : new FrenchCharComb(Comb, StartPos, 2, pron2, isSpecial);
            var fcc3 = pron3 == null ? new FrenchCharComb(Comb, StartPos, 3, isSpecial) : new FrenchCharComb(Comb, StartPos, 2, pron3, isSpecial);
            return (fcc1, fcc2, fcc3);
        }

        private void Mute()
        {
#if DEBUG
            if (MuteReason != FrenchCharCombMuteReason.NonMuted || PronSet)
                throw new FrenchWordException();
#endif
        }
        public void MarkAsTrailingSchwa()
        {
            Mute();
            if (!NonMutable)
            MuteReason = FrenchCharCombMuteReason.TailingSchwa;
        }
        public void MarkAsTrailingConsonant()
        {
            Mute();
            if (!NonMutable)
                MuteReason = FrenchCharCombMuteReason.TailingConsonant;
        }
        public void MarkAsSchwaNearVowel()
        {
            Mute();
            if (!NonMutable)
                MuteReason = FrenchCharCombMuteReason.SchwaNearVowel;
        }
        public void MarkAsSchwaVCECV()
        {
            Mute();
            if (!NonMutable)
                MuteReason = FrenchCharCombMuteReason.SchwaVCECV;
        }
        public void MarkAsVerbER()
        {
            Mute();
            if (!NonMutable)
                MuteReason = FrenchCharCombMuteReason.VerbER;
        }
        public void MarkAsConjugation()
        {
            Mute();
            if (!NonMutable)
                MuteReason = FrenchCharCombMuteReason.Conjugation;
        }

        public bool IsSchwa()
        {
            return MuteReason == FrenchCharCombMuteReason.SchwaNearVowel 
                || MuteReason == FrenchCharCombMuteReason.SchwaVCECV 
                || MuteReason == FrenchCharCombMuteReason.TailingSchwa;
        }

        public void MarkAsNonMutable(bool isSpecial)
        {
            NonMutable = true;
            SpecialCombOrPron = isSpecial;
        }
        public void Elongate()
        {
            Elongated = true;
        }
    }

    public static class FrenchCharCombHelper
    {
        public static LinkedListNode<FrenchCharComb> MergeNext(LinkedListNode<FrenchCharComb> cc)
        {
            var a = cc.Value.Comb + cc.Next.Value.Comb;
            var b = new FrenchCharComb(a, cc.Value.StartPos);
            var l = cc.List;
            var p = cc.Previous;
            l.Remove(cc.Next);
            l.Remove(cc);
            return p == null ? l.AddAfter(p, b) : l.AddFirst(b);
        }
    }
}
