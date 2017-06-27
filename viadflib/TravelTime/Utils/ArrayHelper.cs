using System;

namespace viadflib.TravelTime.Utils
{
    public static class ArrayHelper
    {
        /// <summary>
        /// Finds minimal index with value greater or equal in an ascending ordered array
        /// </summary>
        public static int? FindMinIndexWithValueGreaterOrEqual<T>(T[] values, Func<T, int> valueSelector, int minValue)
        {
            return FindMinIndexWithValueGreaterOrEqual(values, valueSelector, minValue, 0, values.Length - 1);
        }

        private static int? FindMinIndexWithValueGreaterOrEqual<T>(T[] values, Func<T, int> valueSelector, int minValue, int minIndex, int maxIndex)
        {
            if (minValue > valueSelector(values[maxIndex]))
                return null;

            if (minValue < valueSelector(values[minIndex]))
                return minIndex;

            if (minIndex == maxIndex)
            {
                return maxIndex;
            }

            int indexBetween = minIndex + ((maxIndex - minIndex) / 2);

            if (valueSelector(values[indexBetween]) < minValue)
            {
                return FindMinIndexWithValueGreaterOrEqual(values, valueSelector, minValue, indexBetween + 1, maxIndex);
            }
            else
            {
                return FindMinIndexWithValueGreaterOrEqual(values, valueSelector, minValue, minIndex, indexBetween);
            }
        }
    }
}
