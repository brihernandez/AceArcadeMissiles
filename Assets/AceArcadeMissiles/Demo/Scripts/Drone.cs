using UnityEngine;

public class Drone : MonoBehaviour
{
    public bool turn = true;
    public float speed;
    public float turnRate;

    public bool useFixedUpdate = false;

    // Update is called once per frame
    void Update()
    {
        if (!useFixedUpdate)
        {
            if (turn)
                transform.Rotate(0.0f, turnRate * Time.deltaTime, 0.0f);

            transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (useFixedUpdate)
        {
            if (turn)
                transform.Rotate(0.0f, turnRate * Time.deltaTime, 0.0f);

            transform.Translate(0.0f, 0.0f, speed * Time.deltaTime);
        }
    }
}
