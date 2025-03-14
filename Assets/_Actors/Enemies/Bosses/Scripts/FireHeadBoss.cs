using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class FireHeadBoss : MonoBehaviour
{
    public HealthManager m_HealthManager;
    public Transform m_TargetPillar;
    public StateController m_StateController;
    public float m_CurrentHealthThreshold;
    ActorSpawner[] spawners;
    bool canSpawn = true;
    void Start()
    {
        m_HealthManager = GetComponent<HealthManager>();
        m_CurrentHealthThreshold = m_HealthManager.maxHealth / 3 * 2;
        spawners = FindObjectsOfType<ActorSpawner>();
        m_StateController = GetComponent<StateController>();
    }

    void Update()
    {
        if (m_HealthManager.dead) return;

        if (m_StateController.currentState.name.Contains("Pillar"))
        {
            //m_StateController.rigidbodyRef.isKinematic = false;
            m_StateController.aiPath.enabled = false;
            if (m_TargetPillar != null)
            {
                transform.position = m_TargetPillar.GetChild(0).position;
            }
        }
        else
        {
            //m_StateController.rigidbodyRef.isKinematic = true;
            m_StateController.aiPath.enabled = true;
        }
    }
}
