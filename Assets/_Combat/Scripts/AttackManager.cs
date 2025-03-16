using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
public class AttackManager : MonoBehaviour
{
    public List<GameObject> m_HaveHit = new List<GameObject>();
    private ToolType toolType;
    private int damage;
    private float hitRange;
    private float knockBackForce;
    private bool hitboxActive = false;
    [HideInInspector] public Vector3 earthMinePath;
    private GameObject mine;
    Vector3[] aimLinePoints = new Vector3[6];
    Vector3[] arcAimLinePoints = new Vector3[6];
    [HideInInspector] public LineRenderer aimingLine;
    [HideInInspector] public Animator m_Animator;
    readonly int aimLayerIndex = 3;
    readonly int throwAimLayerIndex = 4;
    //References 
    ActorEquipment actorEquipment;
    PlayerInventoryManager inventoryManager;
    PhotonView pv;
    void Start()
    {
        pv = GetComponent<PhotonView>();
        actorEquipment = GetComponent<ActorEquipment>();
        inventoryManager = GetComponent<PlayerInventoryManager>();
        m_Animator = transform.GetChild(0).GetComponent<Animator>();

        aimingLine = GetComponent<LineRenderer>();
        aimLinePoints[0] = new Vector3(0, 2f, 1.5f);
        aimLinePoints[1] = new Vector3(0, 2f, 15f);
        aimLinePoints[2] = new Vector3(0, 2f, 15f);
        aimLinePoints[3] = new Vector3(0, 2f, 15f);
        aimLinePoints[4] = new Vector3(0, 2f, 15f);
        aimLinePoints[5] = new Vector3(0, 2f, 15f);
        float x = -2;
        for (int i = 0; i < 6; i++)
        {
            arcAimLinePoints[i] = new Vector3(0, -Mathf.Pow(x, 2) + 9, x + 2);
            x += 1f;
        }
    }
    public void ResetHitbox()
    {
        m_HaveHit.Clear();
    }
    public void ActivateHitbox(ToolType type, int damageValue, float knockback, float range)
    {
        toolType = type;
        damage = damageValue;
        knockBackForce = knockback;
        hitRange = range;
        hitboxActive = true;
        m_HaveHit.Clear();
    }

    public void DeactivateHitbox()
    {
        ClearHits();
    }

    void FixedUpdate()
    {
        //CheckHit();
        CheckHitWithRays();
    }

    public void ClearHits()
    {
        m_HaveHit.Clear();
        hitboxActive = false;
    }
    private void CheckHitWithRays()
    {
        if (!hitboxActive) return;

        // Box dimensions and position
        float _hitRange = hitRange;
        Vector3 boxHalfExtents = new Vector3(1.5f, 2f, _hitRange / 2f);
        Vector3 boxCenter = transform.position + Vector3.up * 2f + transform.forward * (_hitRange / 2f + 0.5f);

        // Calculate the corners of the box in world space
        Vector3[] corners = new Vector3[8];
        Quaternion rotation = transform.rotation;

        corners[0] = boxCenter + rotation * new Vector3(-boxHalfExtents.x, -boxHalfExtents.y, -boxHalfExtents.z);
        corners[1] = boxCenter + rotation * new Vector3(boxHalfExtents.x, -boxHalfExtents.y, -boxHalfExtents.z);
        corners[2] = boxCenter + rotation * new Vector3(-boxHalfExtents.x, boxHalfExtents.y, -boxHalfExtents.z);
        corners[3] = boxCenter + rotation * new Vector3(boxHalfExtents.x, boxHalfExtents.y, -boxHalfExtents.z);
        corners[4] = boxCenter + rotation * new Vector3(-boxHalfExtents.x, -boxHalfExtents.y, boxHalfExtents.z);
        corners[5] = boxCenter + rotation * new Vector3(boxHalfExtents.x, -boxHalfExtents.y, boxHalfExtents.z);
        corners[6] = boxCenter + rotation * new Vector3(-boxHalfExtents.x, boxHalfExtents.y, boxHalfExtents.z);
        corners[7] = boxCenter + rotation * new Vector3(boxHalfExtents.x, boxHalfExtents.y, boxHalfExtents.z);

        // Visualize the box edges
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(corners[i], corners[(i + 1) % 4], Color.green, 1f); // Bottom edges
            Debug.DrawLine(corners[i + 4], corners[(i + 1) % 4 + 4], Color.green, 1f); // Top edges
            Debug.DrawLine(corners[i], corners[i + 4], Color.green, 1f); // Vertical edges
        }

