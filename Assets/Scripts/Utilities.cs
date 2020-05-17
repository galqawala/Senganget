using System.Collections.Generic;
using UnityEngine;

static class Utilities
{
    public static void GetAllChildren(Transform parent, ref List <Transform> transforms)
    {
        // https://forum.unity.com/threads/loop-through-all-children.53473/
        foreach (Transform t in parent) {
            transforms.Add(t);
            GetAllChildren(t, ref transforms);
        }
    }

    public static string FirstLetterToUpper(string str)
    {
        // https://stackoverflow.com/questions/4135317/make-first-letter-of-a-string-upper-case-with-maximum-performance
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
}