using System;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/PortalNotAccessible")]

public class PortalNotAccessibleDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        GameObject mainPortal = GameObject.FindGameObjectWithTag("MainPortal");
        NavMeshPath pathToCheck = new();
        NavMesh.CalculatePath(controller.transform.position, mainPortal.transform.position, NavMesh.AllAreas, pathToCheck);
        // If the path to the portal is not a complete path -> i.e there are objects blocking the portal.
        if (pathToCheck.status == NavMeshPathStatus.PathPartial || pathToCheck.status == NavMeshPathStatus.PathInvalid)
        {
            BuildingObject target = EnemiesManager.Instance.TryGetTarget();
            BuildingObject[] buildObjects = GameObject.FindObjectsOfType<BuildingObject>();
            if (target == null)
            {
                if (NavMesh.SamplePosition(mainPortal.transform.position, out NavMeshHit hit1, 100, NavMesh.AllAreas))
                {
                    // Raycast from the nearest point on NavMesh to the main portal to find a wall
                    float shortestDistance = 10000;
                    foreach (BuildingObject buildObject in buildObjects)
                    {
                        float dis = Vector3.Distance(buildObject.transform.position, hit1.position);
                        if (dis < shortestDistance && buildObject.buildingPieceType == BuildingObjectType.Wall && !EnemiesManager.Instance.CheckIfTargetExists(buildObject))
                        {
                            NavMesh.CalculatePath(controller.transform.position, buildObject.transform.position + buildObject.transform.forward, NavMesh.AllAreas, pathToCheck);
                            if (pathToCheck.status == NavMeshPathStatus.PathComplete)
                            {
                                shortestDistance = dis;
                                target = buildObject;
                            }
                            else
                            {
                                NavMesh.CalculatePath(controller.transform.position, buildObject.transform.position + buildObject.transform.forward * -1, NavMesh.AllAreas, pathToCheck);
                                if (pathToCheck.status == NavMeshPathStatus.PathComplete)
                                {
                                    shortestDistance = dis;
                                    target = buildObject;
                                }
                            }
                        }
                    }
                }
                try
                {
                    controller.target = target.transform;
                    EnemiesManager.Instance.AddNewTarget(target);
                }
                catch
                {
                    Debug.LogError(" Portal is blocked and there are no available targets - " + name);
                    return true;
                }

                Vector3 finalDestination = Vector3.zero;
                int failSafe = 0;
                while (finalDestination == Vector3.zero || failSafe >= 100)
                {
                    NavMesh.CalculatePath(controller.transform.position, target.transform.position - (target.transform.right * UnityEngine.Random.Range(-2f, 2f)) + target.transform.forward * (controller.enemyStats.attackRange - 0.25f), NavMesh.AllAreas, pathToCheck);
                    if (pathToCheck.status == NavMeshPathStatus.PathComplete)
                    {
                        finalDestination = target.transform.position - (target.transform.right * UnityEngine.Random.Range(-2f, 2f)) + (target.transform.forward * (controller.enemyStats.attackRange - 0.25f));
                    }
                    else
                    {
                        NavMesh.CalculatePath(controller.transform.position, target.transform.position - (target.transform.right * UnityEngine.Random.Range(-2f, 2f)) + (target.transform.forward * -(controller.enemyStats.attackRange - 0.25f)), NavMesh.AllAreas, pathToCheck);
                        if (pathToCheck.status == NavMeshPathStatus.PathComplete)
                        {
                            finalDestination = target.transform.position - (target.transform.right * UnityEngine.Random.Range(-2f, 2f)) + (target.transform.forward * -(controller.enemyStats.attackRange - 0.25f));
                        }
                    }
                    failSafe++;
                }
                controller.navMeshAgent.SetDestination(finalDestination);
            }
            return true;
        }

        return false;
    }
}

