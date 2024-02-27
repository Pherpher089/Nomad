using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_controls_splitscreen : MonoBehaviour
{
    //First Person Controller
    public enum CurrentPlayer
    {
        Player1,
        Player2
    }
    public CurrentPlayer currentPlayer;


    public float speed = 10f;
    public Rigidbody rb;



    private void Update()
    {

        switch (currentPlayer)
        {
            case CurrentPlayer.Player1:
                //Player 1 (Uses same as First Person Player Axis)

                if (Input.GetAxis("Vertical P1") != 0)
                {
                    transform.position += transform.forward * speed * Time.deltaTime * Input.GetAxis("Vertical P1");
                    //rb.AddForce(transform.forward * speed * Input.GetAxis("Vertical P1"), ForceMode.Impulse);
                }
                if (Input.GetAxis("Horizontal P1") != 0)
                {
                    transform.position += transform.right * speed * Time.deltaTime * Input.GetAxis("Horizontal P1");
                    //rb.AddForce(transform.right * speed * Input.GetAxis("Horizontal P1"), ForceMode.Impulse);
                }
                break;

            case CurrentPlayer.Player2:
                //Player 2 (Uses a new Axis that is mapped to the arrow keys)
                if (Input.GetAxis("Vertical P2") != 0)
                {
                    transform.position += transform.forward * speed * Time.deltaTime * Input.GetAxis("Vertical P2");

                }
                if (Input.GetAxis("Horizontal P2") != 0)
                {
                    transform.position += transform.right * speed * Time.deltaTime * Input.GetAxis("Horizontal P2");
                }
                break;
        }

    }
}
