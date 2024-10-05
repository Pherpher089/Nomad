using UnityEngine;
using System.Linq;

public class TransparentObject : MonoBehaviour
{
    // The tag of the player object
    public string playerTag = "Player";

    // The alpha value to use for the transparency
    public float alpha = 0.1f;

    // The material to use for the transparency
    public Material transparentMaterial;

    // The original materials of the object
    private Material[] originalMaterials;

    // The layer mask to use for the raycast
    private int playerLayerMask;
    GameObject[] players = null;

    void Start()
    {
        // Save the original materials of the object
        originalMaterials = GetComponent<Renderer>().materials;
        // Set the player layer mask
        playerLayerMask = LayerMask.GetMask("TransparentFX");
    }

    void Update()
    {
        if (PlayersManager.Instance == null) return;
        // Check if the object is between the camera and any of the players
        bool isTransparent = false;
        if (PlayersManager.Instance.GetDistanceToClosestPlayer(transform) > 100)
        {
            return;
        }
        foreach (ThirdPersonUserControl player in PlayersManager.Instance.localPlayerList)
        {
            Vector3 playerPos;
            if (player.GetComponent<ThirdPersonCharacter>().isRiding)
            {
                playerPos = BeastManager.Instance.gameObject.transform.position;
            }
            else
            {
                playerPos = player.transform.position + new Vector3(0, 2, 0);
            }
            float dis = Vector3.Distance(playerPos, Camera.main.transform.position);
            Ray ray = new Ray(Camera.main.transform.position, playerPos - Camera.main.transform.position);

            RaycastHit[] hits = Physics.SphereCastAll(ray, 4, dis - 6f, playerLayerMask);
            if (hits.Any(hit => hit.transform == transform))
            {

                isTransparent = true;
                break;
            }
        }

        if (isTransparent)
        {
            // Set the transparency of the object
            Material[] materials = new Material[originalMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = transparentMaterial;
                Color color = transparentMaterial.color;
                //color.a = alpha;
                materials[i].color = color;
            }
            GetComponent<Renderer>().materials = materials;
        }
        else
        {
            // Reset the materials to the original materials
            GetComponent<Renderer>().materials = originalMaterials;
        }
    }
}