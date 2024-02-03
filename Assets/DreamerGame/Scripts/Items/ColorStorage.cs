using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class ColorStorage : MonoBehaviour
{
    [SerializeField] public ColorType color;
    [SerializeField] public ItemSprite.SpecialSpriteContainer[] specialStates;
    [SerializeField] public ItemSprite.SpriteContainer[] states;
}
