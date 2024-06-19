using static TMPro.TextMeshPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Photon.Pun;

public class HUDControl : MonoBehaviour
{

    public GameObject pauseScreen;
    public GameObject failScreen;
    public GameObject loadingScreen;
    public GameObject bossHealthBarCanvasObject;
    public GameObject raidCounterCanvasObject;
    public GameObject[] controlsUi;
    Slider healthBarP1;
    Slider healthBarP2;
    Slider healthBarP3;
    Slider healthBarP4;
    Slider hungerBarP1;
    Slider hungerBarP2;
    Slider hungerBarP3;
    Slider hungerBarP4;
    Slider bossHealthSlider;
    public TMP_Text raidCounter;
    public HUDParent hudParent;
    List<GameObject> journalPages = new();
    GameObject[] pageButtonPrompts;
    int currentPage = 0;
    Button previousButton;
    Button nextButton;
    public bool isPaused = false;
    GameStateManager gameController;
    bool initialized = false;
    void Awake()
    {
        previousButton = GameObject.Find("Prev Page").GetComponent<Button>();
        nextButton = GameObject.Find("Next Page").GetComponent<Button>();
        Transform craftingRecipes = GameObject.Find("CraftingRecipes").transform;
        for (int i = 0; i < craftingRecipes.childCount; i++)
        {
            journalPages.Add(craftingRecipes.GetChild(i).gameObject);
        }
        pageButtonPrompts = new GameObject[4];
        Transform pageButtonPromptsParent = GameObject.Find("pageButtonPrompts").transform;
        for (int i = 0; i < pageButtonPromptsParent.childCount; i++)
        {
            pageButtonPrompts[i] = pageButtonPromptsParent.GetChild(i).gameObject;
        }
    }
    public void Initialize()
    {
        gameController = GetComponent<GameStateManager>();
        bossHealthBarCanvasObject = transform.GetChild(transform.childCount - 2).gameObject;
        raidCounterCanvasObject = transform.GetChild(transform.childCount - 1).gameObject;
        pauseScreen = GameObject.Find("Canvas_PauseScreen").transform.GetChild(0).gameObject;
        failScreen = GameObject.Find("Canvas_FailScreen").transform.GetChild(0).gameObject;
        loadingScreen = GameObject.Find("Canvas_LoadingScreen");
        healthBarP1 = GameObject.Find("HealthBar_P1").GetComponent<Slider>();
        healthBarP2 = GameObject.Find("HealthBar_P2").GetComponent<Slider>();
        healthBarP3 = GameObject.Find("HealthBar_P3").GetComponent<Slider>();
        healthBarP4 = GameObject.Find("HealthBar_P4").GetComponent<Slider>();
        hungerBarP1 = GameObject.Find("HungerBar_P1").GetComponent<Slider>();
        hungerBarP2 = GameObject.Find("HungerBar_P2").GetComponent<Slider>();
        hungerBarP3 = GameObject.Find("HungerBar_P3").GetComponent<Slider>();
        hungerBarP4 = GameObject.Find("HungerBar_P4").GetComponent<Slider>();
        hudParent = transform.GetComponentInChildren<HUDParent>();
        controlsUi = new GameObject[transform.childCount - 7];
        bossHealthSlider = transform.GetChild(transform.childCount - 2).GetChild(0).GetComponent<Slider>();
        bossHealthBarCanvasObject.SetActive(false);
        raidCounter = transform.GetChild(transform.childCount - 1).GetChild(0).GetComponent<TMP_Text>();
        raidCounterCanvasObject.SetActive(false);
        for (int i = 5; i < transform.childCount - 2; i++)
        {
            controlsUi[i - 5] = transform.GetChild(i).gameObject;
        }
        hudParent.InitializeBars();
        InitSliders();
        pauseScreen.SetActive(false);
        initialized = true;
    }

    public void OnNextPage()
    {
        if (currentPage >= journalPages.Count - 1)
        {
            nextButton.interactable = false;
            return;
        }
        else
        {
            currentPage++;
            journalPages[currentPage - 1].SetActive(false);
            journalPages[currentPage].SetActive(true);
            if (currentPage == journalPages.Count - 1)
            {
                nextButton.interactable = false;
            }
            else
            {
                nextButton.interactable = true;
            }
            if (currentPage == 0)
            {
                previousButton.interactable = false;
            }
            else
            {
                previousButton.interactable = true;

            }
        }
    }
    public void OnPrevPage()
    {
        if (currentPage <= 0)
        {
            previousButton.interactable = false;
            return;
        }
        else
        {
            currentPage--;
            journalPages[currentPage + 1].SetActive(false);
            journalPages[currentPage].SetActive(true);
            if (currentPage == 0)
            {
                previousButton.interactable = false;
            }
            else
            {
                previousButton.interactable = true;

            }
            if (currentPage == journalPages.Count - 1)
            {
                nextButton.interactable = false;
            }
            else
            {
                nextButton.interactable = true;
            }
        }
    }
    public void InitializeBossHealthBar(BossManager bossManager)
    {
        if (!initialized) return;
        bossHealthBarCanvasObject.SetActive(true);
        bossHealthBarCanvasObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = bossManager.gameObject.name;
        bossHealthSlider.value = bossManager.GetComponent<HealthManager>().health;
        bossHealthSlider.maxValue = bossManager.GetComponent<HealthManager>().maxHealth;
    }

