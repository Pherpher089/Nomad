using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAI/Decisions/IsBetterTarget")]
public class IsBetterTargetDecision : Decision
{
    public override bool Decide(StateController controller)
    {
        bool isBetterTarget = EvaluateTargets(controller);
        return isBetterTarget;
    }

    private bool EvaluateTargets(StateController controller)
    {
        if (controller.reevaluateTargetCounter < 3)
        {
            controller.reevaluateTargetCounter += Time.deltaTime;
            return false;
        }
        // Initialize priority scores for each player
        Dictionary<string, int> playerPriority = new Dictionary<string, int>();
        List<KeyValuePair<string, float>> damageList = new List<KeyValuePair<string, float>>();
        List<KeyValuePair<string, float>> distanceList = new List<KeyValuePair<string, float>>();

        foreach (ThirdPersonUserControl player in PlayersManager.Instance.playerList)
        {
            string viewID = player.GetComponent<PhotonView>().ViewID.ToString();
            if (player.GetComponent<CharacterManager>().actorState != ActorState.Dead)
            {
                playerPriority[viewID] = 0;
                damageList.Add(new KeyValuePair<string, float>(viewID, controller.playerDamageMap.ContainsKey(viewID) ? controller.playerDamageMap[viewID] : 0));
                distanceList.Add(new KeyValuePair<string, float>(viewID, Vector3.Distance(player.transform.position, controller.transform.position)));
            }
            else
            {
                damageList.Remove(new KeyValuePair<string, float>(viewID, controller.playerDamageMap.ContainsKey(viewID) ? controller.playerDamageMap[viewID] : 0));
            }
        }
        string _viewID = BeastManager.Instance.gameObject.GetComponent<PhotonView>().ViewID.ToString();
        if (BeastManager.Instance.gameObject.GetComponent<ActorManager>().actorState != ActorState.Dead)
        {
            playerPriority[_viewID] = 0;
            damageList.Add(new KeyValuePair<string, float>(_viewID, controller.playerDamageMap.ContainsKey(_viewID) ? controller.playerDamageMap[_viewID] : 0));
            distanceList.Add(new KeyValuePair<string, float>(_viewID, Vector3.Distance(BeastManager.Instance.gameObject.transform.position, controller.transform.position)));
        }
        else
        {
            damageList.Remove(new KeyValuePair<string, float>(_viewID, controller.playerDamageMap.ContainsKey(_viewID) ? controller.playerDamageMap[_viewID] : 0));
        }

        // Sort players by the damage they have dealt
        damageList.Sort((firstPair, nextPair) => nextPair.Value.CompareTo(firstPair.Value));

        // Assign points based on damage ranking
        int points = 5;
        for (int i = 0; i < damageList.Count; i++)
        {
            if (i > 0 && damageList[i].Value < damageList[i - 1].Value)
            {
                points--; // Reduce points for lower damage
            }
            if (points < 2) points = 2; // Ensure minimum points

            // Add points to player
            playerPriority[damageList[i].Key] += points;
        }

        // Sort players by proximity
        distanceList.Sort((a, b) => a.Value.CompareTo(b.Value));

        // Assign points based on proximity
        points = 5; // Reset points for proximity calculation
        for (int i = 0; i < distanceList.Count; i++)
        {
            if (i > 0 && distanceList[i].Value > distanceList[i - 1].Value)
            {
                points--; // Reduce points for greater distance
            }
            if (points < 2) points = 2; // Ensure minimum points

            // Add distance-based points
            playerPriority[distanceList[i].Key] += points;
        }

        // Determine the final target based on the highest score
        string targetId = null;
        int highestScore = int.MinValue;
        foreach (var entry in playerPriority)
        {
            if (entry.Value > highestScore)
            {
                highestScore = entry.Value;
                targetId = entry.Key;
            }
            else if (entry.Value == highestScore)
            {
                // Tie-break by proximity
                Transform currentTransform = PhotonView.Find(Int32.Parse(targetId)).transform;
                Transform newTransform = PhotonView.Find(Int32.Parse(entry.Key)).transform;
                if (Vector3.Distance(newTransform.position, controller.transform.position) < Vector3.Distance(currentTransform.position, controller.transform.position))
                {
                    targetId = entry.Key;
                }
            }
        }

        if (targetId != null)
        {
            controller.target = PhotonView.Find(Int32.Parse(targetId)).transform;
        }

        controller.reevaluateTargetCounter = 0;
        return true; // Return true to indicate the target has been successfully evaluated and set
    }
}
