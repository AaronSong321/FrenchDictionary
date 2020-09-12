using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public class FrenchWordException: Exception
    {
        public FrenchWordException() : base() { }
        public FrenchWordException(string message) : base(message) { }
    }

    public class ParserPanic : FrenchWordException
    {
        public ParserPanic(string message) : base(message) { }
    }

    public class UnrecognizedComb : FrenchWordException
    {
        public UnrecognizedComb(string message) : base(message) { }
        public UnrecognizedComb(FrenchCharComb comb) : this($"Char comb {comb.Comb} unrecognized") { }
    }
}
