using System;
using UnityEngine;
[CreateAssetMenu(menuName= "ScriptableObjects/GoalSettings")]
public class GoalSettings : ScriptableObject
{
    [Serializable]
    public class GoalEntity
    {
        public ItemType type;
        public ColorType color;
    }

    public GoalEntity[] goalEntities;
}
