using System;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(menuName= "ScriptableObjects/JsonTypeConverter")]
public class JsonTypeConverter : ScriptableObject
{

    [Serializable]
    public class JsonType
    {
        public JsonInput input;
        public JsonOutput output;
        [Serializable]
        public class JsonInput
        {
            public string entry;
        }
        [Serializable]
        public class JsonOutput
        {
            public ItemType type;
            public ColorType color;
        }
    }
    public JsonType[] jsonTypes;

    public JsonType.JsonOutput GetOutput(string str)
    {
        var jsonType = jsonTypes.First(entry => entry.input.entry == str);
        return jsonType.output;
    }
}
