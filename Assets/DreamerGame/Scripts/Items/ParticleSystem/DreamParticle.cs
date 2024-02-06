using System;
using System.Linq;
using UnityEngine;

public class DreamParticle : MonoBehaviour
{
    [Serializable]
    public class ParticleVariance
    {
        public ColorType color;
        public Sprite[] sprites;
    }

    [HideInInspector] public ItemType type;
    public ParticleVariance[] particleVariances;
    public ParticleSystem particle;
    
    public void SetParticleColor(ColorType newColor)
    {
        var variance = particleVariances.FirstOrDefault(variance => variance.color == newColor);
        if (variance == null)
        {
            Debug.LogError("No particleVariance found.");
            return;
        }
        
        ParticleSystem.TextureSheetAnimationModule tsam = particle.textureSheetAnimation;
        tsam.mode = ParticleSystemAnimationMode.Sprites;

        for (int i = 0; i < variance.sprites.Length; i++)
        {
            tsam.SetSprite(i, variance.sprites[i]);
        }
        
    }
}
