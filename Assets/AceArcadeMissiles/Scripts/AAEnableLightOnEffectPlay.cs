using UnityEngine;

public enum AAEffectType
{
    Trail,
    Particle
}

/// <summary>
/// When attached to a light, this will cause the light to shine only when the effect is playing.
/// To work correctly, this script requires the parent of this light to be a particle system.
/// </summary>
public class AAEnableLightOnEffectPlay : MonoBehaviour
{
    public AAEffectType effectTypeToReference = AAEffectType.Particle;

    TrailRenderer trail;
    ParticleSystem ps_Trail;

    new Light light;

    void Awake()
    {
        if (effectTypeToReference == AAEffectType.Trail)
            trail = GetComponentInParent<TrailRenderer>();        
        
        else if (effectTypeToReference == AAEffectType.Particle)
            ps_Trail = GetComponentInParent<ParticleSystem>();
        
        light = GetComponent<Light>();
    }

    void Update()
    {
        if (effectTypeToReference == AAEffectType.Trail)
        {
            if (trail != null && trail.enabled)
                light.enabled = true;
            else
                light.enabled = false;
        }

        else if (effectTypeToReference == AAEffectType.Particle)
        {
            if (ps_Trail != null && ps_Trail.isPlaying)
                light.enabled = true;
            else
                light.enabled = false;
        }
    }
}