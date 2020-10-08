using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Analysis
{
    public readonly struct Text
    {
        private readonly string _text;
        private readonly int _length;
        private readonly int[] _frequencies;

        public Text(string text)
        {
            _text = text.ToUpper().Trim();
            _length = text.Length;
            _frequencies = new int[Alphabet.Length];
            foreach (char c in _text)
                ++_frequencies[Array.IndexOf(Alphabet.Charset, c)];
        }

        public void PrintFrequencies()
        {
            for (var i = 0; i < Alphabet.Length; ++i)
                Console.Write($"{Alphabet.Charset[i]}:{_frequencies[i]} ");
            Console.WriteLine();
        }

        public string Content()
        {
            return _text;
        }
        
        public double GetIC()
        {
            double ic = 0;
            foreach (int i in _frequencies)
                ic += i * (i - 1);
            return ic / (_length * (_length - 1));
        }

        public double[] GetMutualICList(Text altText)
        {
            var icList = new double[Alphabet.Length];

            for (var i = 0; i < Alphabet.Length; ++i)
            {
                double ic = 0;
                for (var j = 0; j < Alphabet.Length; ++j)
                    ic += _frequencies[j]
                          * altText._frequencies[(j - i + Alphabet.Length) % Alphabet.Length]
                          / (double) (_length * altText._length);
                icList[i] = ic;
            }

            return icList;
        }
        
        public Text[] GetSubstring(int strLength)
        {
            var texts = new List<char>[strLength];
            for (var i = 0; i < strLength; ++i)
            {
                texts[i] = new List<char>();
            }
            for (var i = 0; i < _length; ++i)
            {
                texts[i % strLength].Add(_text[i]);
            }

            var substring = new Text[strLength];
            for (var i = 0; i < strLength; ++i)
                substring[i] = new Text(new string(texts[i].ToArray()));

            return substring;
        }
        
        public void GetRepetitions()
        {
            var substrLen = 2;
            int count;
            do
            {
                string substring = _text.Substring(0, substrLen++);
                count = Regex.Matches(_text, substring).Count;
                Console.WriteLine($"{substring} appears {count} times in text.");
            } while (count > 2);
        }

        public int Kasiski(string pattern)
        {
            int length = pattern.Length;
            var positions = new List<int>();
            
            for (var i = 0; i < _length - length; ++i)
            {
                if (_text.Substring(i, length) == pattern)
                    positions.Add(i);
            }

            int gcd = positions[0];
            for (var i = 1; i < positions.Count; ++i)
                gcd = GCD(gcd, positions[i]);
            
            return gcd;
        }

        public string Decipher(string key)
        {
            key = key.ToUpper();
            var output = new StringBuilder();
            int keyLength = key.Length, keyIndex = 0;
            foreach (char c in _text)
            {
                if (keyIndex == keyLength) keyIndex = 0;
                output.Append((char) ('A' + (c - key[keyIndex++] + Alphabet.Length)
                    % Alphabet.Length));
            }
            return output.ToString();
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