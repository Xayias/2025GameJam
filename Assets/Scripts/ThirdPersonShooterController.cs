using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity;
    [SerializeField] private float aimSensitivity;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;

    // Bubble Gun Variables
    [Header("Bubble Gun Settings")]
    public GameObject bubblePrefab; // The bubble bullet prefab
    public Transform shootPoint;   // Where the bubble spawns from
    public float shootForce = 10f; // Speed of the bubble bullet
    public float shootCooldown = 0.5f; // Time between shots

    private float _lastShootTime; // Tracks cooldown between shots

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;

    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
        }
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSensitivity);
            thirdPersonController.SetRotateOnMove(false);
            animator.SetBool("isAiming", true);

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSensitivity);
            thirdPersonController.SetRotateOnMove(true);
            animator.SetBool("isAiming", false);
        }

        // Handle Shooting
        if (starterAssetsInputs.shoot && Time.time >= _lastShootTime + shootCooldown)
        {
            ShootBubble(mouseWorldPosition);
            _lastShootTime = Time.time; // Update the last shoot time
        }
    }

    private void ShootBubble(Vector3 targetPosition)
    {
        Vector3 aimDir = (targetPosition - shootPoint.position).normalized;

        // Trigger the shooting animation
        animator.SetTrigger("Shoot"); // Plays the shooting animation

        // Instantiate the bubble
        GameObject bubble = Instantiate(bubblePrefab, shootPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));

        // Apply force to the bubble if it has a Rigidbody
        Rigidbody bubbleRb = bubble.GetComponent<Rigidbody>();
        if (bubbleRb != null)
        {
            bubbleRb.velocity = aimDir * shootForce;
        }
    }
}
