using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Avig
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter a filename with extension:");
            string filename = Console.ReadLine();
            string filepath = Directory.GetCurrentDirectory() + "\\" + filename;
            if (!File.Exists(filepath))
            {
                Console.WriteLine("No such file.");
                return;
            }
            string text;
            try
            {
                text = File.ReadAllText(filename).ToUpper();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return;
            }
            Console.WriteLine("Contents of {0}:\n{1}", filename, text);

            const int alphabetLength = 26;
            var characters = new char[alphabetLength];
            for (var i = 0; i < alphabetLength; ++i)
                characters[i] = (char) ('A' + i);

            var frequencies = new int[alphabetLength];
            foreach (char c in text)
                ++frequencies[Array.IndexOf(characters, c)];

            Console.WriteLine();
            Console.Write("Frequencies: ");
            for (var i = 0; i < alphabetLength; ++i)
                Console.Write($"{characters[i]}:{frequencies[i]} ");

            Console.WriteLine();
            Console.WriteLine($"Index of Coincidence: {GetIC(frequencies, text.Length)}.");
            
            Console.WriteLine();
            Console.WriteLine("Kasiski analysis:");
            
            Console.WriteLine("String repetitions:");
            GetRepetitions(text);
            
            Console.WriteLine();
            Console.WriteLine("Which substring would you like to use for testing?");
            string subStr = Console.ReadLine()?.ToUpper();
            
            int length = Kasiski(text, subStr);
            Console.WriteLine($"The probable key-length is {length}.");
            
            Console.WriteLine();
            Console.WriteLine($"Examining substrings based on key-length of {length}:");
            string[] partials = PartialText(text, length);
            for (var i = 0; i < partials.Length; ++i)
            {
                Console.WriteLine($"IC{i + 1}: {GetIC(partials[i])}");
            }
        }

        private static double GetIC(IEnumerable<int> freq, int len)
        {
            double ic = 0;
            foreach (int i in freq)
                ic += i * (i - 1);
            return ic / (len * (len - 1));
        }

        private static double GetIC(string text)
        {
            const int alphabetLength = 26;
            var characters= new char[alphabetLength];
            for (var i = 0; i < alphabetLength; ++i)
                characters[i] = (char) ('A' + i);

            var frequencies = new int[alphabetLength];
            foreach (char c in text)
                ++frequencies[Array.IndexOf(characters, c)];

            return GetIC(frequencies, text.Length);
        }

        private static void GetRepetitions(string text)
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

        private static int Kasiski(string text, string pattern)
        {
            int length = pattern.Length;
            var positions = new List<int>();
            
            for (var i = 0; i < text.Length - length; i += length)
            {
                if (text.Substring(i, length) == pattern)
                    positions.Add(i);
            }

            int gcd = positions[0];
            for (var i = 1; i < positions.Count; ++i)
                gcd = GCD(gcd, positions[i]);
            
            return gcd;
        }

        private static string[] PartialText(string text, int shift)
        {
            var texts = new List<char>[shift];
            for (var i = 0; i < shift; ++i)
            {
                texts[i] = new List<char>();
            }
            for (var i = 0; i < text.Length; ++i)
            {
                texts[i % shift].Add(text[i]);
            }

            var result = new string[shift];
            for (var i = 0; i < shift; ++i)
            {
                result[i] = new string(texts[i].ToArray());
            }

            return result;
        }
    }
}