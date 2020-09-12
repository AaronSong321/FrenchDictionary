using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{

    public class SpanishCharComb
    {
        public string Comb { get; }
        public bool PronSet { get; private set; }
        private string pron;
        public int StartPos { get; }
        public Syllable Syll { get; private set; }
        public bool SpecialPron { get; private set; }

        public SpanishCharComb(string comb, int startPos)
        {
            Comb = comb;
            StartPos = startPos;
        }
        public SpanishCharComb(char comb, int startPos) : this(comb.ToString(), startPos) { }

        public virtual string GetPron()
        {
#if DEBUG
            if (!PronSet)
            {
                throw new PronounciationNotSet(this);
            }
            else
            {
#endif
                return pron;
#if DEBUG
            }
#endif
        }
        public virtual void SetPron(string pron)
        {
#if DEBUG
            if (PronSet)
            {
                throw new PronSetTwiceException(this);
            }
            else
            {
#endif
                PronSet = true;
                this.pron = pron;
#if DEBUG
            }
#endif
        }
        public void SetSpecialPron(string pron)
        {
#if DEBUG
            if (PronSet)
            {
                throw new PronSetTwiceException(this);
            }
            else
            {
#endif
                SpecialPron = true;
                PronSet = true;
                this.pron = pron;
#if DEBUG
            }
#endif
        }

        public void SetSyllable(Syllable syl)
        {
            if (Syll != null)
            {
                // this is not a major bug, to be fixed later
                Console.WriteLine("this is suspicious.");
            }
            Syll = syl;
        }
        public void ChangeSyllable(Syllable syl)
        {
            Syll = syl;
        }
        public override string ToString()
        {
            return Comb;
        }
    }

    public static class CharCombHelper
    {
        public static HashSet<char> VelarConsonant { get; } = new HashSet<char>
        {
            'ŋ','k','g','χ','ɣ','w'
        };
        public static bool IsVelarConsonant(char c) => VelarConsonant.Contains(c);
    }
}
