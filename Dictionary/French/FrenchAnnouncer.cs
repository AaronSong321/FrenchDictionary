using Jmas.Commons;
using Jmas.SpanishDictionary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Jmas.FrenchDictionary.FrenchConstant;

namespace Jmas.FrenchDictionary
{
    public class FrenchAnnouncer
    {
        protected abstract class AnnouncerRule
        {
            public abstract string GetPron(LinkedListNode<FrenchCharComb> comb, FrenchWord word);
        }
        protected class LambdaRule : AnnouncerRule
        {
            readonly Func<LinkedListNode<FrenchCharComb>, FrenchWord, string> read;
            public LambdaRule(Func<LinkedListNode<FrenchCharComb>, FrenchWord, string> r) { read = r; }
            public override string GetPron(LinkedListNode<FrenchCharComb> comb, FrenchWord word)
            {
                return read(comb, word);
            }
        }
        protected class InstantRule : AnnouncerRule
        {
            readonly string ins;
            public InstantRule(string instant) { ins = instant; }
            public override string GetPron(LinkedListNode<FrenchCharComb> comb, FrenchWord word)
            {
                return ins;
            }
        }
        protected class IfRule : AnnouncerRule
        {
            readonly Func<LinkedListNode<FrenchCharComb>, FrenchWord, bool> condition;
            readonly string pron1;
            readonly string pron2;
            public IfRule(Func<LinkedListNode<FrenchCharComb>, FrenchWord, bool> condition, string pronTrue, string pronFalse) { this.condition = condition;pron1 = pronTrue;pron2 = pronFalse; }
            public override string GetPron(LinkedListNode<FrenchCharComb> comb, FrenchWord word)
            {
                return condition(comb, word) ? pron1 : pron2;
            }
        }
        protected List<Dictionary<string, AnnouncerRule>> AnnouncerRules { get; }
        protected Dictionary<string, (int, int, string)> SingleSpecialRules { get; }
        protected Dictionary<string, List<(int, int, string)>> MultiSpecialRules { get; }
        protected Dictionary<string, string> SpecialWords { get; }
        public FrenchAnnouncer()
        {
            AnnouncerRules = new List<Dictionary<string, AnnouncerRule>>();
            SingleSpecialRules = new Dictionary<string, (int, int, string)>();
            MultiSpecialRules = new Dictionary<string, List<(int, int, string)>>();
            SpecialWords = new Dictionary<string, string>();
            Init();
        }
        public void SetRuleNumber(int num)
        {
            for (int i = 0; i < num; i++) AnnouncerRules.Add(new Dictionary<string, AnnouncerRule>());
        }

        protected static AnnouncerRule Lambda(Func<LinkedListNode<FrenchCharComb>, FrenchWord, string> r) => new LambdaRule(r);
        protected static AnnouncerRule Ins(string pron) => new InstantRule(pron);
        protected static AnnouncerRule If(Func<LinkedListNode<FrenchCharComb>, FrenchWord, bool> condition, string pronTrue, string pronFalse) => new IfRule(condition, pronTrue, pronFalse);

