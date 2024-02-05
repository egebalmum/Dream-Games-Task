using System;
using System.Collections;
using System.Collections.Generic;
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
