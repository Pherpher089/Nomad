using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public float m_StomachCapacity;
    public float m_StomachValue;
    public float m_StomachDecayRate;

    HealthManager m_HealthManager;
    CharacterStats stats;
    // Start is called before the first frame update
    void Start()
    {
        m_HealthManager = GetComponent<HealthManager>();
        stats = GetComponent<CharacterStats>();
        SetStats();
    }

    public void SetStats()
    {
        m_StomachCapacity = stats.stomachCapacity;
        m_StomachValue = stats.stomachValue;
        m_StomachDecayRate = stats.stomachDecayRate;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_StomachValue > 0)
        {
            m_StomachValue -= m_StomachDecayRate * Time.deltaTime;
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
