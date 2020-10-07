using System;
using System.Collections.Generic;
using System.IO;
using Analysis;
using static Analysis.Text;
using static Analysis.List;

namespace Avig
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Alphabet.Initialise();
            
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

            var frequencies = new int[Alphabet.Length];
            foreach (char c in text)
                ++frequencies[Array.IndexOf(Alphabet.Charset, c)];

            Console.WriteLine();
            Console.Write("Frequencies: ");
            for (var i = 0; i < Alphabet.Length; ++i)
                Console.Write($"{Alphabet.Charset[i]}:{frequencies[i]} ");

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
            string[] partials = GetSubstring(text, keyLength);
            for (var i = 0; i < partials.Length; ++i)
            {
                Console.WriteLine($"IC{i + 1}: {GetIC(partials[i])}");
            }

            var indices = new List<double[]>();
            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    indices.Add(GetMutualICList(partials[i], partials[j]));

            Console.WriteLine();
            Console.WriteLine("The highest indices of coincidence (truncated, not rounded) are:");
            double[] maximums = GetMaxOfEach(indices, out int[] indexes);

            int validCount;
            string choice;
            var threshold = 0.061;
            do
            {
                ApplyICThreshold(maximums, ref indexes, out validCount, threshold);
                foreach (double t in maximums)
                    Console.Write($"~{t:f3} ");
                Console.WriteLine();
                Console.WriteLine($"The threshold applied was {threshold}. ");
                Console.WriteLine("Would you like to increase the threshold?");
                choice = Console.ReadLine();
                if (choice == "y")
                {
                    Console.WriteLine("What would you like to set as the threshold?");
                    double.TryParse(Console.ReadLine(), out threshold);
                }

                threshold += 0.001;
            } while (choice == "y");

            Console.WriteLine("\n");
            Console.WriteLine("Solve the following system:");
            IEnumerable<int[]> coefficients = GetLinearCoefficients(keyLength, validCount, indexes);
            foreach (int[] line in coefficients)
            {
                Console.WriteLine($"z{line[0]} - z{line[1]} = {line[2]}\t(mod 26)");
            }
        }

        private static double[] GetMutualICList(string text1, string text2)
        {
            var characters = new char[Alphabet.Length];
            for (var i = 0; i < Alphabet.Length; ++i)
                characters[i] = (char) ('A' + i);

            var frequencies1 = new int[Alphabet.Length];
            var frequencies2 = new int[Alphabet.Length];

            foreach (char c in text1)
                ++frequencies1[Array.IndexOf(characters, c)];

            foreach (char c in text2)
                ++frequencies2[Array.IndexOf(characters, c)];

            var icList = new double[Alphabet.Length];

            for (var i = 0; i < Alphabet.Length; ++i)
            {
                double ic = 0;
                for (var j = 0; j < Alphabet.Length; ++j)
                    ic += frequencies1[j]
                          * frequencies2[(j - i + Alphabet.Length) % Alphabet.Length]
                          / (double) (text1.Length * text2.Length);
                icList[i] = ic;
            }

            return icList;
        }

        private static void ApplyICThreshold(IReadOnlyList<double> maximums, ref int[] indexes,
            out int count, double threshold)
        {
            count = 0;
            for (var i = 0; i < maximums.Count; ++i)
            {
                if (maximums[i] < threshold)
                    indexes[i] = -1;
                else
                    ++count;
            }
        }

        private static IEnumerable<int[]> GetLinearCoefficients(int keyLength,
            int coefficientCount, IReadOnlyList<int> indexes)
        {
            var placeholders = new List<int[]>();

            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    placeholders.Add(new[] {i + 1, j + 1, 0});

            var coefficients = new int[coefficientCount][];
            var coefficientIndex = 0;
            for (var i = 0; i < indexes.Count; ++i)
                if (indexes[i] != -1)
                    coefficients[coefficientIndex++] =
                        new[] {placeholders[i][0], placeholders[i][1], indexes[i]};
            
            return coefficients;
        }
    }
}