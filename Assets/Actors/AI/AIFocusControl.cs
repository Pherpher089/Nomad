using UnityEngine;
using System.Collections;

public class AIFocusControl : MonoBehaviour
{
    public bool hasTarget;
    public float turnSpeed = 5;
    Transform target = null;
    Rigidbody rigidbodyRef;
    public StateController controller;

    private void Awake()
    {
        rigidbodyRef = GetComponent<Rigidbody>();
        controller = GetComponent<StateController>();
    }

    private void Update()
    {
        if(controller.focusOnTarget)
        {
            FocusOn(controller.chaseTarget);
        }
        else
        {
            //FaceVelocity();
        }
    }

    public void FocusOn(Vector3 _target)
    {
        Vector3 dir = _target - transform.position;
        dir = dir.normalized;

        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        controller.rigidbodyRef.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
    }

    public void FaceVelocity()
    {
        Vector3 dir = rigidbodyRef.velocity.normalized;
        if(dir != Vector3.zero) { 
            Quaternion look = Quaternion.LookRotation(dir, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * turnSpeed);
        }
    }
}
