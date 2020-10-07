namespace Analysis
{
    public struct Alphabet
    {
        public const int Length = 26;

        public static char[] Charset;
        public static void Initialise()
        {
            Charset = new char[Length];
            for (var i = 0; i < Length; ++i)
                Charset[i] = (char) ('A' + i);
        }
    }
}