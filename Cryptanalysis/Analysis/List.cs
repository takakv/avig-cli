using System.Collections.Generic;

namespace Analysis
{
    public static class List
    {
        private static double GetMax(IReadOnlyList<double> list, out int index)
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
        
        public static double[] GetMaxOfEach(List<double[]> list, out int[] positions)
        {
            var maximums = new double[list.Count];
            positions = new int[list.Count];
            for (var i = 0; i < list.Count; ++i)
                maximums[i] = GetMax(list[i], out positions[i]);
            return maximums;
        }
        
        public static void ApplyThreshold(IReadOnlyList<double> maximums, ref int[] indexes,
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
        
        public static IEnumerable<int[]> GetLinearCoefficients(int iterations,
            int coefficientCount, IReadOnlyList<int> indexes)
        {
            var placeholders = new List<int[]>();

            for (var i = 0; i < iterations; ++i)
                for (int j = i + 1; j < iterations; ++j)
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