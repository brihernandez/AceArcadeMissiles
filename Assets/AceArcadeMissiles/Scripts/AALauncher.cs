using UnityEngine;
using System.Collections.Generic;

public abstract class AALauncher : MonoBehaviour
{
    new Transform transform;

    [Header("General launcher options:")]

    [Tooltip("Assign this to prevent the launcher and missile from colliding with whatever launched/owns it. Especially important for missile pod launchers.")]
    public Transform ownShip;
    
    [Tooltip("Missile prefab that gets shot by the launcher.")]
    public AAMissile missilePrefabToLaunch;

    [Tooltip("How many missiles this launcher carries.")]
    public int missileCount = 1;
    
    [Tooltip("Time between successive shots.")]
    public float fireDelay = 6.0f;

    [Tooltip("Array of Transforms used as reference positions for the missile to launch.\n\nIf unassigned, list will be autopopulated with child Game Objects that follow the naming scheme \"Hp1\", \"Hp2\", \"Hp3\", etc. If none are found, a single launch point will automatically be created at the launcher's center.\n\nOn a hardpoint style launcher, only the first launchpoint is used.\n\nOn a pod style launcher, all launch points are used.")]
    public List<Transform> launchPoints;

    protected float reloadCooldown = 0.0f;
    protected float magazineReloadCooldown = 0.0f;

    [Header("Audio")]
    [Tooltip("Associates the generated audio sources with the specified mixer group.")]
    public UnityEngine.Audio.AudioMixerGroup mixerGroup;

    [Tooltip("Fire sound for this launcher. Will automatically create an AudioSource if not null.")]
    public AudioClip fireClip;
    public float fireVolume = 1.0f;
    public float minDistance = 10.0f;
    public float maxDistance = 200.0f;
    protected AudioSource fireSource;

    abstract public int MagazineCount { get; }

    protected virtual void Awake()
    {
        transform = GetComponent<Transform>();

        // If the list wasn't assigned, manually populate it.
        if (launchPoints.Count == 0)
        {
            Transform[] potentialHPs = GetComponentsInChildren<Transform>();
            for (int i = 0; i < potentialHPs.Length; i++)
            {
                if (potentialHPs[i].name.StartsWith("Hp"))
                    launchPoints.Add(potentialHPs[i]);
            }

            // If nothing was found, let the user know that it's going to just spawn them
            // directly on the launcher.
            if (launchPoints.Count == 0)
            {
                Debug.Log("No missile hardpoints found on " + transform.name + " , using launcher position instead.");
                launchPoints.Add(transform);
            }
        }

        if (fireClip != null)
        {
            fireSource = gameObject.AddComponent<AudioSource>();
            fireSource.clip = fireClip;
            fireSource.minDistance = minDistance;
            fireSource.maxDistance = maxDistance;
            fireSource.loop = false;
            fireSource.dopplerLevel = 0.0f;
            fireSource.spatialBlend = 1.0f;
            fireSource.volume = fireVolume;
            fireSource.pitch = UnityEngine.Random.Range(0.9f, 1.3f);
            fireSource.outputAudioMixerGroup = mixerGroup ?? null;
            fireSource.Stop();
        }

        if (ownShip == null)
        {
            Debug.LogWarning(name + " has no Ownship assigned.");
        }
    }

    /// <summary>
    /// Launches a spawned missile at the given target.
    /// </summary>
    /// <param name="target">If no target is given, the missile will fire without guidance.</param>
    abstract public void Launch(Transform target);

    /// <summary>
    /// Launches a spawned missile at the given target.
    /// </summary>
    /// <param name="target">If no target is given, the missile will fire without guidance.</param>
    /// <param name="velocity">Used to give a missile with a drop delay an initial velocity. Typical
    /// use case would be passing in the velocity of the launching platform.</param>
    abstract public void Launch(Transform target, Vector3 velocity);

    /// <summary>
    /// If the missile prefab has changed, calling this function will delete the currently loaded
    /// missiles and replace them with the new ones. Also resets the ammo count.
    /// </summary>
    /// initialized with.</param>
    abstract public void ResetLauncher();
}