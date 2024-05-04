using System.Collections.Generic;
using UnityEngine;

public class TentManager : MonoBehaviour
{
    public GameObject m_Bounds;
    GameObject[] m_BoundsVisualObjs;

    void Awake()
    {
        m_Bounds = transform.GetChild(0).gameObject;
        m_BoundsVisualObjs = new GameObject[m_Bounds.transform.childCount];
        for (int i = 0; i < m_Bounds.transform.childCount; i++)
        {
            m_BoundsVisualObjs[i] = m_Bounds.transform.GetChild(i).gameObject;
        }
        if (GetComponentInParent<ObjectBuildController>() == null)
        {
            TurnOffBoundsVisuals();
        }
    }
    private void Start()
    {
        if (GetComponent<BuildingObject>().isPlaced)
        {
            GameStateManager.Instance.currentTent = this;
        }
    }

    public void TurnOffBoundsVisuals()
    {
        for (int i = 0; i < m_BoundsVisualObjs.Length; i++)
        {
            m_BoundsVisualObjs[i].SetActive(false);
        }
    }
    public void TurnOnBoundsVisuals()
    {
        for (int i = 0; i < m_BoundsVisualObjs.Length; i++)
        {
            m_BoundsVisualObjs[i].SetActive(true);
        }
    }
}
