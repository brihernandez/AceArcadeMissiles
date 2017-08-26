using UnityEngine;

public class DroneTurret : MonoBehaviour
{
    AALauncher launcher;
    Transform launchTransform;

    public Transform target;

    float wait;
    float startTime;

    void Start()
    {
        startTime = Time.time;
        wait = Random.Range(2.0f, 10.0f);
        
        launcher = GetComponentInChildren<AALauncher>();
        launchTransform = launcher.GetComponent<Transform>();
    }

    void Update()
    {
        if (target != null)
        {
            // Look at the target.
            launchTransform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);

            if (Time.time - startTime > wait)
                launcher.Launch(target);
        }
    }
    
}