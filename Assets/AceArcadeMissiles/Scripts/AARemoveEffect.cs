using UnityEngine;

[DisallowMultipleComponent]
public class AARemoveEffect : MonoBehaviour
{
    ParticleSystem[] particles;
    public bool readyToDestroy = false;

    float effectStartTime = 0.0f;

    void OnEnable()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
        effectStartTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        bool allParticlesCountZero = true;
        foreach (ParticleSystem ps in particles)
        {
            if (ps.particleCount > 0)
            {
                allParticlesCountZero = false;
                break;
            }
        }

        // Only work this if the effect has been alive for longer than a second. Prevents effects from
        // destroying themselves before they can even start.
        if (readyToDestroy && allParticlesCountZero && Time.time - effectStartTime > 1.0f)
            Destroy(gameObject);
    }
}
