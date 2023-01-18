
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBuildController : MonoBehaviour
{
    public ThirdPersonUserControl player;
    public Vector2 itemIndexRange;
    public int itemIndex = 0;
    float groundHeight = 0;
    bool buildInputCoolDown = false;
    GenerateLevel levelMaster;


    // Start is called before the first frame update
    void Start()
    {
        levelMaster = GameObject.FindWithTag("LevelMaster").GetComponent<GenerateLevel>();
        SnapPosToGrid();

    }

    // Update is called once per frame
    void Update()
    {
        if (player)
        {
            float h = Input.GetAxis(player.playerPrefix + "Horizontal");
            float v = Input.GetAxis(player.playerPrefix + "Vertical");

            // Handle Input cool down
            if (buildInputCoolDown && v < 0.2f && h < 0.2f && v > -0.2f && h > -0.2f)
            {
                buildInputCoolDown = false;
            }

            //check for player to handle input accordingly
            if (player.playerPrefix == "sp")
            { // if mouse and keyboard
                if (Input.GetButtonDown(player.playerPrefix + "Horizontal") || Input.GetButtonDown(player.playerPrefix + "Vertical"))
                {
                    Move(h, v);
                }
            }
            else
            { //if game pad
                if (!buildInputCoolDown && v + h != 0)
                {
                    Move(h, v);
                    buildInputCoolDown = true;
                }
            }


            if (Input.GetButtonDown(player.playerPrefix + "Grab") || Input.GetButtonDown(player.playerPrefix + "Build"))
            {
                int l = Input.GetButtonDown(player.playerPrefix + "Grab") ? 1 : 0;
                Rotate(l);
            }

            if (Input.GetButtonDown(player.playerPrefix + "Fire1") || Input.GetAxis(player.playerPrefix + "Fire1") > 0)
            {
                player.gameObject.GetComponent<BuilderManager>().isBuilding = false;
                player.GetComponent<ActorEquipment>().SpendItem();
                GameObject buildPiece;
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    if (gameObject.transform.GetChild(i).gameObject.activeSelf == true)
                    {
                        buildPiece = gameObject.transform.GetChild(i).gameObject;
                        buildPiece.transform.SetParent(null);
                        levelMaster.UpdateObjects(buildPiece);
                    }
                }
                Destroy(this);
            }

            if (Input.GetButtonDown(player.playerPrefix + "Build"))
            {
                CycleBuildPiece();
            }
        }
    }

    public void CycleBuildPiece()
    {
        // Get the list of children
        transform.GetChild(itemIndex).gameObject.SetActive(false);
        if (itemIndex + 1 < itemIndexRange.y)
        {
            transform.GetChild(itemIndex + 1).gameObject.SetActive(true);
            itemIndex = itemIndex + 1;
        }
        else
        {
            transform.GetChild((int)itemIndexRange.x).gameObject.SetActive(true);
            itemIndex = (int)itemIndexRange.x;
        }
    }


    void Rotate(int dir)
    {

        transform.rotation *= Quaternion.AngleAxis(90 * dir, Vector3.up);
    }


    void Move(float x, float z)
    {
        Vector3 targetPos = Vector3.zero;

        if (x < 0)
        {
            targetPos.x = -1;
        }
        else if (x > 0)
        {
            targetPos.x = 1;
        }

        if (z < 0)
        {
            targetPos.z = -1;
        }
        else if (z > 0)
        {
            targetPos.z = 1;
        }
        targetPos.y = groundHeight;
        transform.position += targetPos;
        SnapPosToGrid();
    }

    void SnapPosToGrid()
    {
        float x = Mathf.Round(transform.position.x);
        float z = Mathf.Round(transform.position.z);
        transform.position = new Vector3(x, groundHeight, z);
    }
}
