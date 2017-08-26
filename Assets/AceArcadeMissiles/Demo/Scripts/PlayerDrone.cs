using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerDrone : MonoBehaviour
{
    public Transform target;

    public float thrust;
    public float yaw;
    public float pitch;

    public Text msl;
    public Text xma;
    public Text rcl;
    public Text spd;

    public Image selectMSL;
    public Image selectXMA;
    public Image selectRCL;

    new Rigidbody rigidbody;

    const float FORCEMULT = 100.0f;

    int selectedLauncherGroup = 0;
    
    Queue<AALauncher>[] launchers;
    AALauncher[] allLaunchers;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        
        launchers = new Queue<AALauncher>[3];
        for (int i=0; i<3; i++)
            launchers[i] = new Queue<AALauncher>();
    }

    void Start()
    {
        allLaunchers = GetComponentsInChildren<AALauncher>();

        // Register all the launchers in their appropriate slots so they can be switched between.
        foreach (AALauncher launcher in allLaunchers)
        {
            if (launcher.name.StartsWith("MSL"))
            {
                launchers[0].Enqueue(launcher);
            }
                
            else if (launcher.name.StartsWith("XMA"))
            {
                launchers[1].Enqueue(launcher);
            }
                
            else if (launcher.name.StartsWith("RCL"))
            {
                launchers[2].Enqueue(launcher);
            }
        }
    }

    void Update()
    {
        // Cycle launcher groups.
        if (Input.GetButtonDown("Fire2"))
        {
            selectedLauncherGroup++;
            if (selectedLauncherGroup >= 3)
                selectedLauncherGroup = 0;
        }

        // Fire selected launcher group.
        // Rockets allowed to hold fire down.
        if (selectedLauncherGroup == 2)
        {
            if (Input.GetButton("Fire1"))
                FireWeapon();
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
                FireWeapon();
        }

        // Reset all weapons.
        if (Input.GetKeyDown(KeyCode.X))
        {
            foreach (AALauncher launcher in allLaunchers)
                launcher.ResetLauncher();
        }

        UpdateAmmoCounters();
        spd.text = string.Format("{0:000}", rigidbody.velocity.magnitude);
    }

    void FixedUpdate()
    {
        float inPitch = Input.GetAxis("Vertical");
        float inYaw = Input.GetAxis("Horizontal");

        rigidbody.AddRelativeForce(0.0f, 0.0f, thrust * FORCEMULT * Time.deltaTime);

        rigidbody.AddRelativeTorque(inPitch * pitch * FORCEMULT * Time.deltaTime,
                                    inYaw * yaw * FORCEMULT * Time.deltaTime,
                                    -inYaw * yaw * FORCEMULT * 0.5f * Time.deltaTime);
    }

    private void FireWeapon()
    {
        // Fire the next launcher, then put it back at the end of the queue.
        if (launchers[selectedLauncherGroup].Count > 0)
        {
            AALauncher temp = launchers[selectedLauncherGroup].Dequeue();
            temp.Launch(target, rigidbody.velocity);
            launchers[selectedLauncherGroup].Enqueue(temp);
        }
    }

    private void UpdateAmmoCounters()
    {
        // Update the ammo counters.
        int missileCount = 0;
        int xmaCount = 0;
        int rocketCount = 0;
        int rocketMagazine = 0;

        // This whole method is pretty inefficient, especially because it's in the update, but
        // this is just for the sake of demo.
        foreach (AALauncher launcher in allLaunchers)
        {
            if (launcher.name.StartsWith("MSL"))
                missileCount += launcher.missileCount;
            else if (launcher.name.StartsWith("XMA"))
                xmaCount += launcher.missileCount;
            else if (launcher.name.StartsWith("RCL"))
            {
                rocketCount += launcher.missileCount;
                rocketMagazine += launcher.MagazineCount;
            }
        }

        msl.text = string.Format("{0:00}", missileCount);
        xma.text = string.Format("{0:00}", xmaCount);
        rcl.text = string.Format("{0:00}-{1:00}", rocketCount, rocketMagazine);

        selectMSL.enabled = (selectedLauncherGroup == 0) ? true : false;
        selectXMA.enabled = (selectedLauncherGroup == 1) ? true : false;
        selectRCL.enabled = (selectedLauncherGroup == 2) ? true : false;
    }
}