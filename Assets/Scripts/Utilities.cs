using System;
using UnityEngine;

public static class Utilities
{
    private const string Website = "https://models.readyplayer.me/";
    private const string Extension = ".glb";

    public static string CompleteUrl(string inputString)
    {
        if (!inputString.StartsWith(Website)) inputString = Website + inputString;
        if (!inputString.EndsWith(Extension)) inputString += Extension;
        return inputString;
    }

    public static string StripUrl(string inputString)
    {
        int startIndex = inputString.LastIndexOf('/') + 1;
        int endIndex = inputString.LastIndexOf('.');
        if (endIndex == -1) endIndex = inputString.Length;
        return inputString.Substring(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// Remaps the source bones to the destination bones, in order to build the bones array that the MeshFilter needs.
    /// </summary>
    public static Transform[] MapBones(Transform destinationRoot, Transform[] sourceBones)
    {
        Transform[] mappedBones = new Transform[sourceBones.Length];
        for (int i = 0; i < sourceBones.Length; i++)
        {
            mappedBones[i] = FindDeepChild(destinationRoot, sourceBones[i].name);
            
            if (mappedBones[i] == null) Debug.LogError("Bone mapping failed for: " + sourceBones[i].name);
        }
        return mappedBones;
    }

    /// <summary>
    /// Utility function to find a deeply-nested child, starting from a parent.
    /// </summary>
    private static Transform FindDeepChild(Transform parent, string boneName)
    {
        if (parent.name == boneName) return parent;

        foreach (Transform child in parent)
        {
            if (child.name == boneName)
                return child;

            Transform result = FindDeepChild(child, boneName);
            if (result != null)
                return result;
        }

        return null;
    }
}