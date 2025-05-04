using System;
using UnityEngine;
using Pathfinding;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/PortalNotAccessible")]
public class PortalNotAccessibleDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        GameObject raidTarget = null;
        if (GameStateManager.Instance.raidTarget.TryGetComponent<RestorationSiteUIController>(out var restorationSiteUIController))
        {
            raidTarget = BeastManager.Instance.gameObject;
        }
        else
        {
            raidTarget = GameStateManager.Instance.raidTarget.gameObject;
        }
        Seeker seeker = controller.GetComponent<Seeker>();
        bool isPortalAccessible = true;

        // Start an asynchronous path calculation
        seeker.StartPath(controller.transform.position, raidTarget.transform.position, (Path pathToCheck) =>
        {
            // Check if the path to the portal is valid
            if (pathToCheck.error)
            {
                isPortalAccessible = false;
                BuildingObject target = EnemiesManager.Instance.TryGetTarget();
                BuildingObject[] buildObjects = GameObject.FindObjectsOfType<BuildingObject>();

                if (target == null)
                {
                    // Find the nearest walkable node to the portal
                    var nearestNode = AstarPath.active.GetNearest(raidTarget.transform.position);
                    if (nearestNode.node != null && nearestNode.node.Walkable)
                    {
                        float shortestDistance = float.MaxValue;

                        foreach (BuildingObject buildObject in buildObjects)
                        {
                            float distance = Vector3.Distance(buildObject.transform.position, nearestNode.position);
                            if (distance < shortestDistance &&
                                buildObject.buildingPieceType == BuildingObjectType.Wall &&
                                !EnemiesManager.Instance.CheckIfTargetExists(buildObject))
                            {
                                // Calculate path to the buildObject
                                seeker.StartPath(controller.transform.position, buildObject.transform.position, (Path buildObjectPath) =>
                                {
                                    if (!buildObjectPath.error)
                                    {
                                        shortestDistance = distance;
                                        target = buildObject;
                                    }
                                });
                            }
                        }
                    }

                    if (target != null)
                    {
                        controller.target = target.transform;
                        EnemiesManager.Instance.AddNewTarget(target);

                        Vector3 finalDestination = Vector3.zero;
                        int failSafe = 0;

                        // Find a valid attack position near the target
                        while (finalDestination == Vector3.zero && failSafe < 100)
                        {
                            Vector3 randomOffset = target.transform.position - (target.transform.right * UnityEngine.Random.Range(-2f, 2f)) +
                                                   (target.transform.forward * (controller.enemyStats.attackRange - 0.25f));
                            seeker.StartPath(controller.transform.position, randomOffset, (Path randomPath) =>
                            {
                                if (!randomPath.error)
                                {
                                    finalDestination = randomOffset;
                                }
                            });
                            failSafe++;
                        }

                        if (finalDestination != Vector3.zero)
                        {
                            controller.aiPath.destination = finalDestination;
                        }
                    }
                    else
                    {
                        Debug.LogError("Portal is blocked and there are no available targets - " + name);
                    }
                }
            }
        });

        return isPortalAccessible;
    }
}
