using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.Commons
{
    public class BitMask
    {
        public static int GetMask(int low, int len)
        {
            var mask = 0;
            for (int i = 0; i < len; i++)
                mask = (mask << 1) | 1;
            return mask << low;
        }
    }
}
