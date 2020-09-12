using Jmas.SpanishDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public class SpanishParser
    {
        private readonly SpanishParserMachine machine;

        public SpanishParser()
        {
            machine = new SpanishParserMachine();
        }
        public void Parse(SpanishWord word)
        {
            machine.Parse(word);
        }
    }

    public class SpanishParserMachine
    {
        enum State { E, V, C, SV, CacheConsonant }
        State st;
        Syllable syl;
        SpanishWord word;
        LinkedListNode<SpanishCharComb> cacheLast;
        int number;

        public void Parse(SpanishWord word) 
        {
            number = 0; st = State.E; syl = new Syllable() { Number = number++ }; cacheLast = null; this.word = word; word.SyllableList.AddLast(syl); 
            for (var comb = word.CharCombList.First; comb != null; comb = comb.Next)
            {
                Read(comb);
            }
            ReadEndOfWord();
        }
        protected virtual void Read(LinkedListNode<SpanishCharComb> comb)
        {
            if (st == State.E || st == State.C)
            {
                if (st == State.E)
                    syl.FirstComb = comb;
                if (SpanishCharCombHelper.IsSemiVowelComb(comb.Value) || SpanishCharCombHelper.IsYComb(comb.Value))
                {
                    st = State.SV;
                    syl.VowelComb = comb;
                }
                else if (SpanishCharCombHelper.IsVowelComb(comb.Value))
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
                if (SpanishCharCombHelper.IsSemiVowelComb(comb.Value))
                {
                    syl.LastComb = comb;
                    comb.Value.SetSyllable(syl);
                }
                else if (SpanishCharCombHelper.IsYComb(comb.Value))
                {
                    st = State.CacheConsonant;
                    cacheLast = comb;
                    comb.Value.SetSyllable(syl);
                }
                else if (SpanishCharCombHelper.IsVowelComb(comb.Value))
                {
                    syl = new Syllable()
                    {
                        FirstComb = comb,
                        VowelComb = comb,
                        LastComb = comb,
                        Number = number++
                    };
                    word.SyllableList.AddLast(syl);
                    comb.Value.SetSyllable(syl);
                }
                else
                {
                    st = State.CacheConsonant;
                    cacheLast = comb;
                }
            }
            else if (st == State.SV)
            {
                if (SpanishCharCombHelper.IsSemiVowelComb(comb.Value))
                {
                    comb.Value.SetSyllable(syl);
                    syl.VowelComb = comb;
                    syl.LastComb = comb;
                }
                else if (SpanishCharCombHelper.IsYComb(comb.Value))
                {
                    st = State.CacheConsonant;
                    cacheLast = comb;
                }
                else if (SpanishCharCombHelper.IsVowelComb(comb.Value))
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
                if (SpanishCharCombHelper.IsSemiVowelComb(comb.Value) || SpanishCharCombHelper.IsYComb(comb.Value))
                {
                    syl = new Syllable()
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
                else if (SpanishCharCombHelper.IsVowelComb(comb.Value))
                {
                    syl = new Syllable()
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
                    syl = new Syllable()
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
        protected void ReadEndOfWord()
        {
            if (st == State.CacheConsonant)
            {
                syl.LastComb = cacheLast;
                cacheLast.Value.SetSyllable(syl);
            }
            else if (st == State.C)
            {
                var toBeLast = word.SyllableList.Last.Previous.Value;
                for (var cons = syl.FirstComb; cons != null; cons = cons.Next)
                {
                    cons.Value.ChangeSyllable(toBeLast);
                }
                toBeLast.LastComb = syl.LastComb;
                word.SyllableList.RemoveLast();
            }
        }
    }
}
