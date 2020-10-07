using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Analysis
{
    public static class Text
    {
        public static double GetIC(IEnumerable<int> freq, int len)
        {
            double ic = 0;
            foreach (int i in freq)
                ic += i * (i - 1);
            return ic / (len * (len - 1));
        }
        
        public static double GetIC(string text)
        {
            var characters = new char[Alphabet.Length];
            for (var i = 0; i < Alphabet.Length; ++i)
                characters[i] = (char) ('A' + i);

            var frequencies = new int[Alphabet.Length];
            foreach (char c in text)
                ++frequencies[Array.IndexOf(characters, c)];

            return GetIC(frequencies, text.Length);
        }
        
        public static string[] GetSubstring(string text, int length)
        {
            var texts = new List<char>[length];
            for (var i = 0; i < length; ++i)
            {
                texts[i] = new List<char>();
            }
            for (var i = 0; i < text.Length; ++i)
            {
                texts[i % length].Add(text[i]);
            }

            var substring = new string[length];
            for (var i = 0; i < length; ++i)
            {
                substring[i] = new string(texts[i].ToArray());
            }

            return substring;
        }
        
        public static void GetRepetitions(string text)
        {
            var substrLen = 2;
            int count;
            do
            {
                string substring = text.Substring(0, substrLen++);
                count = Regex.Matches(text, substring).Count;
                Console.WriteLine($"{substring} appears {count} times in text.");
            } while (count > 2);
        }

        public static int Kasiski(string text, string pattern)
        {
            int length = pattern.Length;
            var positions = new List<int>();
            
            for (var i = 0; i < text.Length - length; ++i)
            {
                if (text.Substring(i, length) == pattern)
                    positions.Add(i);
            }

            int gcd = positions[0];
            for (var i = 1; i < positions.Count; ++i)
                gcd = GCD(gcd, positions[i]);
            
            return gcd;
        }
        
        private static int GCD(int a, int b)
        {
            while (true)
            {
                if (b == 0) return a;
                int a1 = a;
                a = b;
                b = a1 % b;
            }
        }
    }
}