using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    public List<GameObject> m_EnemyList;
    // Start is called before the first frame update
    public List<GameObject> AddEnemy(GameObject newEnemy)
    {
        m_EnemyList.Add(newEnemy);
        return m_EnemyList;
    }
}
