using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public float m_StomachCapacity = 100;
    public float m_StomachValue = 100;
    public float m_HungerDeteriorationValue = .05f;

    HealthManager m_HealthManager;
    // Start is called before the first frame update
    void Start()
    {
        m_HealthManager = GetComponent<HealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_StomachValue > 0)
        {
            m_StomachValue -= m_HungerDeteriorationValue * Time.deltaTime;
        }
    }

    public void Eat(float foodValue)
    {
        if (m_StomachValue + foodValue > m_StomachCapacity)
        {
            m_StomachValue = m_StomachCapacity;
        }
        else
        {
            m_StomachValue += foodValue;
        }
    }
}
