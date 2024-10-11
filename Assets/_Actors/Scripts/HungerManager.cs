using UnityEngine;

public class HungerManager : MonoBehaviour
{
    public CharacterStats stats;
    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<CharacterStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (stats.stomachValue > 0)
        {
            stats.stomachValue -= stats.stomachDecayRate * Time.deltaTime;
        }
    }

    public void Eat(float foodValue)
    {

        if (stats.stomachValue + foodValue > stats.stomachCapacity)
        {
            Debug.Log("### m_stomachCapacity" + stats.stomachCapacity);
            Debug.Log("### m_StomachValue" + stats.stomachValue);
            Debug.Log("### foodValue" + foodValue);
            stats.stomachValue = stats.stomachCapacity;
            Debug.Log("### m_StomachValue after" + stats.stomachValue);
        }
        else
        {
            stats.stomachValue += foodValue;
        }

    }

}