        public virtual void Announce(FrenchWord word)
        {
            if (SpecialWords.ContainsKey(word.OriginString))
            {
                word.SetSpeicalPron(SpecialWords[word.OriginString]);
            }
            else
            {
                if (SingleSpecialRules.ContainsKey(word.OriginString))
                {
                    var (sp, _, pron) = SingleSpecialRules[word.OriginString];
                    foreach (var cc in word.CharCombList)
                    {
                        if (cc.StartPos == sp)
                        {
                            cc.SetPron(pron);
                            word.ContainsSpecialPron();
                            break;
                        }
                    }
                }
                if (MultiSpecialRules.ContainsKey(word.OriginString))
                {
                    var k = MultiSpecialRules[word.OriginString];
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
                            word.ContainsSpecialPron();
                        }
                        cc = cc.Next;
                    }
                }
                foreach (var ruleClout in AnnouncerRules)
                {
                    for (var cc = word.CharCombList.First; cc != null; cc = cc.Next)
                    {
                        if (cc.Value.PronSet || cc.Value.IsMuted)
                            continue;
                        if (ruleClout.ContainsKey(cc.Value.Comb))
                        {
                            cc.Value.SetPron(ruleClout[cc.Value.Comb].GetPron(cc, word));
                        }
                    }
                }
                Elongate(word);
                word.Pron = word.GetPronString();
            }
        }
        protected void Elongate(FrenchWord word)
        {
            var lastSyllable = word.SyllableList.Last.Value;
            var lastVowel = lastSyllable.VowelComb.Value;
            var ConsAfterVowel = lastSyllable.VowelComb.Next;
            if (ConsAfterVowel == lastSyllable.LastComb && (ElongateConsonant.Contains(ConsAfterVowel.Value.GetPron()) || ElongateVowel.Contains(lastVowel.GetPron())))
                lastVowel.Elongate();
        }

        protected void BuildRules()
        {
            bool SomeConsonantSchwaConsonantVowel(LinkedListNode<FrenchCharComb> comb) {
                var n1 = comb.Next;
                var n2 = n1?.Next;
                var n3 = n2?.Next;
                var n4 = n3?.Next;
                return n4 != null && IsConsonantComb(n1.Value) && n2.Value.IsSchwa() && IsConsonantComb(n3.Value) && IsVowelComb(n4.Value);
            }

            var r1 = AnnouncerRules[0];
            var r2 = AnnouncerRules[1];
            r1["aa"] = r1["a"] = Ins("a");
            r1["aan"] = Ins("ɑ̃");
            r1["ae"] = r1["æ"] = Ins("e");
            r1["ai"] = Lambda((ai, word) =>
            {
                if (ai.Value.Syll.Emphasized)
                    return "ɛ";
                if (SomeConsonantSchwaConsonantVowel(ai))
                    return "ɛ";
                if (word.HasNextChars(ai.Value, 3) && !word.HasNextChars(ai.Value, 4) && word.GetNextChars(ai.Value, 3) == "rie")
                    return "ɛ";
                return "e";
            });
            r1["aim"] = r1["ain"] = Ins("ɛ̃");
            r1["am"] = r1["an"] = r1["aon"] = Ins("ɑ̃");
            r1["aou"] = Ins("u");
            r1["au"] = If((ai, word) => word.HasNextChars(ai.Value, 1) && word.GetNextChars(ai.Value, 1) == "r", "ɔ", "o");
            r1["ay"] = Ins("ɛ");
            r1["b"] = Lambda((ai, word) =>
            {
                var aiv = ai.Value;
                if (word.HasNextChars(aiv, 1))
                {
                    var t = word.GetNextChars(aiv, 1)[0];
                    if (t == 's' || t == 't')
                    {
                        if (word.HasPreviousChars(aiv, 1) && !word.HasPreviousChars(aiv, 2))
                        {
                            var t1 = word.GetPreviousChars(aiv, 1)[0];
                            if (t1 == 'a' || t1 == 'o')
                                return "p";
                        }
                        else if (word.HasPreviousChars(aiv, 2) && !word.HasPreviousChars(aiv, 3))
                        {
                            var t1 = word.GetPreviousChars(aiv, 2);
                            if (t1 == "su")
                                return "p";
                        }
                    }
                }
                return "b";
            });
            r1["bb"] = Ins("b");
            r1["c"] = Lambda((ai, word) =>
            {
                var aiv = ai.Value;
                if (!word.HasNextChars(aiv, 1))
                    return "k";
                if (word.HasNextChars(aiv, 2) && word.GetNextChars(aiv, 2) == "œu")
                    return "k";
                return SoftVowelChars.Contains(word.GetNextChars(aiv, 1)[0]) ? "s" : "k";
            });
            r1["cc"] = Ins("k");
            r1["ch"] = If((ai, word) => word.HasNextChars(ai.Value, 1) && word.GetNextChars(ai.Value, 1)[0] == 'r', "k", "ʃ");
            r1["ck"] = r1["cqu"] = r1["k"] = r1["cc"];
            r1["ct"] = Ins("ct");
            r1["d"] = r1["dd"] = Ins("d");
            r1["e"] = Lambda((e, word) =>
            {
                var ev = e.Value;
                if (ev.IsMuted)
                    return "";
                var env = e.Next?.Value;
                var ennv = e.Next?.Next?.Value;
                var haspc1 = word.HasPreviousChars(ev, 1);
                var isSingleSyll = word.SingleSyllable();
                var isLastSyll = ev.Syll == word.SyllableList.Last.Value;
                var empha = ev.Syll.Emphasized;
                if (env != null)
                {
                    var envc = env.Comb;
                    if (envc.Length == 2 && IsConsonantChar(envc[0]) && envc[0] == envc[1])
                    {
                        if ((envc[0] == 's' || envc[0] == 'f') && !haspc1)
                            return "e";
                        if (envc[0] == 's' && haspc1 && !word.HasPreviousChars(ev, 2))
                        {
                            if (word.GetPreviousChars(ev, 1)[0] == 'd')
                                return "e";
                            else if (word.GetPreviousChars(ev, 1)[0] == 'r')
                                return "ə";
                        }
                        if (envc[0] == 'm')
                            return haspc1 ? "a" : "ɑ̃";
                        if (envc[0] == 'n')
                        {
                            if (empha)
                                return "ɛ";
                            if (!haspc1)
                                return "ɑ̃";
                            return "e";
                        }
                        //return isLastSyll ? "ɛ" : "e";
                        return "ɛ";
                    }
                    if (envc == "r" && IsVerb(word.OriginString))
                        return "e";
                    if (ennv == null && (envc == "t" || envc == "ct"))
                        return "e";
                    if (word.HasNextChars(ev, 2))
                    {
                        var nextc2 = word.GetNextChars(ev, 2);
                        if (nextc2 == "il")
                        {
                            return isLastSyll ? "ɛ" : "e";
                        }
                        else if (nextc2 == "sc" || nextc2 == "ck")
                            return "e";
                    }
                    var epv = e.Previous?.Value;
                    var eppv = e.Previous?.Previous?.Value;
                    if (epv == null && envc == "n")
                        return "ɑ̃";
                    if (epv != null && eppv != null && IsConsonantComb(epv) && IsConsonantComb(eppv) && IsConsonantComb(env))
                        return "ə";
                    var isFirstSyll = ev.Syll == word.SyllableList.First.Value;
                    if (isFirstSyll && epv != null && IsConsonantComb(epv) && ev.Syll.LastComb == e)
                        return "ə";
                    if (ev.Syll.LastComb != e)
                        return "ɛ";
                }
                else
                {
                    if (isSingleSyll) return "ə";
                }

                throw new UnrecognizedComb(ev);
            });
            r1["é"] = Lambda((e, word) => SomeConsonantSchwaConsonantVowel(e) ? "ɛ" : "e");
            r1["è"] = Ins("ɛ");
            r1["ê"] = If((e, word) => e.Value.Syll.Emphasized || SomeConsonantSchwaConsonantVowel(e), "ɛ", "e");
            r1["eau"] = Ins("o");
            r1["ed"] = r1["eds"] = Ins("e");
            r1["ei"] = If((ei, word) => ei.Value.Syll.Emphasized, "ɛ", "e");
            r1["ein"] = Ins("ɛ̃");
            r1["em"] = Ins("ɑ̃");
            r1["en"] = Lambda((en, word) =>
            {
                if (word.HasPreviousChars(en.Value, 1))
                {
                    var enpc1 = word.GetPreviousChars(en.Value, 1)[0];
                    if (enpc1 == 'i' || enpc1 == 'y' || enpc1 == 'é')
                        return "ɛ̃";
                }
                return "ɑ̃";
            });
            r1["es"] = r1["ez"] = Ins("e");
            r2["eu"] = If((eu, word) =>
            {
                if (eu.Previous == null
                    || eu.Value.Syll == word.SyllableList.Last.Value && eu.Value.Syll.LastComb == eu)
                    return true;
                var eunc = eu.Next?.Value?.GetPron();
                if (eunc != null && (eunc[0] == 't' || eunc[0] == 'z' || eunc[0] == 'd' || eunc[0] == 'ʒ'))
                    return true;
                return false;
            }, "ø", "œ");
            r1["eun"] = Ins("ɛ̃");
            r1["f"] = r1["ff"] = Ins("f");
            r1["g"] = If((g, word) => word.HasNextChars(g.Value, 1) && SoftVowelChars.Contains(word.GetNextChars(g.Value, 1)[0]), "ʒ", "g");
            r1["ge"] = Ins("ʒ");
            r1["gn"] = Ins("ɲ");
            r1["gg"] = Ins("g");
            r1["gu"] = If((gu, word) => word.HasNextChars(gu.Value, 1) && word.OriginString[gu.Value.StartPos + 2] == 'ï', "gɥ", "g");
            r1["h"] = Ins("");
            r1["i"] = r1["ï"] = If((i, word) => i.Value.Syll.VowelComb == i, "i", "j");
            r1["î"] = Ins("i");
            r1["il"] = r1["ill"] = r1["illi"] = Ins("j");
            r1["im"] = r1["in"] = Ins("ɛ̃");
            r1["j"] = Ins("ʒ");
            r1["ll"] = r1["l"] = Ins("l");
            r1["m"] = r1["mm"] = Ins("m");
            r1["mn"] = r1["n"] = r1["nn"] = Ins("n");
            r1["ng"] = Ins("ŋ");
            r2["o"] = Lambda((o, word) =>
            {
                var ov = o.Value;
                if (word.HasTailClosedSyllable_Contains(ov)
                    || o.Next?.Value?.GetPron() == "z"
                    || word.HasNextChars(ov, 4) && word.GetNextChars(ov, 4) == "tion"
                    || word.HasNextChars(ov, 2) && !word.HasNextChars(ov, 3) && (word.GetNextChars(ov, 2) == "me" || word.GetNextChars(ov, 2) == "ne"))
                    return "o";
                return "ɔ";
            });
            r1["ô"] = Ins("o");
            r1["œ"] = Ins("e");
            r1["œu"] = If((oeu, word) => word.HasTailClosedSyllable_Contains(oeu.Value), "ø", "œ");
            r1["oi"] = Ins("wa");
            r1["oin"] = r1["oim"] = Ins("wɛ̃");
            r1["om"] = r1["on"] = Ins("ɔ̃");
            r1["ou"] = Lambda((ou, word) =>
            {
                var origin = word.OriginString.Substring(ou.Value.StartPos, 2);
                if (origin == "oû" || origin == "où")
                    return "u";
                return ou.Value.Syll.VowelComb == ou ? "u" : "w";
            });
            r1["oy"] = Ins("wa");
            r1["pt"] = Ins("t");
            r1["ph"] = Ins("f");
            r1["p"] = r1["pp"] = Ins("p");
            r1["q"] = r1["qu"] = r1["cc"];
            r1["r"] = r1["rr"] = Ins("ʁ");
            r1["s"] = If((s, word) => word.HasPreviousChars(s.Value, 1) && IsVowelChar(word.GetPreviousChars(s.Value, 1)[0]) && word.HasNextChars(s.Value, 1) && IsVowelChar(word.GetNextChars(s.Value, 1)[0]), "z", "s");
            r1["sc"] = r1["sç"] = Ins("s");
            r1["sch"] = r1["sh"] = Ins("ʃ");
            r1["ss"] = Ins("s");
            r1["t"] = Lambda((t, word) =>
            {
                var n1 = t.Next?.Value;
                var n2 = t.Next?.Next?.Value;
                return n2 != null && n1.Comb == "i" && IsVowelComb(n2) && t.Previous != null && t.Previous.Value.Comb != "s" ? "s" : "t";
            });
            r1["tch"] = Ins("ʧ");
            r1["tt"] = Ins("t");
            r1["tz"] = Ins("ts");
            r1["u"] = Lambda((u, word) =>
            {
                if (word.OriginString[u.Value.StartPos] == 'û')
                    return "u";
                return u.Value.Syll.VowelComb == u ? "y" : "ɥ";
            });
            r1["ue"] = Ins("œ");
            r1["um"] = If((um, word) => um.Next == null, "ɔm", "œ̃");
            r1["un"] = Ins("œ̃");
            r1["v"] = Ins("v");
            r1["vr"] = Ins("vʁ");
            r1["w"] = Ins("w");
            r1["y"] = r1["i"];
            r1["ym"] = r1["yn"] = r1["im"];
            r1["zz"] = r1["z"] = Ins("z");

            //    "aeiouéèàùâêîôûëïäöüyœæ";
            // "bcçdfghjklmnpqrstwxz";
            //"pbtdkgfvlszʃʒmnɲŋʁɥwjieɛayøœuoɔɑəɛ̃œ̃ɔ̃ɑ̃";
        }

        public void BuildSpecialRules(string p = null)
        {
            var sw = SpecialWords;
            sw["papeterie"] = "papεtʁi";
            sw["et cetera"] = "e sətəʁa";
            sw["vice versa"] = "vis vεrsa";
            sw["Saint Gaudens"] = "sɛ̃ godɛn";
            sw["Rubens"] = "ʁyben";
            sw["Stendhal"] = "stɑ̃dal";
            sw["touer"] = "tue";
            sw["trouer"] = "tʁue";
            sw["est"] = "ɛst";
            sw["ouest"] = "wεst";
            sw["influence"] = "ɛ̃flyɑ̃:s";
            sw["kumquat"] = "kumkuat";
            sw["bruyère"] = "bryjεʁ";
            sw["interviewer"] = "ɛ̃tεʁvjuve";
            sw["interview"] = "ɛ̃tεʁvju";
            sw["dix-neuf"] = "diznœf";
            sw["mezzo-soprano"] = "mεdzo sɔpʁano";
            var a = p ?? @"
pâte 1 1 ɑ
mât 1 1 ɑ
faisant 1 2 ə
faisable 1 2 ə
faiseur 1 2 ə
satisfaisant 6 2 ə
faisan 1 2 ə
vraiment 2 2 ɛ
aiglon 0 2 ɛ
aigrelet 0 2 ɛ
gai 1 2 e
quai 2 2 e
ai 0 2 e
eurai 3 2 e
serai 3 2 e
fraîcheur 2 2 ɛ
paul 1 2 ɔ
mauvais 1 2 ɔ
augmenter 0 2 ɔ
// 'ay' needs split
//pays 1 2 ei
//paysage
//paysan
//abbaye
auréole
subsister 2 1 b
subside 
second 2 1 g
seconder 
zinc 3 1 g
chaos 0 2 k
chœur
écho
orchestre
psychologie
archaïque
orchidée
technique
chrétien
chlore
munich
moloch
machiavel
varech
sandwich 6 2 ʧ
macho
meilleur 1 1 ɛ
et 0 1 e
septième 1 1 e
clef
interpeller 6 1 ə
agneler
atelier
placebo 4 1 ə
céleri 1 1 ɛ
événement 2 1 ɛ
têtard 1 1 ɛ
bêtement
êta
bêta
thêta
treizième 2 2 ɛ
seizième
lemme 1 1 ɛ
examen 4 2 ɛ̃
appendice
rhododendron
agenda
mémneto
pentagone
benzine
pensum
benjamin
agen
heureux 1 2 ø
//neuro-
//leuco-
bleuir 2 2 y
bleuâtre
// avoir -> eu
clef 2 1 e
tungstène 3 1 k
immangeable 0 1 ɛ̃
immanquable
jazz 0 1 dʒ
Jeep
jet
jota 0 1 χ
fjord 1 1 j
oseille 0 1 ɔ
myosostis
grosse 2 1 o
fosse
grossir
grossesse
posséder 1 1 o
kilogramme
coco
hôpital 1 1 o
hôtel
oignon 0 2 ɔ
boom 1 2 u
hooligan
monsieur 1 2 ə
subsister 3 1 z
subsistance 3 1 z
transitif
transit
transaction
Alsace
vraisemblable 4 1 s
antisocial
parasol
monosyllabique
préséance
tournesol
cosignataire
asymptote
fascisme 2 2 ʃ
ischion 2 2 k
schizophrénie
forsythia 5 2 s
club 2 1 œ
tub
parfum 4 2 œ̃
lumbago 1 2 ɔ̃
secundo 3 2 ɔ̃
wagon 0 1 v
weber
six 2 1 s
dix
soixante
Bruxelles
deuxième 3 1 z
sixième
dixième
partie 3 1 t
introvertie
sortie
centième
amitié
pitié
abricotier
héritier
quartier
entier
matière
laitière
chrétien
ortie
tritium 3 1 t
étions
tiens
retiens
étiez
étiolement
zêta 0 1 dz
pizza 2 2 dz
";

            //    "aeiouéèàùâêîôûëïäöüyœæ";
            // "bcçdfghjklmnpqrstwxz";
            //"pbtdkgfvlszʃʒmnɲŋʁɥwjieɛayøœuoɔɑəɛ̃œ̃ɔ̃ɑ̃";
            void AddSpecialRule(string __s, int __sp, int __len, string __pron)
            {
                var __sinh = SingleSpecialRules.ContainsKey(__s);
                var __mulh = MultiSpecialRules.ContainsKey(__s);
                if (!__sinh && !__mulh)
                    SingleSpecialRules[__s] = (__sp, __len, __pron);
                else if (!__sinh)
                    MultiSpecialRules[__s].Add((__sp, __len, __pron));
                else if (!__mulh)
                {
                    var (l1, l2, l3) = SingleSpecialRules[__s];
                    SingleSpecialRules.Remove(__s);
                    MultiSpecialRules[__s] = new List<(int, int, string)>
                    {
                        (l1, l2, l3), (__sp, __len, __pron)
                    };
                }
            }
            string lastWord = null;
            string lastComb = null;
            string lastPron = null;
            foreach (var line in Iteration.ForeachNonTrivialLine(a))
            {
                var split = line.Split(' ');
                if (split.Length == 4)
                {
                    var sp = int.Parse(split[1]);
                    var len = int.Parse(split[2]);
                    AddSpecialRule(split[0], sp, len, split[3]);
                    lastWord = split[0];
                    lastComb = split[0].Substring(sp, len);
                }
                else if (split.Length == 3)
                {
                    var sp = int.Parse(split[0]);
                    var len = int.Parse(split[1]);
                    AddSpecialRule(lastWord, sp, len, split[2]);
                    lastComb = lastWord.Substring(sp, len);
                }
                else if (split.Length == 1)
                {
                    var sp = split[0].IndexOf(lastComb);
                    AddSpecialRule(split[0], sp, lastComb.Length, lastPron);
                    lastWord = null; // last word is unimportant
                }
            }
        }

        public void Init()
        {
            var a = "pbtdkgfvlszʃʒmnɲŋRɥwjieɛayøœuoɔɑəɛ̃œ̃ɔ̃ɑ̃";
            var sr = new StreamWriter("qq.txt");
            foreach (var c in a)
            {
                int d = c;
                sr.WriteLine($"{c}\t{d:X}");
            }
            sr.Close();
            SetRuleNumber(2);
            BuildSpecialRules();
            BuildRules();
        }
    }
}
