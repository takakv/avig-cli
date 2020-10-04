using System;
using System.Collections.Generic;
using System.IO;

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
        }

        private static double GetIC(IEnumerable<int> freq, int len)
        {
            double ic = 0;
            foreach (int i in freq)
                ic += i * (i - 1);
            return ic / (len * (len - 1));
        }
    }
}