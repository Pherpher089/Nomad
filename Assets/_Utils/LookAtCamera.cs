using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        // Rotate the object towards the camera
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);    
    }
}
