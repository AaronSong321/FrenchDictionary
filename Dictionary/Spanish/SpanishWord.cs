using System;
using System.Collections.Generic;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public class SpanishWord
    {
        public string Content { get; }
        public LinkedList<SpanishCharComb> CharCombList { get; }
        public LinkedList<Syllable> SyllableList { get; }
        private string pron;
        public string Pron
        {
            get
            {
                if (pron == null) throw new PronounciationNotSet(this);
                else return pron;
            }
            set
            {
                if (pron != null) throw new PronSetTwiceException(this);
                else pron = value;
            }
        }
        public SpanishWord(string content)
        {
            Content = content;
            CharCombList = new LinkedList<SpanishCharComb>();
            SyllableList = new LinkedList<Syllable>();
            pron = null;
        }

        public string GetCharCombString()
        {
            var sb = new StringBuilder(50);
            foreach(var c in CharCombList)
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
                sb.Append(syll.Number);
                sb.Append('(');
                for (var cc = syll.FirstComb; cc != syll.LastComb; cc = cc.Next)
                {
                    if (cc.Value.Syll != syll)
                    {
                        throw new MismatchSyllableException();
                    }
                    sb.Append(cc.Value);
                    if (cc != syll.LastComb)
                        sb.Append(",");
                }
                sb.Append(syll.LastComb.Value);
                sb.Append(')');
            }
            return sb.ToString();
        }
    }
}
