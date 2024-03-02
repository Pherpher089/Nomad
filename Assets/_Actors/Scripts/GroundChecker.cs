using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    ThirdPersonCharacter thirdPersonCharacter;
    void Awake()
    {
        thirdPersonCharacter = GetComponentInParent<ThirdPersonCharacter>();
    }
    void OnTriggerStay(Collider other)
    {
        thirdPersonCharacter.m_IsGrounded = true;
    }

    void OnTriggerExit(Collider other)
    {
        thirdPersonCharacter.m_IsGrounded = false;
    }
}
