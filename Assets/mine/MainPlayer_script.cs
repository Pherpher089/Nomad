using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer_script : MonoBehaviour
{
    // Start is called before the first frame update

    //public player movement and jump variables
    public int player_speed = 5;
    public int player_jump_height = 2;

    //Mouse Movement private variables and controls initiations
    private Transform cam;
    private Vector3 camRotation;

    [Range(-45, -15)]
    public int minAngle = -30;
    [Range(30, 80)]
    public int maxAngle = 45;
    [Range(50, 500)]
    public int sensitivity = 200;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    void Start()
    {
        
    }
   
    // Update is called once per frame
    void Update()
    {
        //Player Movement and Input
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * player_speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * player_speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * player_speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * player_speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * player_jump_height * Time.deltaTime);
        }

        Rotate();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * sensitivity * Time.deltaTime * Input.GetAxis("Mouse X"));

        camRotation.x -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
        camRotation.x = Mathf.Clamp(camRotation.x, minAngle, maxAngle);
        cam.localEulerAngles = camRotation;

    }

}
