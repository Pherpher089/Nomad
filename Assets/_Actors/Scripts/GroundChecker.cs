using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    ThirdPersonCharacter thirdPersonCharacter;
    StateController stateController;
    void Awake()
    {
        thirdPersonCharacter = GetComponentInParent<ThirdPersonCharacter>();
        stateController = GetComponentInParent<StateController>();
    }
    void OnTriggerStay(Collider other)
    {
        if (thirdPersonCharacter != null) thirdPersonCharacter.m_IsGrounded = true;
        if (stateController != null) stateController.isGrounded = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (thirdPersonCharacter != null) thirdPersonCharacter.m_IsGrounded = false;
        if (stateController != null) stateController.isGrounded = false;

    }
}
