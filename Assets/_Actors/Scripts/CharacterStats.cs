using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Base Character Info")]
    public string characterName;
    public int characterLevel;
    public int experiencePoints;
    public int gold;
    [Space]
    [Header("Base Stats")]
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    [Space]
    [Header("Health Stats")]
    public float maxHealth;
    public float health;
    public float healthRegenerationRate;
    [Space]
    [Header("Hunger Stats")]
    public float stomachDecayRate;
    public float stomachValue;
    public float stomachCapacity;
    [Space]
    [Header("Combat Stats")]
    public int attack;
    public int rangedAttack;
    public int magicAttack;
    public int defense;
    public float stamina;


    string m_SaveFilePath;
    public bool isLoaded = false; //True when this player has been initialized
    public int[] experienceThresholds;
    ActorEquipment actorEquipment;
    // Call this method to initialize your experience thresholds
    public void InitializeExperienceThresholds(int maxLevel)
    {
        experienceThresholds = new int[maxLevel + 1];
        for (int i = 1; i <= maxLevel; i++)
        {
            // This is a quadratic curve, adjust this formula as per your requirements
            experienceThresholds[i] = i * i * 100;
        }
    }
    public void Initialize(string _name)
    {
        actorEquipment = GetComponent<ActorEquipment>();
        InitializeExperienceThresholds(100);
        string saveDirectoryPath = Path.Combine(Application.persistentDataPath, "Characters");
        Directory.CreateDirectory(saveDirectoryPath);
        characterName = _name;
        m_SaveFilePath = saveDirectoryPath + "/" + characterName + "-stats.json";
        bool didLoad = LodeCharacterStats();
        GenerateStats();
        if (!didLoad) SaveCharacter();
        GetComponent<HealthManager>().SetStats();

        isLoaded = true;
    }

    public void GenerateStats()
    {
        CalculateLevel();
        maxHealth = GetMaxHealth();
        attack = GetAttack("melee");
        rangedAttack = GetAttack("ranged");
        magicAttack = GetAttack("magic");
        defense = GetDefense();
        healthRegenerationRate = GetHealthRegeneration();
        stomachDecayRate = GetHungerDecay();
        stomachCapacity = GetStomachCapacity();
        GetComponent<HungerManager>().SetStats();
    }
    public void CalculateLevel()
    {
        int level = 1;
        while (level < experienceThresholds.Length && experiencePoints >= experienceThresholds[level])
        {
            level++;
        }
        characterLevel = level;
        CalculateBaseStats();
    }

    public void CalculateBaseStats()
    {
        EquipmentStatBonus bonus = actorEquipment.GetStatBonus();
        strength = characterLevel + bonus.strBonus;
        dexterity = characterLevel + bonus.dexBonus;
        constitution = characterLevel + bonus.conBonus;
        intelligence = characterLevel + bonus.intBonus;
    }
    public bool LodeCharacterStats()
    {
        string json;
        try
        {
            json = File.ReadAllText(m_SaveFilePath);
        }
        catch
        {
            Debug.Log("~ New Character. No data to load");
            characterLevel = 1;
            experiencePoints = 0;
            strength = 1;
            dexterity = 1;
            constitution = 1;
            intelligence = 1;
            gold = 0;
            health = GetMaxHealth();
            stomachValue = 100;
            return false;
        }

        // Deserialize the data object from the JSON string
        CharacterStatsSaveData data = JsonConvert.DeserializeObject<CharacterStatsSaveData>(json);
        characterName = data.characterName;
        characterLevel = data.characterLevel;
        experiencePoints = data.experiencePoints;
        strength = data.strength;
        dexterity = data.dexterity;
        constitution = data.constitution;
        intelligence = data.intelligence;
        gold = data.gold;
        health = data.health;
        stomachValue = data.stomachValue;
        stamina = data.stamina;
        return true;
    }
    public void SaveCharacter()
    {

        if (isLoaded)
        {
            health = GetComponent<HealthManager>().health;
            stomachValue = GetComponent<HungerManager>().m_StomachValue;
            //stamina = ?
            CharacterStatsSaveData data = new CharacterStatsSaveData(characterName, characterLevel, experiencePoints, gold, strength, dexterity, constitution, intelligence, health, stomachValue, stamina);
            string json = JsonConvert.SerializeObject(data);
            // Open the file for writing
            using (FileStream stream = new FileStream(m_SaveFilePath, FileMode.Create))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                // Write the JSON string to the file
                writer.Write(json);
            }
        }
    }
    public float GetMaxHealth()
    {
        // Adjust the multiplier for constitution as per your balance needs.
        return 10 + constitution * 2.5f;
    }
    public float GetStomachCapacity()
    {
        // Adjust the multiplier for constitution as per your balance needs.
        return 100 + constitution * 10f;
    }
    public int GetAttack(string type)
    {
        switch (type)
        {
            case "melee":
                return strength;
            case "ranged":
                return dexterity;
            case "magic":
                return intelligence;
            default:
                // Debug.LogError("Invalid attack type!");
                return 0;
        }
    }

    public int GetDefense()
    {
        return dexterity;
    }

    public float GetHealthRegeneration()
    {
        // Assuming health regeneration is a rate per second, adjust as needed.
        return constitution / 10f;
    }

    public float GetHungerDecay()
    {
        // Assuming hunger decay is a rate per second, adjust as needed.
        return 0.35f / constitution;
    }

    public int GetStamina()
    {
        return 10 + dexterity / 2 + constitution / 2;
    }
}

public class CharacterStatsSaveData
{
    public string characterName;
    public int characterLevel;
    public int experiencePoints;
    public int gold;
    public int strength;
    public int dexterity;
    public int constitution;
    public int intelligence;
    public float health;
    public float stomachValue;
    public float stamina;

    public CharacterStatsSaveData(string characterName, int characterLevel, int experiencePoints, int gold, int strength, int dexterity, int constitution, int intelligence, float health, float stomachValue, float stamina)
    {
        this.characterName = characterName;
        this.characterLevel = characterLevel;
        this.experiencePoints = experiencePoints;
        this.gold = gold;
        this.strength = strength;
        this.dexterity = dexterity;
        this.constitution = constitution;
        this.intelligence = intelligence;
        this.health = health;
        this.stamina = stamina;
        this.stomachValue = stomachValue;
    }
}
public class EquipmentStatBonus
{
    public int dexBonus;
    public int strBonus;
    public int intBonus;
    public int conBonus;
    public EquipmentStatBonus(int dexBonus, int strBonus, int intBonus, int conBonus)
    {
        this.dexBonus = dexBonus;
        this.strBonus = strBonus;
        this.intBonus = intBonus;
        this.conBonus = conBonus;
    }
}
