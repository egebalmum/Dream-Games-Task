using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSprite
{
    [Serializable]
    public class SpecialSpriteContainer
    {
        public string name;
        public Sprite sprite;
    }

    [Serializable]
    public class SpriteContainer
    {
        public Sprite sprite;
    }
}
