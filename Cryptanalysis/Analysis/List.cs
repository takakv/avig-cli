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
    }
}