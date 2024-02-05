using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelReader
{
    private JsonTypeConverter typeConverter;
    public LevelReader()
    {
        typeConverter = Resources.Load<JsonTypeConverter>("JsonTypeConverter");
    }
    public LevelData LoadLevel(string levelName)
    {
        TextAsset levelFile = Resources.Load<TextAsset>($"Levels/{levelName}");
        if (levelFile != null)
        {
            return JsonUtility.FromJson<LevelData>(levelFile.text);
        }
        Debug.LogError($"Level file not found: {levelName}");
        return null;
    }


    public JsonTypeConverter.JsonType.JsonOutput GetTypes(string str)
    {
        return typeConverter.GetOutput(str);
    }
}
