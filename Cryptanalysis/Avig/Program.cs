using System;
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
                text = File.ReadAllText(filename);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return;
            }
            Console.WriteLine("Contents of {0} = {1}", filename, text);
        }
    }
}