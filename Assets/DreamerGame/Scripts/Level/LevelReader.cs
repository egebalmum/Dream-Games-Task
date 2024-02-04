using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelReader
{
    public static LevelData LoadLevel(string levelName)
    {
        TextAsset levelFile = Resources.Load<TextAsset>($"Levels/{levelName}");
        if (levelFile != null)
        {
            return JsonUtility.FromJson<LevelData>(levelFile.text);
        }
        Debug.LogError($"Level file not found: {levelName}");
        return null;
    }
}
