using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTransparency : MonoBehaviour
{
    // A dictionary to store the original materials and colors of the objects
    Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

    // Update is called once per frame
    void Update()
    {
        // Find all objects with the "Player" tag
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // Iterate through the array of player objects
        foreach (GameObject player in players)
        {
            // Cast a ray from the camera to the player object
            Ray ray = new Ray(transform.position, player.transform.position - transform.position);
            RaycastHit hit;

            // Check if the ray hits the player object
            if (Physics.Raycast(ray, out hit))
            {
                // If the ray hits the player object, make the object partially transparent
                Renderer renderer = hit.collider.GetComponent<Renderer>();

                // Store the original material and color of the object
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = renderer.material;
                    originalColors[renderer] = renderer.material.color;
                }

                // Set the material and color of the object to partially transparent
                renderer.material.color = new Color(1, 1, 1, 0.1f);
            }
            else
            {
                // If the ray does not hit the player object, restore the original material and color
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (originalMaterials.ContainsKey(renderer))
                {
                    renderer.material = originalMaterials[renderer];
                    renderer.material.color = originalColors[renderer];
                }
            }
        }
    }
}
