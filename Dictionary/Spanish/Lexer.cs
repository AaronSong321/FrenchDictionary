

namespace Jmas.SpanishDictionary
{
    public class Lexer
    {
        public LexerMachine Machine { get; }

        public Lexer()
        {
            Machine = new LexerMachine();
        }
        public void Init(string rules)
        {
            Machine.Init(rules);
        }

        public void CutCombs(SpanishWord word)
        {
            var cutResult = Machine.FindAllCombs(word);
            var usedLetters = -1;
            var w = word.Content;

            foreach (var (a, b) in cutResult)
            {
                for (int i = usedLetters + 1; i < a; i++)
                {
                    word.CharCombList.AddLast(new SpanishCharComb(w[i], i));
                }
                word.CharCombList.AddLast(new SpanishCharComb(w.Substring(a, b), a));
                usedLetters = a + b - 1;
            }
            for (int i = usedLetters + 1; i < w.Length; i++)
                word.CharCombList.AddLast(new SpanishCharComb(w[i], i));
        }
    }

}
