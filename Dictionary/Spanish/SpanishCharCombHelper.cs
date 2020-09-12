using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public static class SpanishCharCombHelper
    {
        public static HashSet<char> VowelComb { get; } = new HashSet<char>
        {
            'a','e','i','o','u','y','á','é','í','ó','ú'
        };
        public static bool IsVowelComb(SpanishCharComb comb)
        {
            var c = comb.Comb;
            return c.Length == 1 && VowelComb.Contains(c[0]);
        }
        public static bool IsConsonantComb(SpanishCharComb comb) => !IsVowelComb(comb);
        public static bool IsYComb(SpanishCharComb comb) => comb.Comb == "y";
        public static bool IsSemiVowelComb(SpanishCharComb comb) => comb.Comb == "u" || comb.Comb == "i";
        public static HashSet<char> Labio { get; } = new HashSet<char>
        {
            'm','p','b','f','v'
        };
        public static bool IsLabio(char c) => Labio.Contains(c);
        public static HashSet<char> EmphasizedVowel { get; } = new HashSet<char>
        {
            'á','é','í','ó','ú'
        };
        public static bool IsEmphasizedVowel(char c) => EmphasizedVowel.Contains(c);
    }
}
