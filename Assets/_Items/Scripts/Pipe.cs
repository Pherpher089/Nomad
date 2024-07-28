using UnityEngine;

public class Pipe : Item
{
    public int m_PipeIndex;
    ParticleSystem m_SmokeEffect;
    HealthManager m_UserHealthManager;
    public bool m_UseHealthRegen;
    public int m_HelthRegenInterval = 5;
    public int m_HealthRegenValue = 1;
    float m_TimeCounter = 0;
    void Awake()
    {
        m_SmokeEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        m_SmokeEffect.Stop();
    }
    public void Update()
    {
        if (m_UseHealthRegen && isEquipped && m_UserHealthManager != null)
        {
            m_TimeCounter += Time.deltaTime;
            if (m_TimeCounter > m_HelthRegenInterval)
            {
                m_UserHealthManager.Heal(m_HealthRegenValue, m_UserHealthManager.gameObject);
                m_TimeCounter = 0;
            }
        }
    }

    public override void OnEquipped(GameObject character)
    {
        base.OnEquipped(character);
        m_UserHealthManager = character.GetComponent<HealthManager>();
        m_SmokeEffect.Play();
    }

    public override void OnUnequipped()
    {
        base.OnUnequipped();
        m_UserHealthManager = null;
        m_SmokeEffect.Stop();

    }
}