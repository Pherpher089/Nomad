using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class fpc_WPIDEMO : MonoBehaviour
{
    //First Person Controller

    public float speed = 10f;
    public float sensitivityX = 2f;
    public float sensitivityY = 2f;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationY = 0f;
    float rotationX = 0f;
    public static bool playerCanMove = false;

    private void Update()
    {
        if (playerCanMove)
        {
            
            #region Movement: Translate

            if (Input.GetAxis("Vertical") != 0)
            {
                transform.position += transform.forward * speed * Time.deltaTime * Input.GetAxis("Vertical");

            }
            if (Input.GetAxis("Horizontal") != 0)
            {
                transform.position += transform.right * speed * Time.deltaTime * Input.GetAxis("Horizontal");
            }

            #endregion

            #region Mouse Look
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0); //cam freely looks up and down and left/right
            #endregion

        }
    }
}
