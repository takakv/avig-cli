using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Analysis;
using static Analysis.List;

namespace Avig
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Possibility of adding support for multiple alphabets.
            Alphabet.Initialise();
            
            // Get file from the user with minimal validation.
            // This is a tool for busy students after all :-)
            string inFile;
            if (args.Length == 0)
            {
                Console.WriteLine("Enter a file name (with extension):");
                inFile = Console.ReadLine();
            }
            else
            {
                inFile = args[0];
            }

            string inPath = Directory.GetCurrentDirectory();
            inPath += RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\\" : "/";
            if (!File.Exists(inPath + inFile))
            {
                Console.WriteLine("No such file.");
                return;
            }
            Text ciphertext;
            try
            {
                ciphertext = new Text(File.ReadAllText(inPath + inFile));
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return;
            }
            Console.WriteLine("Contents of {0}:\n{1}", inFile, ciphertext.Content());

            Console.WriteLine();
            Console.Write("Frequencies: ");
            ciphertext.PrintFrequencies();

            // Index of coincidence of the ciphertext. The longer a Vigenere
            // ciphertext, the more the IC tends toward 4.
            Console.WriteLine();
            Console.WriteLine($"Index of Coincidence: {ciphertext.GetIC()}.");
            
            Console.WriteLine();
            Console.WriteLine("Kasiski analysis:");
            
            Console.WriteLine("String repetitions:");
            ciphertext.GetRepetitions();
            
            // The user has to make an educated guess about which substring
            // is the most probable to yield good results.
            Console.WriteLine();
            Console.WriteLine("Which substring would you like to use for testing?");
            string subStr = Console.ReadLine();

            while (subStr == null || !Regex.IsMatch(subStr, @"[a-zA-Z]$"))
            {
                if (subStr == "") return;
                Console.WriteLine("Please enter a valid substring, or press enter to quit.");
                subStr = Console.ReadLine();
            }
            
            // Use Kasiski analysis to determine the most probable key-length.
            int keyLength = ciphertext.Kasiski(subStr);
            Console.WriteLine($"The probable key-length is {keyLength}.");
            
            // If the Kasiski analysis got the correct key length, the IC
            // of each block of letter-position modulo key length letters
            // should be about ~0.065.
            Console.WriteLine();
            Console.WriteLine($"Examining substrings based on key-length of {keyLength}:");
            Text[] partials = ciphertext.GetSubstring(keyLength);
            for (var i = 0; i < partials.Length; ++i)
                Console.WriteLine($"IC{i + 1}: {partials[i].GetIC()}");

            // Compute the ICs of each block pair, to try and identify the relations
            // between keys.
            var indices = new List<double[]>();
            for (var i = 0; i < keyLength; ++i)
                for (int j = i + 1; j < keyLength; ++j)
                    indices.Add(partials[i].GetMutualICList(partials[j]));

            Console.WriteLine();
            Console.WriteLine("The highest indices of coincidence between substrings (truncated, not rounded) are:");
            double[] maximums = GetMaxOfEach(indices, out int[] indexes);

            // Only the ICs of relevance should be kept. If there are too many ICs
            // there is the possibility that the linear congruence system cannot
            // be solved. This is due to "noise" in the IC calculation.
            int validCount;
            string choice;
            var threshold = 0.061;
            do
            {
                ApplyThreshold(maximums, ref indexes, out validCount, threshold);
                foreach (double t in maximums)
                {
                    Console.ForegroundColor = t < threshold ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
                    Console.Write($"~{t:f3} ");
                }
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine($"Above threshold: {validCount}. The threshold applied was {threshold}. ");
                Console.Write("You may enter a custom threshold or press enter to continue: ");
                choice = Console.ReadLine();
                if (choice == "") continue;
                double.TryParse(choice, out threshold);
                Console.WriteLine();
            } while (choice != "");

            // Use an external tool or good-old pen and paper to solve the system
            // of linear congruences.
            Console.WriteLine();
            Console.WriteLine("Solve the following system:");
            IEnumerable<int[]> coefficients = GetLinearCoefficients(keyLength, validCount, indexes);
            foreach (int[] line in coefficients)
                Console.WriteLine($"z{line[0]} - z{line[1]} = {line[2]}\t(mod 26)");

            // The user only has to enter the "shift", without any variable names.
            // All the keys will be generated based on that info.
            // Again, minimal validation, trolls beware!
            Console.WriteLine();
            Console.WriteLine("Enter the results:");
            var keys = new Keys(keyLength);

            Console.WriteLine("All potential keys are:");
            keys.Print();

            // User must rely on instinct to find the right key.
            // Alternatively, if all keys are chosen, all plaintexts
            // will be generated.
            Console.WriteLine();
            Console.Write("Enter a deciphering key or press enter for testing all keys: ");
            string testKey = Console.ReadLine();
            string plaintext = ciphertext.Decipher(testKey, keys);
            Console.WriteLine();
            Console.WriteLine(plaintext);
            string outPath = inPath + "decrypted_" + inFile;
            try
            {
                File.WriteAllText(outPath, plaintext);
                Console.Write("Output text to ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(outPath);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                Console.WriteLine("Could not write output to file. Quitting...");
            }
        }
    }
}