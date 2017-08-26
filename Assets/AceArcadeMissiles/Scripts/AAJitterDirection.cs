using System.Collections;
using UnityEngine;

public class AAJitterDirection : MonoBehaviour
{
    [Tooltip("Speed at which the jitter's angle moves towards a new angle.")]
    public float jitterSpeed = 90.0f;

    [Tooltip("Maximum angle a new target jitter can be.")]
    [Range(0.0f, 90.0f)]
    public float jitterAngleMax = 5.0f;

    [Tooltip("How often a new angle for the jitter to move to is created.")]
    public float jitterRefreshRate = 15.0f;

    private Vector3 startingEulers;
    private Vector3 newJitterRot;
    private Vector3 jitterRot = Vector3.zero;

    // Use this for initialization
    private void Start()
    {
        startingEulers = transform.localEulerAngles;
        StartCoroutine(NewRotationTarget());
    }

    // Update is called once per frame
    private void Update()
    {
        jitterRot = Vector3.MoveTowards(jitterRot, newJitterRot, jitterSpeed * Time.deltaTime);
        transform.localEulerAngles = startingEulers + jitterRot;
    }

    private IEnumerator NewRotationTarget()
    {
        while (true)
        {
            newJitterRot.x = Random.Range(-jitterAngleMax, jitterAngleMax);
            newJitterRot.y = Random.Range(-jitterAngleMax, jitterAngleMax);
            newJitterRot.z = Random.Range(-jitterAngleMax, jitterAngleMax);

            // Prevent division by zero.
            if (jitterSpeed <= 0.0f)
                jitterSpeed = 1.0f;

            yield return new WaitForSeconds(1.0f / jitterRefreshRate);
        }
    }
}
