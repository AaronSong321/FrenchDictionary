using Jmas.SpanishDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public class FrenchWord
    {
        public string OriginString { get; }
        public string Content { get; }
        public bool Special { get; private set; }
        public LinkedList<FrenchCharComb> CharCombList { get; }
        public LinkedList<FrenchSyllable> SyllableList { get; }
        private string pron;
        public string Pron
        {
            get
            {
#if DEBUG
                if (pron == null)
                    throw new FrenchWordException("Pron has not been set.");
#endif
                else return pron;
            }
            set
            {
#if DEBUG
                if (pron != null) 
                    throw new FrenchWordException("Pron cannot been set twice.");
#endif
                else pron = value;
            }
        }
        public void SetSpeicalPron(string pron)
        {
#if DEBUG
            if (pron != null)
                throw new FrenchWordException("Pron cannot been set twice.");
#endif
            this.pron = pron;
            Special = true;
        }
        public void ContainsSpecialPron() { Special = true; }
        public FrenchWord(string content)
        {
            OriginString = content;
            Content = Reduce(content);
            CharCombList = new LinkedList<FrenchCharComb>();
            SyllableList = new LinkedList<FrenchSyllable>();
        }

        string Reduce(string origin)
        {
            return origin.Replace('à', 'a').Replace('â', 'a').Replace('û', 'u').Replace('ù', 'u');
        }
        public string GetCharCombString()
        {
            var sb = new StringBuilder(50);
            foreach (var c in CharCombList)
            {
                sb.Append($"{c},");
            }
            return sb.ToString();
        }

        public string GetSyllableString()
        {
            var sb = new StringBuilder(80);
            foreach (var syll in SyllableList)
            {
                sb.Append('(');
                for (var cc = syll.FirstComb; cc != syll.LastComb.Next; cc = cc.Next)
                {
#if DEBUG
                    if (!cc.Value.IsMuted && cc.Value.Syll != syll)
                        throw new MismatchSyllableException();
#endif
                    var m = cc.Value.IsMuted;
                    if (cc.Value.DeterminedDuringSplit)
                        sb.Append(cc.Value.GetPron());
                    else if (!m)
                        sb.Append(cc.Value);
                    if (cc != syll.LastComb)
                    {
                        if (!m)
                            sb.Append(',');
                    }
                    else
                    {
                        if (m)
                            sb.Remove(sb.Length - 1, 1);
                        sb.Append(')');
                    }
                }
            }
            return sb.ToString();
        }

        public string GetPronString()
        {
            StringBuilder sb = new StringBuilder(32);
            foreach (var cc in CharCombList)
            {
                if (cc.Syll == null || cc.IsMuted)
                    continue;
                if (cc.Syll.Emphasized && cc.Syll != SyllableList.Last.Value && cc.Syll.FirstComb.Value == cc)
                    sb.Append('\'');
                sb.Append(cc.GetPron());
            }
            return sb.ToString();
        }

        public bool HasNextChars(FrenchCharComb cc, int n)
        {
            return cc.StartPos + cc.Comb.Length + n <= Content.Length;
        }
        public string GetNextChars(FrenchCharComb cc, int n)
        {
            return Content.Substring(cc.StartPos + cc.Comb.Length, n);
        }
        public bool HasPreviousChars(FrenchCharComb cc, int n) => cc.StartPos - n >= 0;
        public string GetPreviousChars(FrenchCharComb cc, int n) => Content.Substring(cc.StartPos - n, n);
        public bool SingleSyllable() => SyllableList.Count == 1;
        public bool HasTailClosedSyllable_Contains(FrenchCharComb cc) => cc.Syll == SyllableList.Last.Value && cc.Syll.VowelComb == cc.Syll.LastComb;
    }
}
