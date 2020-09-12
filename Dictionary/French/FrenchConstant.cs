using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public static class FrenchConstant
    {
        public static readonly string VowelChars = "aeiouéèàùâêîôûëïäöüyœæ";
        public static readonly string ConsonantChars = "bcçdfghjklmnpqrstvwxz";
        public static readonly HashSet<string> NasalizableVowelCC = new HashSet<string>
        {
            "a","e","ai","ei","oi","u","i","y","o","eu",
        };
        public static readonly HashSet<string> VowelMultiCharCombs = new HashSet<string> { "aan", "aa", "ae", "aon", "aou", "au", "ay", "eau", "ed", "eds", "er", "es", "eu", "eû", "ey", "œu", "oo", "ou", "où", "oû", "oy", "ue", "uy", "ai", "ei", "ei", "oi" };
        public static readonly HashSet<string> ConsonantMultiCharCombs = new HashSet<string> { "bb", "cc", "ch", "ck", "cqu", "ct", "dd", "ff", "gn", "gg", "illi", "il", "ill", "ll", "mm", "mn", "nn", "ph", "pp", "pt", "qu", "rr", "ss", "sch", "sc", "sç", "tch", "tt", "tz", "vr", "zz", 
        };
        public static readonly HashSet<string> ElongateConsonant = new HashSet<string> { "ʁ", "ʒ", "v", "z", "j", "vʁ" };
        public static readonly HashSet<string> ElongateVowel = new HashSet<string> { "ø", "o", "ɛ̃", "œ̃", "ɔ̃", "ɑ̃" };
        public static readonly string SoftVowelChars = "ieyæêéè";
        public static readonly string NonSoftVowelChars = "aeouàùâôûœ";
        public static readonly HashSet<string> SemiVowelComb = new HashSet<string> { "u", "i", "ou", "y", "ï" };
        public static readonly HashSet<string> MutedTrail = new HashSet<string> { "b", "d", "g", "j", "k", "p", "s", "t", "x", "z", "es", "ent" };

        public static readonly string SingleCharVowelPhenomene = "ieɛayøœuoɔɑə";
        public static readonly string VowelPhenomene = "ieɛayøœuoɔɑəɛ̃œ̃ɔ̃ɑ̃";
        public static readonly string ConsonantPhenomene = "pbtdkgfvlszʃʒmnɲŋRɥwj";

        public static bool IsVowelChar(char c) => VowelChars.Contains(c);
        public static bool IsConsonantChar(char c) => ConsonantChars.Contains(c);
        public static bool IsNasalizable(string s) => NasalizableVowelCC.Contains(s);
        public static bool IsSemiVowelComb(string s) => SemiVowelComb.Contains(s);
        public static bool IsSemiVowelComb(FrenchCharComb cc) => SemiVowelComb.Contains(cc.Comb);
        public static bool IsVowelComb(FrenchCharComb cc)
        {
            var c = cc.Comb;
            return cc.PronSet && IsVowelPron(cc)
                || c.Length == 1 && VowelChars.Contains(c[0]) 
                || VowelMultiCharCombs.Contains(c) 
                || c.Length > 1 && IsNasalizable(c.Substring(0, c.Length - 1)) && (c[c.Length - 1] == 'm' || c[c.Length - 1] == 'n');
        }
        public static bool IsVowelPron(FrenchCharComb cc)
        {
            foreach (var p in cc.GetPron())
                if (SingleCharVowelPhenomene.Contains(p))
                    return true;
            return false;
        }
        public static bool IsConsonantComb(FrenchCharComb cc)
        {
            var c = cc.Comb;
            return cc.PronSet && IsConsonantPron(cc) 
                || c.Length == 1 && ConsonantChars.Contains(c[0])
                || ConsonantMultiCharCombs.Contains(c);
        }
        public static bool IsConsonantPron(FrenchCharComb cc)
        {
            return !IsVowelPron(cc);
        }

        public static readonly HashSet<string> verbs = new HashSet<string>
        {
            "neiger","prier","exister",
            "comprendre","prendre","craindre",
            "lire","dire","faire",
            "vouloir",
            "finir",
        };
        public static readonly HashSet<string> nonVerbs = new HashSet<string>
        {
            "mer","cher","fer","fier","ver","amer","aster","hiver","éther","cancer","geyser","premier","enfer",
        };
        public static bool IsVerb(string s)
        {
            if (verbs.Contains(s)) return true;
            if (nonVerbs.Contains(s)) return false;
            if (s.Length >= 2)
            {
                var t = s.Substring(s.Length - 2, 2);
                if (t == "er" || t == "ir" || t == "re")
                    return true;
            }
            return false;
        }
    }
}
