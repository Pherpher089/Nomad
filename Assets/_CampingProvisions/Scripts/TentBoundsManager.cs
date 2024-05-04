using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentBoundsManager : MonoBehaviour
{
    BuildingObject m_ParentBuildingObject;
    BoxCollider m_BoundCollider;
    // Start is called before the first frame update
    void Start()
    {
        m_ParentBuildingObject = GetComponentInParent<BuildingObject>();
        m_BoundCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_ParentBuildingObject.isPlaced && !m_BoundCollider.enabled)
        {
            m_BoundCollider.enabled = true;
            m_BoundCollider.isTrigger = true;
        }
        if (!m_ParentBuildingObject.isPlaced && m_BoundCollider.enabled)
        {
            m_BoundCollider.enabled = false;
        }
    }
}
