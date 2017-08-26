using UnityEngine;

public class AAPod : AALauncher
{
    [Header("Missile pod specific options:")]

    [Tooltip("Randomized angle at which the missile comes out of the pod at. Higher numbers mean a bigger spread.")]
    public float dispersionAngle = 0.0f;

    [Tooltip("Number of times the pod can additionally reload after all missiles have been fired from the first salvo or a manual reload is triggered.")]
    public int magazineCount = 1;

    [Tooltip("Time to reload a missile pod magazine.\n\nAll missiles must be depleted to start a reload. Alternatively, you can manually call the \"ReloadPod\" function.")]
    public float magazineReloadTime = 6.0f;

    int tubeCount = 0;
    int initialMissileCount = 1;
    int initialMagCount = 1;

    private void Start()
    {
        initialMissileCount = missileCount;
        initialMagCount = magazineCount;
    }

    private void Update()
    {
        // In the middle of a cooldown.
        if (reloadCooldown > 0.0f)
        {
            reloadCooldown -= Time.deltaTime;
            if (reloadCooldown <= 0.0f)
                reloadCooldown = 0.0f;
        }

        // In the middle of a magazine reload.
        if (magazineCount > 0 && magazineReloadCooldown > 0.0f)
        {
            magazineReloadCooldown -= Time.deltaTime;

            // Finished reloading.
            if (magazineReloadCooldown <= 0.0f)
            {
                missileCount = initialMissileCount;
                reloadCooldown = 0.0f;
                magazineCount--;
            }
        }
    }

    /// <summary>
    /// Launches a spawned missile at the given target.
    /// </summary>
    /// <param name="target">If no target is given, the missile will fire without guidance.</param>
    public override void Launch(Transform target)
    {
        Launch(target, Vector3.zero);
    }

    /// <summary>
    /// Launches a spawned missile at the given target.
    /// </summary>
    /// <param name="target">If no target is given, the missile will fire without guidance.</param>
    /// <param name="velocity">Used to give a missile with a drop delay an initial velocity. Typical
    /// use case would be passing in the velocity of the launching platform.</param>
    public override void Launch(Transform target, Vector3 velocity)
    {
        if (missileCount > 0 && reloadCooldown <= 0.0f && magazineReloadCooldown <= 0.0f)
        {
            if (fireSource != null)
                fireSource.Play();

            // Random deviation.
            Vector3 deviation = UnityEngine.Random.insideUnitCircle * (dispersionAngle * Mathf.Deg2Rad);
            deviation = launchPoints[tubeCount].TransformDirection(deviation);
            Vector3 randomizedForward = launchPoints[tubeCount].forward + deviation;
            Quaternion randomizedRotation = Quaternion.LookRotation(randomizedForward);

            AAMissile missile = CreateMissile(launchPoints[tubeCount].position, randomizedRotation);
            missile.target = target;
            missile.Launch(target, velocity);
            reloadCooldown = fireDelay;

            missileCount--;

            // Cycle through launch points.
            tubeCount++;
            if (tubeCount >= launchPoints.Count)
                tubeCount = 0;

            // Reload magazine if all out of missiles.
            if (missileCount <= 0)
                ReloadMagazine();
        }
    }

    /// <summary>
    /// Used to reset the ammo on a rocket pod. Intended to be used when the missile prefab is changed. 
    /// </summary>
    public override void ResetLauncher()
    {
        reloadCooldown = 0.0f;
        magazineReloadCooldown = 0.0f;
        missileCount = initialMissileCount;
        magazineCount = initialMagCount;

        // Missile pods don't actually have anything to worry about in terms of the missile
        // itself because it's spawned dynamically anyway. Only the hardpoint has to despawn
        // the old missile and recreate it.
    }

    /// <summary>
    /// Tell the missile launcher to manually load the next magazine. Note that any missiles left
    /// in this magazine will be discarded.
    /// </summary>
    public void ReloadMagazine()
    {
        magazineReloadCooldown = magazineReloadTime;
    }

    /// <summary>
    /// Get the magazine count of a pod launcher. Hardpoints don't have magazines, so they
    /// will always return 1. 
    /// </summary>
    /// <returns>Number of magazines left in this rocket pod.</returns>
    public override int MagazineCount
    {
        get
        {
            return magazineCount;
        }
    }

    /// <summary>
    /// Spawns and fires a missile.
    /// </summary>
    /// <param name="position">Where the missile spawns.</param>
    /// <param name="rotation">Initial rotation of the missile.</param>
    /// <returns></returns>
    private AAMissile CreateMissile(Vector3 position, Quaternion rotation)
    {
        AAMissile mis = Instantiate(missilePrefabToLaunch) as AAMissile;
        mis.ownShip = ownShip;
        mis.transform.position = position;
        mis.transform.rotation = rotation;

        return mis;
    }
}