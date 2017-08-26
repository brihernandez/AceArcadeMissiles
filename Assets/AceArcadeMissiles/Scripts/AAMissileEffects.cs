using UnityEngine;

[RequireComponent(typeof(AAMissile))]
public class AAMissileEffects : MonoBehaviour
{
    new Transform transform;

    [Tooltip("Trail remains on whether or not the motor is running. For most games using this kind of arcade missile, this looks/works best.")]
    public bool trailAlwaysOn = true;

    [Tooltip("Position to attach either a particle system or trailrenderer prefab. If not assigned, this will automatically search for a GameObject named \"TrailFx\".")]
    public Transform trailFxPoint;
    
    [Tooltip("TrailRenderer played at missile's attach point.")]
    public TrailRenderer trailPrefab;

    [Tooltip("ParticleSystem played at missile's attach point.")]
    public ParticleSystem particleTrailPrefab;

    [Tooltip("Effect spawened when missile explodes.")]
    public ParticleSystem explosionFXPrefab;
    public bool playExplosionOnSelfDestruct = false;

    [Header("Audio")]
    [Tooltip("Associates the generated audio sources with the specified mixer group.")]
    public UnityEngine.Audio.AudioMixerGroup mixerGroup;

    [Tooltip("Sound played when motor ignites. Will automatically create an AudioSource with these specs.")]
    public AudioClip fireClip;
    public float fireVolume = 1.0f;
    public float fireMinDistance = 25.0f;
    public float fireMaxDistance = 500.0f;
    protected AudioSource fireSource;

    [Tooltip("Loop sound of the missile motor. Will automatically create an AudioSource with these specs.")]
    public AudioClip loopClip;
    public float loopVolume = 1.0f;
    public float loopMinDistance = 30.0f;
    public float loopMaxDistance = 500.0f;
    protected AudioSource loopSource;

    TrailRenderer trail;
    ParticleSystem particleTrail;

    AAMissile missile;
    AARemoveEffect effectRemover;

    bool motorHasActivated = false;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        missile = GetComponent<AAMissile>();

        if (fireClip != null)
        {
            fireSource = gameObject.AddComponent<AudioSource>();
            fireSource.clip = fireClip;
            fireSource.minDistance = fireMinDistance;
            fireSource.maxDistance = fireMaxDistance;
            fireSource.loop = false;
            fireSource.dopplerLevel = 0.0f;
            fireSource.spatialBlend = 1.0f;
            fireSource.volume = fireVolume;
            fireSource.pitch = Random.Range(0.9f, 1.3f);
            fireSource.outputAudioMixerGroup = mixerGroup ?? null;
            fireSource.Stop();
        }

        if (loopClip != null)
        {
            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.clip = loopClip;
            loopSource.minDistance = loopMinDistance;
            loopSource.maxDistance = loopMaxDistance;
            loopSource.loop = true;
            loopSource.dopplerLevel = 1.0f;
            loopSource.volume = loopVolume;
            loopSource.spatialBlend = 1.0f;
            loopSource.outputAudioMixerGroup = mixerGroup ?? null;
            loopSource.Stop();
        }
    }

    private void Start()
    {
        // First make sure that an effect was assigned at all.
        if (trailPrefab != null || particleTrailPrefab != null)
        {
            // Make sure there is a reference point for where to spawn the trail.
            if (trailFxPoint == null)
            {
                // There isn't an already assigned one. Check to see if one already exists.
                trailFxPoint = transform.Find("TrailFx");

                // Still haven't found one, just make a new one.
                if (trailFxPoint == null)
                {
                    trailFxPoint = transform;
                    Debug.Log("No trail effect point given, or child object named \"TrailFx\" to place the missile trail effect on " + name + ". Using object origin instead.");
                }
            }

            // Instantiate the TrailRenderer trail.
            if (trailPrefab != null)
            {
                trail = Instantiate(trailPrefab, trailFxPoint);
                trail.transform.localPosition = Vector3.zero;
                trail.transform.localEulerAngles = Vector3.zero;

                trail.enabled = false;
            }

            // Instantiate the ParticleSystem trail.
            if (particleTrailPrefab != null)
            {
                particleTrail = Instantiate(particleTrailPrefab, trailFxPoint);
                particleTrail.transform.localPosition = Vector3.zero;
                particleTrail.transform.localEulerAngles = Vector3.zero;

                particleTrail.Stop();

                // Ensure that the effect remover is on the gameobject to prevent undeleted particle effects.
                if (particleTrail.GetComponent<AARemoveEffect>() == null)
                    effectRemover = particleTrail.gameObject.AddComponent<AARemoveEffect>();
            }
        }
    }

    private void Update()
    {
        // When motor gets activated, start playing effects.
        if (!motorHasActivated && missile.MotorActive)
        {
            motorHasActivated = true;

            if (fireSource != null)
                fireSource.Play();
            if (loopSource != null)
                loopSource.Play();

            if (trail != null)
                trail.enabled = true;
            else if (particleTrail != null)
                particleTrail.Play();            
            else
                Debug.LogWarning("No TrailRenderer or ParticleSystem prefabs assigned for missile trail FX on " + transform.name + ".");
        }

        // Detach the trail when motor shuts off. (If applicable.)
        if (!trailAlwaysOn && motorHasActivated && !missile.MotorActive)
        {
            DetachTrail();
        }
    }

    /// <summary>
    /// Trigger the explosion effect and automatically detach any spawned trail effects.
    /// </summary>
    public void Explode()
    {
        // If particle systems or trails are destroyed, their trail/particles do not persist.
        // Unparenting them before destruction prevents this. The missile calls this function
        // when it gets destroyed.
        DetachTrail();

        if (explosionFXPrefab != null)
        {
            ParticleSystem explode = GameObject.Instantiate(explosionFXPrefab);
            explode.transform.position = transform.position;
            explode.transform.rotation = transform.rotation;

            // Give the explosion particle system the component to destroy itself after it finished playing.
            AARemoveEffect remove = explode.GetComponent<AARemoveEffect>();
            if (remove == null)    
                remove = explode.gameObject.AddComponent<AARemoveEffect>();

            remove.readyToDestroy = true;
        }
        else
            Debug.LogWarning("No ParticleSystem prefab assigned for explosions on missile " + transform.name + ".");
    }

    /// <summary>
    /// Detaches all trail effects.
    /// </summary>
    private void DetachTrail()
    {
        if (fireSource != null)
            fireSource.Stop();
        if (loopSource != null)
            loopSource.Stop();

        if (trail != null)
        {
            // If the trail was disabled, just delete it straight up.
            if (trail.gameObject.activeSelf)
            {
                trail.transform.parent = null;
                trail.autodestruct = true;
            }
            else
                GameObject.Destroy(trail);
        }

        if (particleTrail != null)
        {
            // If the trail was disabled, just delete it straight up.
            if (particleTrail.gameObject.activeSelf)
            {
                particleTrail.transform.parent = null;
                particleTrail.Stop();
                effectRemover.readyToDestroy = true;
            }
            else
                GameObject.Destroy(particleTrail);
        }
    }
}