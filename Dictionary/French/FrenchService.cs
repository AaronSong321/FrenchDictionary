using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public class FrenchService
    {
        public readonly FrenchLexer Lexer;
        public readonly FrenchParser Parser;
        public readonly FrenchAnnouncer Announcer;

        public FrenchService()
        {
            Lexer = new FrenchLexer();
            Parser = new FrenchParser();
            Announcer = new FrenchAnnouncer();
        }

        public string GetPron(FrenchWord w)
        {
            Lexer.FindAllCombs(w);
            Parser.Parse(w);
            Announcer.Announce(w);
            return w.Pron;
        }
    }
}
