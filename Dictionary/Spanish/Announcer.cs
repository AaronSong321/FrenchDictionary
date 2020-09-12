
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public abstract class Announcer
    {
        protected List<Dictionary<string, Func<LinkedListNode<SpanishCharComb>, string>>> AnnouncerRules { get; }
        protected Dictionary<string, (int, int, string)> SingleSpecialRules { get; }
        protected Dictionary<string, List<(int, int, string)>> MultiSpecialRules { get; }
        protected Dictionary<string, string> SpecialWords { get; }
        public Announcer()
        {
            AnnouncerRules = new List<Dictionary<string, Func<LinkedListNode<SpanishCharComb>, string>>>();
            SingleSpecialRules = new Dictionary<string, (int, int, string)>();
            MultiSpecialRules = new Dictionary<string, List<(int, int, string)>>();
            SpecialWords = new Dictionary<string, string>();
        }
        public void SetRuleNumber(int num)
        {
            for (int i = 0; i < num; i++) AnnouncerRules.Add(new Dictionary<string, Func<LinkedListNode<SpanishCharComb>, string>>());
        }

        protected virtual void Emphasize(SpanishWord word)
        {
            foreach (var cc in word.CharCombList)
            {
                if (cc.Comb.Length > 0 && SpanishCharCombHelper.IsEmphasizedVowel(cc.Comb[0]))
                {
                    cc.Syll.Emphasized = true;
                    return;
                }
            }
            if (word.SyllableList.Count > 1)
            {
                var lastChar = word.Content[word.Content.Length-1];
                if ("mnsaeoiuáéíóúy".Contains(lastChar))
                    word.SyllableList.Last.Previous.Value.Emphasized = true;
                else word.SyllableList.Last.Value.Emphasized = true;
            }
            else
                word.SyllableList.First.Value.Emphasized = true;
        }
        public virtual void Announce(SpanishWord word)
        {
            if (SpecialWords.ContainsKey(word.Content))
                word.Pron = SpecialWords[word.Content];
            else
            {
                Emphasize(word);
                if (SingleSpecialRules.ContainsKey(word.Content))
                {
                    var (sp, len, pron) = SingleSpecialRules[word.Content];
                    foreach (var cc in word.CharCombList)
                    {
                        if (cc.StartPos == sp)
                        {
                            cc.SetPron(pron);
                            break;
                        }
                    }
                }
                if (MultiSpecialRules.ContainsKey(word.Content))
                {
                    var k = MultiSpecialRules[word.Content];
                    var specialCombIndex = 0;
                    var sp = k[specialCombIndex].Item1;
                    var cc = word.CharCombList.First;
                    while (specialCombIndex < k.Count && cc.Next != null)
                    {
                        if (cc.Value.StartPos == sp)
                        {
                            var (_, _, pron) = k[specialCombIndex];
                            cc.Value.SetPron(pron);
                            specialCombIndex++;
                        }
                        cc = cc.Next;
                    }
                }
                foreach (var ruleClout in AnnouncerRules)
                {
                    for (var cc = word.CharCombList.First; cc != null; cc = cc.Next)
                    {
                        if (ruleClout.ContainsKey(cc.Value.Comb))
                        {
                            var p = ruleClout[cc.Value.Comb](cc);
                            cc.Value.SetPron(p);
                        }
                    }
                }
                StringBuilder sb = new StringBuilder(32);

                foreach (var cc in word.CharCombList)
                {
                    if (cc.Syll.Emphasized && cc.Syll.FirstComb.Value == cc && word.SyllableList.Count != 1)
                        sb.Append('\'');
                    sb.Append(cc.GetPron());
                }
                word.Pron = sb.ToString();
            }
        }
        protected abstract void BuildRules();
        protected abstract void BuildSpecialRules();
        protected abstract void AddSpecialWords();
    }

    public class SpanishAnnouncer : Announcer
    {
        public void Init()
        {
            SetRuleNumber(2);
            BuildRules();
            BuildSpecialRules();
        }
        protected override void BuildSpecialRules()
        {
            SingleSpecialRules.Add("mexico", (2, 1, "χ"));
        }
        protected override void AddSpecialWords()
        {

        }
        protected override void BuildRules()
        {
            var rule = AnnouncerRules[0];
            rule.Add("a", _ => "a");
            rule.Add("á", rule["a"]);
            rule.Add("b", cc =>
            {
                if (cc.Previous != null)
                {
                    var ccp = cc.Previous.Value.Comb;
                    if (ccp == "m" || ccp == "n")
                        return "b";
                    else
                        return "β";
                }
                else
                    return "b";
            });
            rule.Add("c", cc=>
            {
                if (cc.Next != null)
                {
                    var ccn = cc.Next.Value.Comb;
                    if (ccn == "i" || ccn == "e" || ccn == "y")
                        return "θ";
                }
                return "k";
            });
            rule.Add("ch", _ => "ʧ");
            rule.Add("d", cc =>
            {
                if (cc.Previous != null)
                {
                    var ccp = cc.Previous.Value;
                    if (ccp.Comb == "m" || ccp.Comb == "n" || ccp.Comb == "l")
                        return "d";
                    else
                        return "ð";
                }
                else
                    return "d";
            });
            rule.Add("e", _ => "e");
            rule.Add("é", rule["e"]);
            rule.Add("f", _ => "f");
            rule.Add("g", cc =>
            {
                if (cc.Next != null)
                {
                    var ccn = cc.Next.Value.Comb;
                    if (ccn == "i" || ccn == "e" || ccn == "y")
                        return "χ";
                    else
                    {
                        if (cc.Previous != null)
                        {
                            var ccp = cc.Previous.Value.Comb;
                            if (ccp != "m" && ccp != "n")
                                return "ɣ";
                            else
                                return "g";
                        }
                        else
                            return "g";
                    }
                }
                else
                    if (cc.Previous != null)
                    {
                        var ccp = cc.Previous.Value.Comb;
                        if (ccp != "m" && ccp != "n")
                            return "ɣ";
                        else
                            return "g";
                    }
                    else
                        return "g";
            });
            rule.Add("gu", gu =>
            {
                if (gu.Previous != null)
                {
                    var gup = gu.Previous.Value.Comb;
                    return gup != "m" && gup != "n" ? "ɣw" : "gw";
                }
                else return "gw";
            });
            rule.Add("gü", rule["gu"]);
            rule.Add("h", _ => "");
            rule.Add("i", cc => cc.Value.Syll.VowelComb == cc ? "i" : "j");
            rule.Add("í", _ => "i");
            rule.Add("j", _ => "χ");
            rule.Add("k", _ => "k");
            rule.Add("l", _ => "l");
            rule.Add("ll", _ => "ʎ");
            rule.Add("o", _ => "o");
            rule.Add("ó", rule["o"]);
            rule.Add("p", _ => "p");
            rule.Add("qu", _ => "k");
            rule.Add("r", cc =>
            {
                if (cc.Previous != null)
                {
                    var ccp = cc.Previous.Value;
                    if (ccp.Comb == "s" || ccp.Comb == "n" || ccp.Comb == "l")
                        return "r";
                    else
                        return "ɾ";
                }
                else
                    return "r";
            });
            rule.Add("rr", _ => "r");
            rule.Add("s", cc =>
            {
                if (cc.Next != null)
                {
                    var ccp = cc.Next.Value;
                    if (ccp.Comb == "m" || ccp.Comb == "n" || ccp.Comb == "l")
                        return "z";
                }
                return "s";
            });
            rule.Add("t", _ => "t");
            rule.Add("u", cc => cc.Value.Syll.VowelComb == cc ? "u" : "w");
            rule.Add("ú", _ => "u");
            rule.Add("v", rule["b"]);
            rule.Add("w", _ => "β");
            rule.Add("x", _ => "ks");
            rule.Add("y", cc => cc.Value.Syll.VowelComb == cc ? "i" : "ʝ");
            rule.Add("z", _ => "θ");


            var rule2 = AnnouncerRules[1];
            rule2.Add("m", cc =>
            {
                if (cc.Next != null)
                {
                    if (cc.Next.Value.Comb == "m" || cc.Next.Value.Comb == "n")
                        return "m";
                    else
                    {
                        var ccnp = cc.Next.Value.GetPron();
                        if (ccnp == "f")
                            return "m";
                        else if (ccnp.Length > 0 && SpanishCharCombHelper.IsLabio(ccnp[0]))
                            return "ɱ";
                        else if (ccnp.Length > 0 && CharCombHelper.IsVelarConsonant(ccnp[0]))
                            return "ŋ";
                    }
                }
                return "m";
            });
            rule2.Add("n", cc =>
            {
                if (cc.Next != null)
                {
                    if (cc.Next.Value.Comb == "m" || cc.Next.Value.Comb == "n")
                        return "n";
                    else
                    {
                        var ccnp = cc.Next.Value.GetPron();
                        if (ccnp == "f")
                            return "m";
                        else if (ccnp.Length > 0 && SpanishCharCombHelper.IsLabio(ccnp[0]))
                            return "ɱ";
                        else if (ccnp.Length > 0 && CharCombHelper.IsVelarConsonant(ccnp[0]))
                            return "ŋ";
                    }
                }
                return "n";
            });
        }
    }
}
