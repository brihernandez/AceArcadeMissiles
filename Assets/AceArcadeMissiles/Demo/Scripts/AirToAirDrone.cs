using UnityEngine;

public class AirToAirDrone : MonoBehaviour
{
    AALauncher launcher;

    public Transform target;
    public float turnRate = 15.0f;
    public float speed = 50.0f;

    float wait;
    float startTime;

    void Start()
    {
        startTime = Time.time;
        wait = Random.Range(2.0f, 10.0f);

        launcher = GetComponentInChildren<AALauncher>();
    }

    void FixedUpdate()
    {
        transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);

        if (target != null)
        {
            // Look at the target.
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position, transform.up), turnRate * Time.deltaTime);

            // Fire missile.
            if (Time.time - startTime > wait)
                launcher.Launch(target, transform.forward * speed);
        }
    }
}
