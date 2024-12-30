using UnityEngine;
using System.Linq;

public class TransparentObject : MonoBehaviour
{
    public string playerTag = "Player";          // The tag of the player object
    public Material transparentMaterial;        // The material to use for transparency
    public bool affectChildren = false;         // Toggle to include child objects

    private Material[][] originalMaterials;     // Array to store original materials for each renderer
    private Renderer[] renderers;               // Array to store all relevant renderers (self or children)
    private int playerLayerMask;                // The layer mask to use for the raycast

    [HideInInspector] public bool isTransparent = false;

    void Start()
    {
        // Get renderers based on whether child objects should be affected
        renderers = affectChildren ? GetComponentsInChildren<Renderer>() : new Renderer[] { GetComponent<Renderer>() };

        // Store original materials for each renderer
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalMaterials[i] = renderers[i].materials;
            }
        }

        // Set the player layer mask
        playerLayerMask = LayerMask.GetMask("TransparentFX");
    }

    void Update()
    {
        if (PlayersManager.Instance == null) return;

        isTransparent = false;

        // Skip transparency checks if the closest player is too far away
        if (PlayersManager.Instance.GetDistanceToClosestPlayer(transform) > 100)
        {
            return;
        }

        // Check if the object (or its children) is between the camera and any of the players
        foreach (ThirdPersonUserControl player in PlayersManager.Instance.localPlayerList)
        {
            if (player == null) continue;

            Vector3 playerPos = player.GetComponent<ThirdPersonCharacter>().isRiding
                ? BeastManager.Instance.gameObject.transform.position
                : player.transform.position + new Vector3(0, 2, 0);

            float dis = Vector3.Distance(playerPos, Camera.main.transform.position);
            Ray ray = new Ray(Camera.main.transform.position, playerPos - Camera.main.transform.position);

            RaycastHit[] hits = Physics.SphereCastAll(ray, 4, dis - 6f, playerLayerMask);
            if (hits.Any(hit => hit.transform == transform))
            {
                isTransparent = true;
                break;
            }
        }

        // Apply transparency or reset materials
        if (isTransparent)
        {
            SetTransparent();
        }
        else
        {
            ResetMaterials();
        }
    }

    private void SetTransparent()
    {
        // Apply the transparent material to all renderers
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] materials = new Material[originalMaterials[i].Length];
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = transparentMaterial;
                Color color = transparentMaterial.color;
                materials[j].color = color;
            }

            renderers[i].materials = materials;
        }
    }

    private void ResetMaterials()
    {
        // Reset to the original materials for all renderers
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
    }
}
