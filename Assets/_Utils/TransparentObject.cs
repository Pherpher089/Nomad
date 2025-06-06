using UnityEngine;
using System.Linq;

public class TransparentObject : MonoBehaviour
{
    public string playerTag = "Player";                  // The tag of the player object
    public Material transparentMaterial;                 // The material to use for transparency
    public bool affectChildren = false;                  // Toggle to include child objects

    private Material[][] originalMaterials;              // Original materials
    private Renderer[] renderers;                        // Cached renderers
    private int playerLayerMask;                         // Layer mask for raycast

    private float nextCheckTime = 0f;                    // Throttle timer
    private float checkInterval = 0.2f;                  // Interval in seconds
    private bool wasTransparentLastFrame = false;        // To avoid redundant material swapping

    [HideInInspector] public bool isTransparent = false;

    void Start()
    {
        renderers = affectChildren
            ? GetComponentsInChildren<Renderer>()
            : new Renderer[] { GetComponent<Renderer>() };

        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalMaterials[i] = renderers[i].materials;
            }
        }

        playerLayerMask = LayerMask.GetMask("TransparentFX");
    }

    void Update()
    {
        if (Time.time < nextCheckTime || PlayersManager.Instance == null)
            return;

        nextCheckTime = Time.time + checkInterval;
        isTransparent = false;

        // Skip check if no local players or player is too far
        float closestDistance = PlayersManager.Instance.GetDistanceToClosestPlayer(transform);
        if (closestDistance > 10f)
            return;

        foreach (ThirdPersonUserControl player in PlayersManager.Instance.localPlayerList)
        {
            if (player == null) continue;

            Vector3 playerPos = player.GetComponent<ThirdPersonCharacter>().isRiding
                ? BeastManager.Instance.transform.position
                : player.transform.position + new Vector3(0, 2, 0);

            Vector3 camPos = Camera.main.transform.position;
            float distance = Vector3.Distance(playerPos, camPos);
            Vector3 direction = (playerPos - camPos).normalized;

            Ray ray = new Ray(camPos, direction);
            RaycastHit[] hits = Physics.SphereCastAll(ray, 4f, distance - 6f, playerLayerMask);

            if (hits.Any(hit => hit.transform == transform))
            {
                isTransparent = true;
                break;
            }
        }

        if (isTransparent && !wasTransparentLastFrame)
        {
            SetTransparent();
            wasTransparentLastFrame = true;
        }
        else if (!isTransparent && wasTransparentLastFrame)
        {
            ResetMaterials();
            wasTransparentLastFrame = false;
        }
    }
    public void ForceTransparentRefresh()
    {
        SetTransparent(); // Ensures material is correctly reapplied
    }

    private void SetTransparent()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] materials = new Material[originalMaterials[i].Length];
            for (int j = 0; j < materials.Length; j++)
            {
                materials[j] = transparentMaterial;
            }

            renderers[i].materials = materials;
        }
    }

    private void ResetMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
    }
}
