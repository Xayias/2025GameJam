using UnityEngine;

public class FloatingShield : MonoBehaviour
{
    public Transform bossTransform;
    public float orbitSpeed = 50f;
    public float orbitRadius = 2f;

    private Vector3 orbitOffset;

    private void Start()
    {
        if (bossTransform != null)
        {
            // Calculate the initial offset to maintain the orbit radius
            orbitOffset = (transform.position - bossTransform.position).normalized * orbitRadius;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bossTransform != null)
        {
            // Orbit around the boss's position
            transform.RotateAround(bossTransform.position, Vector3.up, orbitSpeed * Time.deltaTime);

            // Maintain the orbit radius by moving the shield outward along its offset direction
            Vector3 newPosition = (transform.position - bossTransform.position).normalized * orbitRadius;
            transform.position = bossTransform.position + newPosition;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bubble"))
        {
            // Shield is Destroyed when hit by a bubble
            Debug.Log("Shield hit by bubble");
            Destroy(gameObject);
        }
    }
}
