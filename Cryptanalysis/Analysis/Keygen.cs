using System;
using System.Text;

namespace Analysis
{
    public readonly struct Keys
    {
        private readonly int[] _relations;
        private readonly string[] _keys;

        public Keys(int keyLength)
        {
            _relations = new int[keyLength - 1];
            for (var i = 0; i < keyLength - 1; ++i)
            {
                Console.Write($"z{i+2} = z1 + ");
                int.TryParse(Console.ReadLine(), out _relations[i]);
            }
            Console.WriteLine();
            _keys = new string[Alphabet.Length];
            GenerateKeys();
        }

        private void GenerateKeys()
        {
            for (var i = 'A'; i <= 'Z'; ++i)
            {
                var key = new StringBuilder();
                key.Append(i);
                foreach (int rel in _relations)
                    key.Append((char) ((i - 'A' + rel) % Alphabet.Length + 'A'));
                _keys[i - 'A'] = key.ToString();
            }
            
        }
        
        public void Print()
        {
            for (var i = 0; i < _keys.Length; ++i)
            {
                Console.Write($"{_keys[i]}\t");
                // Newline on half
                if (i == Alphabet.Length / 2 - 1)
                    Console.WriteLine();
            }
            Console.WriteLine();
        }

        public string[] Get()
        {
            return _keys;
        }
    }
}