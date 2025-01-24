using UnityEngine;

public class ToxicGasCloud : MonoBehaviour
{
    public float damagePerSecond = 5f; // Damage dealt over time
    public float forwardSpeed = 5f;
    public float lifetime = 5f; // How long the gas cloud lasts
    [HideInInspector] public GameObject shooter; // Reference to the enemy that shot the gas cloud
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing on ToxicGasCloud!");
            return;
        }

        rb.velocity = transform.forward * forwardSpeed;

        // Debugging: Confirm that the shooter is assigned
        if (shooter != null)
        {
            Debug.Log($"ToxicGasCloud shooter assigned: {shooter.name}");
        }
        else
        {
            Debug.LogError("ToxicGasCloud shooter is not assigned!");
        }

        // Destroy the gas cloud after its lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Gas Cloud collided with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            // Deal damage over time to the player
            //PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            //if (playerHealth != null)
            //{
            //playerHealth.TakeDamage(damagePerSecond * Time.deltaTime);
            //}
            Debug.Log("Gas Cloud Hits Player");
        }
        else if (other.CompareTag("ControlPoint"))
        {
            // Deal damage to the control point
            ControlPoint controlPoint = other.GetComponent<ControlPoint>();
            if (controlPoint != null)
            {
                controlPoint.TakeDamage(5);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Bubble"))
        {
            // If hit by a bubble, trigger the absorption mechanic
            BubbleBullet bubble = other.GetComponent<BubbleBullet>();
            if (bubble != null)
            {
                bubble.AbsorbGasCloud(this); // Implement this function in BubbleBullet
            }
        }
    }
}