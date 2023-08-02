using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;
    void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(true);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(false);
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(true);
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.collider.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(true);
    }
    void OnCollisionExit(Collision other)
    {

        if (other.collider.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(false);
    }
    void OnCollisionStay(Collision other)
    {
        if (other.collider.gameObject == playerController.gameObject) return;
        playerController.SetGroundedState(true);
    }
}