    public void UpdateOnScreenControls()
    {
        if (LevelPrep.Instance.firstPlayerGamePad)
        {
            pageButtonPrompts[0].SetActive(false);
            pageButtonPrompts[1].SetActive(false);
            pageButtonPrompts[2].SetActive(true);
            pageButtonPrompts[3].SetActive(true);

        }
        else
        {
            pageButtonPrompts[0].SetActive(true);
            pageButtonPrompts[1].SetActive(true);
            pageButtonPrompts[2].SetActive(false);
            pageButtonPrompts[3].SetActive(false);
        }
        if (PlayersManager.Instance.playerList.Count > 0)
        {
            int newActivePanel = LevelPrep.Instance.firstPlayerGamePad ? 0 : 5;
            GameObject item = PlayersManager.Instance.playerList[0].GetComponent<ActorEquipment>().equippedItem;
            if (!gameController.showOnScreenControls || PlayersManager.Instance.playerList[0].GetComponent<ThirdPersonUserControl>().usingUI || GameStateManager.Instance.gameState == GameState.PauseState)
            {
                newActivePanel = -1;
            }
            else if (PlayersManager.Instance.playerList[0].GetComponent<BuilderManager>().isBuilding)
            {
                newActivePanel += 4;
            }
            else if (item != null)
            {
                if (item.GetComponent<Item>().gameObject.CompareTag("Tool") && item.GetComponent<Item>().itemName == "Torch")
                {
                    newActivePanel++;
                }
                else if (item.GetComponent<BuildingMaterial>() != null)
                {
                    newActivePanel += 2;
                }
                else if (!item.GetComponent<Item>().gameObject.CompareTag("Tool"))
                {
                    newActivePanel += 3;
                }
            }

            for (int i = 0; i < controlsUi.Length; i++)
            {
                if (i == newActivePanel)
                {
                    controlsUi[i].SetActive(true);
                }
                else
                {
                    controlsUi[i].SetActive(false);
                }
            }
        }
    }

    public void EnablePauseScreen(bool _enabled)
    {
        isPaused = _enabled;
        pauseScreen.SetActive(_enabled);
        GameStateManager.Instance.UpdateSettingsValues();
        if (_enabled)
        {
            gameController.gameState = GameState.PauseState;
        }
        else
        {
            gameController.gameState = GameState.PlayState;
        }
    }

    public void EnableFailScreen(bool _enabled)
    {
        failScreen.SetActive(_enabled);
    }

    public void OnRetry()
    {
        //This UI is no longer needed
        //FindObjectOfType<GameStateManager>().RespawnParty();
    }

    public void OnContinue()
    {
        EnablePauseScreen(false);
    }

    public void OnQuit()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }
    public void InitSliders()
    {
        int activePlayer = PlayersManager.Instance.playerList.Count;
        for (int i = 0; i < hudParent.healthList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.canvasList[i].enabled = true;
                hudParent.healthList[i].minValue = 0;
                hudParent.healthList[i].maxValue = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().maxHealth;

            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.hungerList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.hungerList[i].minValue = 0;
                hudParent.hungerList[i].maxValue = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().stomachCapacity;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.experienceList.Count; i++)
        {
            if (i < activePlayer)
            {
                SetExpSlider(i);
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        for (int i = 0; i < hudParent.nameList.Count; i++)
        {
            if (i < activePlayer)
            {
                hudParent.nameList[i].text = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().characterName;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
    }

    private void SetExpSlider(int i)
    {
        CharacterStats stats = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>();
        hudParent.experienceList[i].minValue = stats.experienceThresholds[stats.characterLevel - 1];
        hudParent.experienceList[i].maxValue = stats.experienceThresholds[stats.characterLevel];
        hudParent.levelList[i].text = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().characterLevel.ToString();
    }

    void LateUpdate()
    {
        if (PlayersManager.Instance)
        {
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.healthList[i].value = PlayersManager.Instance.playerList[i].GetComponent<HealthManager>().health;
            }
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.hungerList[i].value = PlayersManager.Instance.playerList[i].GetComponent<HungerManager>().m_StomachValue;
            }
            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                hudParent.experienceList[i].value = PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().experiencePoints;
                if (PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().experiencePoints >= hudParent.experienceList[i].maxValue)
                {
                    PlayersManager.Instance.playerList[i].GetComponent<CharacterStats>().GenerateStats();
                    SetExpSlider(i);
                }
            }

            for (int i = 0; i < PlayersManager.Instance.playerList.Count; i++)
            {
                for (int j = 0; j < hudParent.toolBeltImages[i].Count; j++)
                {
                    if (PlayersManager.Instance.playerList[i].actorEquipment.inventoryManager.items[j].isEmpty)
                    {
                        hudParent.toolBeltImages[i][j].color = new Color(255, 255, 255, 0);
                    }
                    else
                    {
                        hudParent.toolBeltImages[i][j].color = new Color(255, 255, 255, 1);

                        hudParent.toolBeltImages[i][j].sprite = PlayersManager.Instance.playerList[i].actorEquipment.inventoryManager.items[j].item.icon;
                    }
                    hudParent.toolBeltItemCount[i][j].text = PlayersManager.Instance.playerList[i].actorEquipment.inventoryManager.items[j].count.ToString();
                }
            }
        }
    }
}

