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
}
