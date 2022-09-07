using System;
using System.Collections.Generic;
using System.IO;

namespace lib;

public static class ScreenRepo
{
    public static IEnumerable<int> GetProblemIds()
    {
        for (var i = 1; DoesProblemExist(i); ++i)
        {
            yield return i;
        }
    }

    private static bool DoesProblemExist(int index) =>
        File.Exists(GetProblemFileName(index));

    private static string GetProblemFileName(int index) =>
        FileHelper.FindFilenameUpwards($"problems/problem{index}.png");

    public static Screen GetProblem(int index)
    {
        if (!DoesProblemExist(index))
            throw new InvalidOperationException($"invalid problem {index}");
        return Screen.LoadProblem(index);
    }

    public static void SaveProblem(int index, byte[] problem)
    {
        var dir = FileHelper.FindDirectoryUpwards("problems");
        var filename = Path.Combine(dir, $"problem{index}.png");
        File.WriteAllBytes(filename, problem);
    }
}
