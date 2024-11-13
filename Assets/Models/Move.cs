using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Run Animation
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetBool("isRunning", true);
            animator.SetBool("isShooting", false);
        }

        // Shooting Animation (can shoot while running)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (animator.GetBool("isRunning"))
            {
                // Allow shooting while running
                animator.SetBool("isShooting", true); animator.SetBool("isRunning", false);
            }
            
            else
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isShooting", true);
            }
        }

        // Die Animation
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetBool("isDead", true);
            animator.SetBool("isRunning", false);
            animator.SetBool("isShooting", false);
        }

        // Idle Animation
        if (Input.GetKeyDown(KeyCode.I))
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isShooting", false);
            animator.SetBool("isDead", false);
            animator.SetBool("Idle", true);
        }
    }
}
