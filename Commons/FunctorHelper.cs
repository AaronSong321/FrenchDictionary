using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jmas.Commons
{
    public static class FunctorHelper
    {
        public static TRet Invoke<TRet>(this object obj, string methodName, Type[] paramList, object[] argList)
        {
            var type = obj.GetType();
            var method = type.GetMethod(methodName, paramList);
            if (method == null)
                throw new ArgumentException("method does not exist");
            return (TRet)method.Invoke(obj, argList);
        }
    }
}