        // Cast rays between corners and visualize them
        for (int i = 0; i < corners.Length; i++)
        {
            for (int j = i + 1; j < corners.Length; j++)
            {
                Vector3 direction = (corners[j] - corners[i]).normalized;
                float distance = Vector3.Distance(corners[i], corners[j]);

                // Debug ray
                Debug.DrawRay(corners[i], direction * distance, Color.red, 1f);

                // Perform the raycast
                if (Physics.Raycast(corners[i], direction, out RaycastHit hit, distance, -1, QueryTriggerInteraction.Collide))
                {
                    ProcessHit(hit);
                }
            }
        }
    }

    // Example hit processing
    private void ProcessHit(RaycastHit hit)
    {
        Transform currentTransform = hit.transform;

        while (currentTransform != null)
        {
            if (currentTransform.CompareTag("WorldTerrain") || currentTransform.CompareTag("MousePlane") || currentTransform.gameObject.name == gameObject.name)
                break;
            // Check for SpawnMotionDriver
            if (currentTransform.TryGetComponent<SpawnMotionDriver>(out var driver) && !driver.hasSaved)
                break;
            if (m_HaveHit.Contains(currentTransform.gameObject))
                break;

            m_HaveHit.Add(currentTransform.gameObject);

            if (currentTransform.TryGetComponent<BuildingMaterial>(out var bm))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(
                    bm.id,
                    bm.spawnId,
                    damage,
                    toolType,
                    hit.point,
                    GetComponent<PhotonView>()
                );
                break;
            }

            if (currentTransform.TryGetComponent<HealthManager>(out var hm))
            {
                hm.Hit(damage, toolType, hit.point, gameObject, knockBackForce);
                break;
            }

            if (currentTransform.TryGetComponent<SourceObject>(out var so))
            {
                LevelManager.Instance.CallUpdateObjectsPRC(
                    so.id,
                    "",
                    damage,
                    toolType,
                    hit.point,
                    GetComponent<PhotonView>()
                );
                break;
            }

            currentTransform = currentTransform.parent;
        }
    }


    private void CheckHit()
    {
        if (!hitboxActive) return;
        float _hitRange;
        if (tag == "Enemy")
        {
            _hitRange = hitRange * .1f;
        }
        else
        {
            _hitRange = hitRange;
        }
        // Define fixed world-space offsets for the box position
        Vector3 boxPosition = transform.position + Vector3.up * 2 + transform.forward.normalized * (_hitRange / 2 + 0.5f);
        Vector3 boxHalfExtents = new Vector3(1.5f, 2, _hitRange / 2);

        // Use a consistent world rotation
        Quaternion boxRotation = Quaternion.identity;

        // Apply correct rotation for the box
        // Perform the overlap box detection
        Collider[] hits = Physics.OverlapBox(boxPosition, boxHalfExtents, boxRotation, -1, QueryTriggerInteraction.Collide);
        DebugDrawBoxCast(boxPosition, boxHalfExtents, transform.forward, boxRotation, _hitRange, Color.green);

        foreach (Collider hit in hits)
        {
            // Start checking up the hierarchy
            Transform currentTransform = hit.transform;

            while (currentTransform != null)
            {
                // Stop searching if we reach the WorldTerrain object
                if (currentTransform.CompareTag("WorldTerrain")) break;
                if (currentTransform.gameObject.name == gameObject.name) break;
                // Check for SpawnMotionDriver
                if (currentTransform.TryGetComponent<SpawnMotionDriver>(out var driver) && !driver.hasSaved)
                    break;

                if (m_HaveHit.Contains(currentTransform.gameObject)) break;
                m_HaveHit.Add(currentTransform.gameObject);
                // Check for BuildingMaterial
                if (currentTransform.TryGetComponent<BuildingMaterial>(out var bm))
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(
                        bm.id,
                        bm.spawnId,
                        damage,
                        toolType,
                        hit.transform.position,
                        GetComponent<PhotonView>()
                    );
                    break; // No need to continue once a component is found
                }

                HealthManager hm = currentTransform.GetComponent<HealthManager>();
                // Check for HealthManager
                if (hm != null)
                {
                    hm.Hit(damage, toolType, hit.transform.position, gameObject, knockBackForce);

                    break; // No need to continue once a component is found
                }

                // Check for SourceObject
                if (currentTransform.TryGetComponent<SourceObject>(out var so))
                {
                    LevelManager.Instance.CallUpdateObjectsPRC(
                        so.id,
                        "",
                        damage,
                        toolType,
                        hit.transform.position,
                        gameObject.GetComponent<PhotonView>()
                    );
                    break; // No need to continue once a component is found
                }

                // Move to the parent object
                currentTransform = currentTransform.parent;
            }
        }
    }

    public void ShootBow()
    {

        Vector3 direction = transform.forward;

        if (tag == "Enemy")
        {
            // Special aiming logic for enemies using the StateController target
            direction = GetComponent<StateController>().target.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
            direction = direction.normalized;
        }
        else
        {
            if (inventoryManager == null)
            {
                Debug.LogError("Attack Manager: InventoryManager required to Shoot Bow for player characters");
                return;
            }
            if (actorEquipment.equippedItem == null)
            {
                Debug.LogError("Attack Manager: equippedItem required to Shoot Bow for player characters");
                return;
            }
            // Define the box cast dimensions
            Vector3 boxHalfExtents = new Vector3(0.1f, 20.0f, 0.5f); // Thin along X, tall in Y, range in Z
            Vector3 boxOrigin = transform.position; // Center of the player character
            Quaternion boxOrientation = Quaternion.identity; // Default orientation

            // Debug draw the box cast
            DebugDrawBoxCast(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, Color.green);

            // Perform the BoxCastAll and log hits
            RaycastHit[] hits = Physics.BoxCastAll(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    direction = hit.collider.transform.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
                    direction = direction.normalized;
                    break;
                }
            }
            // Ensure the player has arrows
            bool hasArrows = false;
            for (int i = 0; i < inventoryManager.items.Length; i++)
            {
                if (inventoryManager.items[i].item && inventoryManager.items[i].item.itemListIndex == 14 && inventoryManager.items[i].count > 0)
                {
                    hasArrows = true;
                    inventoryManager.RemoveItem(i, 1);
                    break;
                }
            }
            if (!hasArrows)
            {
                for (int i = 0; i < inventoryManager.beltItems.Length; i++)
                {
                    if (inventoryManager.beltItems[i].item && inventoryManager.beltItems[i].item.itemListIndex == 14 && inventoryManager.beltItems[i].count > 0)
                    {
                        hasArrows = true;
                        inventoryManager.RemoveBeltItem(i, 1);
                        break;
                    }
                }
            }
            if (!hasArrows) return;
        }

        // Instantiate the arrow
        GameObject arrow = PhotonNetwork.Instantiate(
            Path.Combine("PhotonPrefabs", "Arrow"),
            transform.position + (transform.forward * 1.5f) + (transform.up * 2f),
            Quaternion.LookRotation(direction)
        );

        // Initialize arrow properties
        arrow.GetComponent<ArrowControl>().Initialize(gameObject, actorEquipment.equippedItem);
        arrow.GetComponent<Rigidbody>().velocity = direction * 80;
        arrow.GetComponent<Rigidbody>().useGravity = true;
    }

    public void CastWand()
    {
        GameObject MagicObject;
        if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex is 55 or 83 or 90)
        {
            Vector3 direction = transform.forward;
            // Define the box cast dimensions
            Vector3 boxHalfExtents = new Vector3(0.1f, 20.0f, 0.5f); // Thin along X, tall in Y, range in Z
            Vector3 boxOrigin = transform.position; // Center of the player character
            Quaternion boxOrientation = Quaternion.identity; // Default orientation

            // Debug draw the box cast
            // DebugDrawBoxCast(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, Color.green);

            // Perform the BoxCastAll and log hits
            RaycastHit[] hits = Physics.BoxCastAll(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    direction = hit.collider.transform.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
                    direction = direction.normalized;
                    break;
                }
            }
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "MagicMissle"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(direction));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, actorEquipment.equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = direction * 25;
            return;
        }
        if (!actorEquipment.CheckForMana()) return;
        if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 49)
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "IceShardsParent"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            int childCount = MagicObject.transform.childCount;
            Transform[] magicChild = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                magicChild[i] = MagicObject.transform.GetChild(i);
            }
            for (int i = 0; i < childCount; i++)
            {
                magicChild[i].parent = null;
                magicChild[i].GetComponent<FireBallControl>().Initialize(gameObject, actorEquipment.equippedItem, false);
                magicChild[i].GetComponent<Rigidbody>().velocity = magicChild[i].up * 10;
            }
        }
        else if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50)
        {
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "RockWave"), transform.position + (transform.forward * 1.5f) + (transform.up), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<RockWallParticleController>().Initialize(this.gameObject);
            MagicObject.GetComponentInChildren<RockWallParticleController>().Initialize(this.gameObject);
        }
        else
        {
            Vector3 direction = transform.forward;
            // Define the box cast dimensions
            Vector3 boxHalfExtents = new Vector3(0.1f, 20.0f, 0.5f); // Thin along X, tall in Y, range in Z
            Vector3 boxOrigin = transform.position; // Center of the player character
            Quaternion boxOrientation = Quaternion.identity; // Default orientation

            // Debug draw the box cast
            DebugDrawBoxCast(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, Color.green);

            // Perform the BoxCastAll and log hits
            RaycastHit[] hits = Physics.BoxCastAll(boxOrigin, boxHalfExtents, transform.forward, boxOrientation, 50f, LayerMask.GetMask("Enemy"), QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    direction = hit.collider.transform.position + Vector3.up * 2 - (transform.position + transform.forward + (transform.up * 1.5f));
                    direction = direction.normalized;
                    break;
                }
            }
            MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(direction));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, actorEquipment.equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = direction * 20;
        }
    }



    public void CastWandArc()
    {

        if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex is 55 or 83 or 90)
        {
            GameObject MagicObject = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "MagicMissleBig"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            MagicObject.GetComponent<FireBallControl>().Initialize(gameObject, actorEquipment.equippedItem, false);
            MagicObject.GetComponent<Rigidbody>().velocity = (transform.forward * 10);
            return;
        }

        if (!actorEquipment.CheckForMana()) return;
        if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 49)
        {
            GameObject glacialHeal = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "GlacialHeal"), transform.position + (transform.up * 2.5f), Quaternion.LookRotation(transform.forward));
            glacialHeal.GetComponent<AoeHeal>().Initialize(gameObject);

        }
        else if (actorEquipment.equippedItem.GetComponent<Item>().itemListIndex == 50)
        {
            GameObject earthMine = mine;
            earthMine.transform.rotation = Quaternion.Euler(0, 0, 0);
            earthMine.GetComponent<EarthMineController>().Initialize(earthMinePath, this.gameObject, actorEquipment.equippedItem);
            mine = null;

        }
        else
        {
            GameObject fireBall = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBall"), transform.position + (transform.forward * 1.5f) + (transform.up * 1.5f), Quaternion.LookRotation(transform.forward));
            fireBall.GetComponent<FireBallControl>().Initialize(gameObject, actorEquipment.equippedItem, true);
            fireBall.GetComponent<Rigidbody>().velocity = (transform.forward * 7) + (transform.up * 15);
            fireBall.GetComponent<Rigidbody>().useGravity = true;
        }
    }

    public void HandleAiming(bool isAiming, bool isSprinting, bool isRolling, bool isAttacking, bool crouching, bool throwing, bool preventCrouching)
    {
        if (isAiming)
        {
            if (isSprinting || isRolling || isAttacking)
            {
                ResetArcAimLines();
                aimingLine.enabled = false;
                m_Animator.SetLayerWeight(aimLayerIndex, 0);
                m_Animator.SetLayerWeight(throwAimLayerIndex, 0);
            }
            else
            {
                if (crouching && preventCrouching || !crouching)
                {
                    aimingLine.enabled = true;
                    if (throwing)
                    {
                        UpdateArcAimLines();
                        aimingLine.SetPositions(arcAimLinePoints);
                        m_Animator.SetLayerWeight(throwAimLayerIndex, 1);
                        ReadyEarthMine();
                    }
                    else
                    {
                        aimingLine.SetPositions(aimLinePoints);
                        m_Animator.SetLayerWeight(aimLayerIndex, 1);
                    }
                }
            }
        }
        else if (m_Animator.GetLayerWeight(aimLayerIndex) > 0 && !isAiming)
        {
            if (!isSprinting && !isRolling && !isAttacking) m_Animator.SetTrigger("Shoot");
            aimingLine.enabled = false;
            m_Animator.SetLayerWeight(aimLayerIndex, 0);

        }
        else if (m_Animator.GetLayerWeight(aimLayerIndex) > 0 && !isAiming && isSprinting)
        {
            aimingLine.enabled = false;
            m_Animator.SetLayerWeight(aimLayerIndex, 0);
        }
        else if (m_Animator.GetLayerWeight(throwAimLayerIndex) > 0 && !isAiming)
        {
            if (!isSprinting && !isRolling && !isAttacking) m_Animator.SetTrigger("Shoot");
            ResetArcAimLines();
            earthMinePath = transform.TransformPoint(aimingLine.GetPosition(5));
            m_Animator.SetLayerWeight(throwAimLayerIndex, 0);
            aimingLine.enabled = false;

        }
        else if (m_Animator.GetLayerWeight(throwAimLayerIndex) > 0 && !isAiming && isSprinting)
        {
            ResetArcAimLines();
            earthMinePath = transform.TransformPoint(aimingLine.GetPosition(5));
            m_Animator.SetLayerWeight(throwAimLayerIndex, 0);
            aimingLine.enabled = false;
        }
    }
    public void UpdateArcAimLines(bool isThrowing = false)
    {
        float x = -2f;
        for (int i = 0; i < 6; i++)
        {
            arcAimLinePoints[i] = new Vector3(0, -Mathf.Pow(x, 2) + 9, arcAimLinePoints[i].z + (i * Time.deltaTime));
            x += 1f;
        }
    }

    public void ResetArcAimLines()
    {
        float x = -2f;
        for (int i = 0; i < 6; i++)
        {
            arcAimLinePoints[i] = new Vector3(0, -Mathf.Pow(x, 2) + 9, x + 2);
            x += 1f;
        }
        aimingLine.enabled = false;
        m_Animator.SetLayerWeight(aimLayerIndex, 0);
        m_Animator.SetLayerWeight(throwAimLayerIndex, 0);
    }

    public void ReadyEarthMine()
    {
        if (mine == null)
        {
            if (!actorEquipment.CheckForMana()) return;
            GameObject earthMine = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "EarthMine"), actorEquipment.m_OtherSockets[0].position, Quaternion.identity);
            earthMine.GetComponent<EarthMineController>().SetSudoParent(actorEquipment.m_OtherSockets[0].transform);
            mine = earthMine;
        }
        // earthMine.transform.parent = m_OtherSockets[0];
    }

    private void DebugDrawBoxCast(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion orientation, float distance, Color color)
    {
        // Calculate the 8 corners of the box in local space
        Vector3[] localCorners = new Vector3[8];
        localCorners[0] = new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
        localCorners[1] = new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
        localCorners[2] = new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
        localCorners[3] = new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
        localCorners[4] = new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
        localCorners[5] = new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
        localCorners[6] = new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);
        localCorners[7] = new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);

        // Apply the orientation and offset to the corners
        for (int i = 0; i < localCorners.Length; i++)
        {
            localCorners[i] = origin + orientation * localCorners[i];
        }

        // Offset for forward distance
        Vector3 forwardOffset = direction.normalized * distance;

        // Draw the edges of the box
        for (int i = 0; i < 4; i++)
        {
            // Bottom face
            Debug.DrawLine(localCorners[i], localCorners[(i + 1) % 4], color);
            // Top face
            Debug.DrawLine(localCorners[i + 4], localCorners[(i + 1) % 4 + 4], color);
            // Vertical edges
            Debug.DrawLine(localCorners[i], localCorners[i + 4], color);

            // Forward bottom face
            Debug.DrawLine(localCorners[i] + forwardOffset, localCorners[(i + 1) % 4] + forwardOffset, color);
            // Forward top face
            Debug.DrawLine(localCorners[i + 4] + forwardOffset, localCorners[(i + 1) % 4 + 4] + forwardOffset, color);
            // Forward vertical edges
            Debug.DrawLine(localCorners[i] + forwardOffset, localCorners[i + 4] + forwardOffset, color);

            // Connect original and forward faces
            Debug.DrawLine(localCorners[i], localCorners[i] + forwardOffset, color);
            Debug.DrawLine(localCorners[i + 4], localCorners[i + 4] + forwardOffset, color);
        }
    }

    private void SpawnDebugCube(Vector3 center, Vector3 halfExtents, Quaternion orientation, float duration, Color color)
    {
        // Create a cube game object
        GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.GetComponent<Collider>().isTrigger = true;

        // Set the cube's position, scale, and rotation
        debugCube.transform.position = center;
        debugCube.transform.rotation = orientation;
        debugCube.transform.localScale = halfExtents * 2; // Multiply by 2 because halfExtents represent half the size

        // Set the cube's material color (optional)
        var renderer = debugCube.GetComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = color;

        // Destroy the cube after the specified duration
        Destroy(debugCube, duration);
    }


}
