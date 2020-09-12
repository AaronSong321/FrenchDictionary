using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.SpanishDictionary
{
    public class Syllable
    {
        public LinkedListNode<SpanishCharComb> FirstComb { get; set; }
        public LinkedListNode<SpanishCharComb> LastComb { get; set; }
        public LinkedListNode<SpanishCharComb> VowelComb { get; set; }
        internal int Number { get; set; }
        public bool Emphasized { get; set; }
    }

}
