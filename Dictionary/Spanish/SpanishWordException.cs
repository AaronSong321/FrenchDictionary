
using Jmas.FrenchDictionary;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public class SpanishWordException: Exception
    {
        public SpanishWordException(string message) : base(message)
        {

        }
    }

    public class PronSetTwiceException: SpanishWordException
    {
        public PronSetTwiceException(SpanishCharComb charComb) : base($"The pronunciation of char combination {charComb} is set twice")
        { }
        public PronSetTwiceException(SpanishWord word) : base($"The pronunciation of word {word.Content} is set twice") { }
    }

    public class LexerStateConflict : SpanishWordException
    {
        public LexerStateConflict(SpanishLexerState state, char input): base($"state {state.State} + {input} is already defined") { }
        public LexerStateConflict(FrenchLexerState state, char input) : base($"state {state.State} + {input} is already defined") { }
    }

    public class MismatchSyllableException : Exception
    {
        public MismatchSyllableException() : base() { }
    }

    public class PronounciationNotSet : Exception
    {
        public PronounciationNotSet(SpanishCharComb cc) : base(cc.Comb) { }
        public PronounciationNotSet(SpanishWord word) : base(word.Content) { }
    }

    public class SpecialCharCombNotMatched : Exception
    {
        public SpecialCharCombNotMatched() : base() { }
    }
}
