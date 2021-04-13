using System;
using System.Collections.Generic;

namespace SCHelper.Services
{
    public interface IMathService
    {
        IEnumerable<T[]> GetAllCombinations<T>(T[] data, int count);

        IEnumerable<T[]> GetAllPermutations<T>(T[] data);

        IEnumerable<T[]> BuildOrderedClusters<T>(T[] data, Func<T, T, CompareResult> compare);
    }

    public enum CompareResult
    {
        GreaterOrEqual,
        Less,
        NotComparable,
    }
}
