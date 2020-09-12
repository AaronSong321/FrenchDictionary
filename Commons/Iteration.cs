using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.Commons
{
    public static class Iteration
    {

        public static IEnumerable<string> ForeachNonTrivialLine(string bigString)
        {
            string s = bigString;
            foreach (var line in s.Split('\n'))
            {
                var lineq = line.Trim();
                if (lineq.Length > 2 && (lineq[0] != '/' || lineq[1] != '/'))
                {
                    yield return lineq;
                }
            }
        }
        // line number start from 0
        public static int ForeachNonTrivialLineDo(ref string bigString, Action<int, string> action)
        {
            var lineNumber = 0;
            foreach (var line in bigString.Split('\n'))
            {
                var lineq = line.Trim();
                if (lineq.Length <= 2 || lineq[0] == '/' && lineq[1] == '/')
                {
                    action(lineNumber++, lineq);
                }
            }
            return lineNumber;
        }
    }
}
