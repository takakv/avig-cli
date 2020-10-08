using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Analysis;
using static Analysis.List;

namespace Avig
{
    internal static class Program
    {
        private static void Main(string[] args)
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
            Text ciphertext;
            try
            {
                ciphertext = new Text(File.ReadAllText(filename));
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return;
            }
            Console.WriteLine("Contents of {0}:\n{1}", filename, ciphertext.Content());

            Console.WriteLine();
            Console.Write("Frequencies: ");
            ciphertext.PrintFrequencies();

            Console.WriteLine();
            Console.WriteLine($"Index of Coincidence: {ciphertext.GetIC()}.");
            
            Console.WriteLine();
            Console.WriteLine("Kasiski analysis:");
            
            Console.WriteLine("String repetitions:");
            ciphertext.GetRepetitions();
            
            Console.WriteLine();
            Console.WriteLine("Which substring would you like to use for testing?");
            string subStr = Console.ReadLine()?.ToUpper();
            
            int keyLength = ciphertext.Kasiski(subStr);
            Console.WriteLine($"The probable key-length is {keyLength}.");
            
            Console.WriteLine();
            Console.WriteLine($"Examining substrings based on key-length of {keyLength}:");
            Text[] partials = ciphertext.GetSubstring(keyLength);
            for (var i = 0; i < partials.Length; ++i)
                Console.WriteLine($"IC{i + 1}: {partials[i].GetIC()}");

            var indices = new List<double[]>();
            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    indices.Add(partials[i].GetMutualICList(partials[j]));

            Console.WriteLine();
            Console.WriteLine("The highest indices of coincidence between substrings (truncated, not rounded) are:");
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
                Console.WriteLine($"Below threshold: {validCount}. The threshold applied was {threshold}. ");
                Console.Write("You may enter a custom threshold or press enter to continue: ");
                choice = Console.ReadLine();
                if (choice == "") continue;
                double.TryParse(choice, out threshold);
                Console.WriteLine();
            } while (choice != "");

            Console.WriteLine("\n");
            Console.WriteLine("Solve the following system:");
            IEnumerable<int[]> coefficients = GetLinearCoefficients(keyLength, validCount, indexes);
            foreach (int[] line in coefficients)
                Console.WriteLine($"z{line[0]} - z{line[1]} = {line[2]}\t(mod 26)");

            Console.WriteLine();
            Console.WriteLine("Enter the results:");
            int[] differences = GetLetterDifferences(keyLength);

            Console.WriteLine("All potential keys are:");
            PrintKeys(differences);

            Console.WriteLine();
            Console.Write("Enter a deciphering key or press enter for testing all keys: ");
            string testKey = Console.ReadLine();
            Console.WriteLine();
            Console.WriteLine(ciphertext.Decipher(testKey));
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

        private static int[] GetLetterDifferences(int keyLength)
        {
            var differences = new int[keyLength - 1];
            for (var i = 0; i < keyLength - 1; ++i)
            {
                Console.Write($"z{i+2} = z1 + ");
                int.TryParse(Console.ReadLine(), out differences[i]);
            }
            Console.WriteLine();
            return differences;
        }

        private static void PrintKeys(int[] differences)
        {
            for (var i = 'A'; i <= 'Z'; ++i)
            {
                var key = new StringBuilder();
                key.Append(i);
                foreach (int t in differences)
                    key.Append((char) ((i - 'A' + t) % Alphabet.Length + 'A'));
                Console.Write($"{key}\t");
                if (i == 'A' + Alphabet.Length / 2 - 1)
                    Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}