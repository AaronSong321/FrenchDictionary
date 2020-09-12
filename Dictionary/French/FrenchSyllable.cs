using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.FrenchDictionary
{
    public class FrenchSyllable
    {
        public LinkedListNode<FrenchCharComb> FirstComb { get; set; }
        public LinkedListNode<FrenchCharComb> LastComb { get; set; }
        public LinkedListNode<FrenchCharComb> VowelComb { get; set; }
        internal int Number { get; set; }
        public bool Emphasized { get; set; }
    }
}
