using System;
using System.Text;

namespace Analysis
{
    public static class Keygen
    {
        public static int[] GetLetterDifferences(int keyLength)
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
        
        public static void PrintKeys(int[] differences)
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