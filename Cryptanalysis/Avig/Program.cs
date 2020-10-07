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
            
            int keyLength = Kasiski(text, subStr);
            Console.WriteLine($"The probable key-length is {keyLength}.");
            
            Console.WriteLine();
            Console.WriteLine($"Examining substrings based on key-length of {keyLength}:");
            string[] partials = PartialText(text, keyLength);
            for (var i = 0; i < partials.Length; ++i)
            {
                Console.WriteLine($"IC{i + 1}: {GetIC(partials[i])}");
            }

            var indices = new List<double[]>();
            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    indices.Add(GetMutualICList(partials[i], partials[j]));

            Console.WriteLine();
            Console.WriteLine("The highest indexes of coincidence are:");
            double[] maximums = GetListOfMax(indices, out int[] indexes);
            ApplyICThreshold(maximums, ref indexes, out int validCount);
            foreach (double t in maximums)
                Console.Write($"~{t:f3} ");

            Console.WriteLine();
            Console.WriteLine("Solve the following system:");
            int[][] coefficients = GetLinearCoefficients(keyLength, validCount, indexes);
            foreach (int[] line in coefficients)
            {
                Console.WriteLine($"z{line[0]} - z{line[1]} = {line[2]}\t(mod 26)");
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

        private static double[] GetMutualICList(string text1, string text2)
        {
            const int alphabetLength = 26;
            var characters = new char[alphabetLength];
            for (var i = 0; i < alphabetLength; ++i)
                characters[i] = (char) ('A' + i);

            var frequencies1 = new int[alphabetLength];
            var frequencies2 = new int[alphabetLength];

            foreach (char c in text1)
                ++frequencies1[Array.IndexOf(characters, c)];

            foreach (char c in text2)
                ++frequencies2[Array.IndexOf(characters, c)];

            var icList = new double[alphabetLength];

            for (var i = 0; i < alphabetLength; ++i)
            {
                double ic = 0;
                for (var j = 0; j < alphabetLength; ++j)
                    ic += frequencies1[j] * frequencies2[(j - i + 26) % 26]
                          / (double) (text1.Length * text2.Length);
                icList[i] = ic;
            }

            return icList;
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

        private static double GetMaxFromList(IReadOnlyList<double> list, out int index)
        {
            double max = list[0];
            index = 0;
            for (var i = 1; i < list.Count; ++i)
            {
                if (list[i] < max) continue;
                max = list[i];
                index = i;
            }

            return max;
        }

        private static double[] GetListOfMax(IReadOnlyList<double[]> list, out int[] positions)
        {
            var maximums = new double[list.Count];
            positions = new int[list.Count];
            for (var i = 0; i < list.Count; ++i)
                maximums[i] = GetMaxFromList(list[i], out positions[i]);
            return maximums;
        }

        private static void
            ApplyICThreshold(IReadOnlyList<double> maximums, ref int[] indexes, out int count)
        {
            count = 0;
            for (var i = 0; i < maximums.Count; ++i)
            {
                if (maximums[i] < 0.06)
                    indexes[i] = -1;
                else
                    ++count;
            }
        }

        private static int[][]
            GetLinearCoefficients(int keyLength, int coefficientCount, int[] indexes)
        {
            var placeholders = new List<int[]>();

            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    placeholders.Add(new[] {i + 1, j + 1, 0});

            var coefficients = new int[coefficientCount][];
            var coefficientIndex = 0;
            for (var i = 0; i < indexes.Length; ++i)
                if (indexes[i] != -1)
                    coefficients[coefficientIndex++] =
                        new[] {placeholders[i][0], placeholders[i][1], indexes[i]};
            
            return coefficients;
        }
    }
}