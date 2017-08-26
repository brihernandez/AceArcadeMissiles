using UnityEngine;
using System.Collections.Generic;

public class AAHardpoint : AALauncher
{
    Queue<HardpointStation> stations;
    int initialMisCount = 1;
    int spawnedMissiles = 0;

    /// <summary>
    /// Get the magazine count of a pod launcher. Hardpoints don't have magazines, so they
    /// will always return 1. 
    /// </summary>
    public override int MagazineCount { get { return 1; } }

    protected override void Awake()
    {
        base.Awake();
        stations = new Queue<HardpointStation>(launchPoints.Count);
    }

    private void Start()
    {
        initialMisCount = missileCount;
        InitializeStations();
    }

    private void Update()
    {
        // Station updates handle reloading. Don't run the reload timers if all the missiles
        // have been spawned. Account for the full set of missiles that is spawned on Start.
        if (spawnedMissiles < initialMisCount)
        {
            foreach (HardpointStation sta in stations)
            {
                // Station update returns true when a new missile was created.
                if (sta.Update(Time.deltaTime))
                    spawnedMissiles++;
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
        // Peek instead of dequeue so that failed launch commands don't cycle the stations.
        HardpointStation launchingStation = stations.Peek();

        if (launchingStation != null)
        {
            bool launchSuccessful = launchingStation.Launch(target, velocity);

            if (launchSuccessful)
            {
                if (fireSource != null)
                    fireSource.Play();

                // Put this station back at the end of the queue after a launch.
                stations.Dequeue();
                stations.Enqueue(launchingStation);

                missileCount--;
            }
        }
    }

    /// <summary>
    /// If the missile prefab has changed, calling this function will delete the currently loaded
    /// missiles and replace them with the new ones.
    /// </summary>
    /// initialized with.</param>
    [ContextMenu("Reset Launcher")]
    public override void ResetLauncher()
    {
        missileCount = initialMisCount;

        foreach (HardpointStation station in stations)
            station.ClearHardpoint();

        InitializeStations();
    }

    /// <summary>
    /// Rebuilds the stations queue.
    /// </summary>
    private void InitializeStations()
    {
        stations.Clear();
        spawnedMissiles = 0;

        // Spawn missiles on each of the launchpoints.
        // Note that if the missile count is less than launch points, this will cause funniness.
        foreach (Transform point in launchPoints)
        {
            HardpointStation newStation = new HardpointStation(fireDelay, missilePrefabToLaunch, point, ownShip);
            stations.Enqueue(newStation);
            spawnedMissiles++;
        }
    }

    /// <summary>
    /// Helper class that spawns missiles at the ready and launches them on command.
    /// </summary>
    internal class HardpointStation
    {
        public AAMissile loadedMissile;

        AAMissile prefab;
        Transform launchPoint;
        Transform ownShip;

        float reloadTime = 1.0f;
        float cooldown = 0.0f;

        /// <param name="reloadTime">Time to reload the station with a new missile.</param>
        /// <param name="missilePrefab">Missile to spawn.</param>
        /// <param name="launchPoint">Point where the spawned missile will attach to.</param>
        /// <param name="ownShip">Launching object. Used to prevent missile colliding with self.</param>
        public HardpointStation(float reloadTime, AAMissile missilePrefab, Transform launchPoint, Transform ownShip)
        {
            prefab = missilePrefab;
            this.reloadTime = reloadTime;
            this.launchPoint = launchPoint;
            this.ownShip = ownShip;
            cooldown = 0.0f;

            loadedMissile = CreateMissile(launchPoint);
        }

        /// <summary>
        /// Automatically reloads missiles based on a cooldown.
        /// </summary>
        /// <param name="deltaTime">Time between frames. Usually Time.DeltaTime.</param>
        /// <returns>True when a missile was spawned on this frame.</returns>        
        public bool Update(float deltaTime)
        {
            bool spawnedNewMissle = false;

            if (loadedMissile == null)
            {
                cooldown -= deltaTime;

                // Finished reloading, spawn new missile.
                if (cooldown <= 0.0f)
                {
                    loadedMissile = CreateMissile(launchPoint);
                    spawnedNewMissle = true;
                }
            }

            return spawnedNewMissle;
        }

        /// <param name="target">If no target is given, the missile will fire without guidance.</param>
        /// <param name="inheritedVelocity">Used to give a missile with a drop delay an initial velocity. Typical
        /// use case would be passing in the velocity of the launching platform.</param>
        public bool Launch(Transform target, Vector3 inheritedVelocity)
        {
            bool successfulLaunch = false;

            if (loadedMissile != null)
            {
                loadedMissile.Launch(target, inheritedVelocity);
                loadedMissile = null;
                cooldown = reloadTime;

                successfulLaunch = true;
            }

            return successfulLaunch;
        }

        public void ClearHardpoint()
        {
            if (loadedMissile != null)
            {
                Destroy(loadedMissile.gameObject);
                loadedMissile = null;
            }

            cooldown = 0.0f;
        }

        /// <summary>
        /// Spawns a missile on a station.
        /// </summary>
        /// <param name="newParent">Point where the missile will be attached to.</param>
        /// <returns></returns>
        private AAMissile CreateMissile(Transform newParent)
        {
            AAMissile mis = Instantiate(prefab, newParent);
            mis.ownShip = ownShip;

            // Attach the missile to the hardpoint by its attach point if possible.
            if (mis.attachPoint != null)
            {
                mis.transform.localPosition = -mis.attachPoint.localPosition;
                mis.transform.localEulerAngles = -mis.attachPoint.localEulerAngles;
            }
            else
            {
                mis.transform.localPosition = Vector3.zero;
                mis.transform.localEulerAngles = Vector3.zero;
            }

            return mis;
        }
    }
}