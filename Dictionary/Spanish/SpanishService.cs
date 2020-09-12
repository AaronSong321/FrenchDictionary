using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public class SpanishService
    {
        private Lexer spanishLexer = new Lexer();

        private SpanishParser spanishParser = new SpanishParser();
        private SpanishAnnouncer spanishAnnouncer = new SpanishAnnouncer();

        public SpanishService()
        {
            spanishLexer.Init(@"
ch
quTi
quTe
guTi
guTe
güTi
güTe
ll
rr
");
            spanishAnnouncer.Init();
        }

        public void AnalyzeWordPron(SpanishWord word)
        {
            spanishLexer.CutCombs(word);
            spanishParser.Parse(word);
            spanishAnnouncer.Announce(word);
        }
        public string GetWordPron(string word)
        {
            var a = new SpanishWord(word);
            AnalyzeWordPron(a);
            return a.Pron;
        }
    }
}
