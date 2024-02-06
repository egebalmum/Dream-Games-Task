using System;
using UnityEngine;

[Serializable]
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

    public SpecialSpriteContainer[] specialSprites;
    public SpriteContainer[] sprites;
}
