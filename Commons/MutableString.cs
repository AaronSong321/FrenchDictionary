using System;

namespace Jmas.Commons
{
    public class MutableString
    {
        private char[] content;
        public int Length { get; private set; }
        public int Capacity { get; private set; }

        public MutableString(string s)
        {
            Capacity = Length + Length;
            content = new char[Capacity];
            for (int i = 0; i < s.Length; i++)
            {
                content[i] = s[i];
            }
            Length = s.Length;
        }
        public MutableString(int capacity = 12)
        {
            Capacity = capacity > 0 ? capacity : 12;
            content = new char[Capacity];
        }

        public char this[int index]
        {
            get
            {
                if (index >= 0 && index < Length)
                    return content[index];
                else
                    throw new IndexOutOfRangeException($"MutableString index {index} out of range 0..{Length}");
            }
            set
            {
                if (index >= 0 && index < Length)
                    content[index] = value;
                else
                    throw new IndexOutOfRangeException($"MutableString index {index} out of range 0..{Length}");
            }
        }
        //public string this[Range range]
        //{
        //    get
        //    {
        //        return new string(content[range]);
        //    }
        //}

        public void Append(string s)
        {
            var newLen = Length + s.Length;
            if (newLen > Capacity)
            {
                Capacity = newLen + newLen;
                var a = new char[Capacity];
                for (int i = 0; i < Length; i++)
                {
                    a[i] = content[i];
                }
                content = a;
            }
            for (int i = 0, j = Length; i < s.Length; i++, j++)
            {
                content[j] = s[i];
            }
            Length = newLen;
        }
        public void Append(char c)
        {
            var newLen = Length + 1;
            if (newLen > Capacity)
            {
                Capacity = Length + Length;
                var a = new char[Capacity];
                for (int i = 0; i < Length; i++)
                {
                    a[i] = content[i];
                }
                content = a;
            }
            content[Length] = c;
            Length = newLen;
        }


        public override string ToString()
        {
            return new string(content, 0, Length);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is MutableString ms)
                return ToString() == ms.ToString();
            else
                return base.Equals(obj);
        }
    }
}
