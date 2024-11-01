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
            stats.stomachValue = stats.stomachCapacity;
        }
        else
        {
            stats.stomachValue += foodValue;
        }

    }

}
