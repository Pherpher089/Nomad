using static TMPro.TextMeshPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class HUDControl : MonoBehaviourPunCallbacks
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
    List<GameObject> journalPages = new List<GameObject>();
    GameObject[] pageButtonPrompts;
    int currentPage = 0;
    public int currentTab = 0;
    Button previousButton;
    Button nextButton;
    public bool isPaused = false;
    GameStateManager gameController;
    bool initialized = false;
    GameObject toolBeltCursor;
    public bool quitting = false;
    public GameObject[] tabs;
    Vector3 scaleUp;

    void Awake()
    {
        previousButton = GameObject.Find("Prev Page").GetComponent<Button>();
        nextButton = GameObject.Find("Next Page").GetComponent<Button>();
        Transform tabParent = GameObject.Find("Tabs").transform;
        tabs = new GameObject[tabParent.childCount - 1];
        for (int i = 0; i < tabParent.childCount - 1; i++)
        {
            tabs[i] = tabParent.GetChild(i).gameObject;
        }
        scaleUp = new(1.1f, 1.1f, 1.1f);
        SetJournalTab(currentTab);
        pageButtonPrompts = new GameObject[5];
        pageButtonPrompts[0] = GameObject.Find("Next Page Control Prompt Keyboard");
        pageButtonPrompts[1] = GameObject.Find("Prev Page Control Prompt Keyboard");
        pageButtonPrompts[2] = GameObject.Find("Next Page Control Prompt Controller");
        pageButtonPrompts[3] = GameObject.Find("Prev Page Control Prompt Controller");
        pageButtonPrompts[4] = GameObject.Find("Tab Selection Control Prompt Controller");
    }

    private void SetJournalTab(int tab)
    {
        Transform journalParent = GameObject.Find("Journal").transform;
        foreach (GameObject page in journalPages)
        {
            page.SetActive(false);
        }
        journalPages = new List<GameObject>();
        for (int i = 0; i < journalParent.GetChild(tab).childCount; i++)
        {
            journalPages.Add(journalParent.GetChild(tab).GetChild(i).gameObject);
        }
        journalPages[tab].SetActive(true);
        tabs[currentTab].GetComponent<RectTransform>().localScale = Vector3.one;
        currentTab = tab;
        SetPage(0);
        tabs[currentTab].GetComponent<RectTransform>().localScale = scaleUp;
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

    public void OnChangeTab(int tab)
    {
        SetJournalTab(tab);
    }
    public void OnTabUp()
    {
        int newTab = GameObject.Find("Journal").transform.childCount - 2;
        if (currentTab > 0)
        {
            newTab = currentTab - 1;
        }
        SetJournalTab(newTab);
    }
    public void OnTabDown()
    {
        int newTab = 0;
        if (currentTab < GameObject.Find("Journal").transform.childCount - 2)
        {
            newTab = currentTab + 1;
        }
        SetJournalTab(newTab);
    }
    public void SetPage(int page)
    {
        for (int i = 0; i < journalPages.Count; i++)
        {
            if (i == page)
            {
                journalPages[i].SetActive(true);
            }
            else
            {
                journalPages[i].SetActive(false);

            }
            currentPage = 0;
            nextButton.interactable = true;
            previousButton.interactable = false;
        }
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

    public void TurnOfBossHealth()
    {
        bossHealthBarCanvasObject.SetActive(false);
    }

    public void UpdateOnScreenControls()
    {
        if (quitting) return;
        if (LevelPrep.Instance.firstPlayerGamePad)
        {
            pageButtonPrompts[0].SetActive(false);
            pageButtonPrompts[1].SetActive(false);
            pageButtonPrompts[2].SetActive(true);
            pageButtonPrompts[3].SetActive(true);
            pageButtonPrompts[4].SetActive(true);
        }
        else
        {
            pageButtonPrompts[0].SetActive(true);
            pageButtonPrompts[1].SetActive(true);
            pageButtonPrompts[2].SetActive(false);
            pageButtonPrompts[3].SetActive(false);
            pageButtonPrompts[4].SetActive(false);
        }
        if (PlayersManager.Instance.localPlayerList.Count > 0)
        {
            int newActivePanel = LevelPrep.Instance.firstPlayerGamePad ? 0 : 5;
            GameObject item = PlayersManager.Instance.localPlayerList[0].GetComponent<ActorEquipment>().equippedItem;
            if (!gameController.showOnScreenControls || PlayersManager.Instance.localPlayerList[0].GetComponent<ThirdPersonUserControl>().usingUI || GameStateManager.Instance.gameState == GameState.PauseState)
            {
                newActivePanel = -1;
            }
            else if (PlayersManager.Instance.localPlayerList[0].GetComponent<BuilderManager>().isBuilding)
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
        quitting = true;
        if (LevelManager.Instance != null) PhotonNetwork.Destroy(LevelManager.Instance.gameObject);
        if (RoomManager.Instance != null) PhotonNetwork.Destroy(RoomManager.Instance.gameObject);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
        StartCoroutine(WaitForDisconnectionAndLoadMainMenu());
    }

    private IEnumerator WaitForDisconnectionAndLoadMainMenu()
    {
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        if (LevelManager.Instance != null) Destroy(LevelManager.Instance.gameObject);
        if (RoomManager.Instance != null) Destroy(RoomManager.Instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Disconnect();
        LevelPrep.Instance.ResetLevelPrep();
    }

    public void InitSliders()
    {
        int activePlayer = PlayersManager.Instance.localPlayerList.Count;
        int offset = 0;
        for (int i = 0; i < hudParent.healthList.Count; i++)
        {
            if (i < PlayersManager.Instance.localPlayerList.Count && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
            {
                offset++;
                continue;
            }
            if (i < activePlayer)
            {
                hudParent.canvasList[i].enabled = true;
                hudParent.healthList[i].minValue = 0;
                hudParent.healthList[i].maxValue = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().maxHealth;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        offset = 0;
        for (int i = 0; i < hudParent.hungerList.Count; i++)
        {
            if (i < PlayersManager.Instance.localPlayerList.Count && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
            {
                offset++;
                continue;
            }
            if (i < activePlayer)
            {
                hudParent.hungerList[i].minValue = 0;
                hudParent.hungerList[i].maxValue = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().stomachCapacity;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        offset = 0;
        for (int i = 0; i < hudParent.experienceList.Count; i++)
        {
            if (i < PlayersManager.Instance.localPlayerList.Count && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
            {
                offset++;
                continue;
            }
            if (i < activePlayer)
            {
                SetExpSlider(i);
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
        offset = 0;
        for (int i = 0; i < hudParent.nameList.Count; i++)
        {
            if (i < PlayersManager.Instance.localPlayerList.Count && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
            {
                offset++;
                continue;
            }
            if (i < activePlayer)
            {
                hudParent.nameList[i].text = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().characterName;
            }
            else
            {
                hudParent.canvasList[i].enabled = false;
            }
        }
    }

    private void SetExpSlider(int i)
    {
        CharacterStats stats = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>();
        hudParent.experienceList[i].minValue = stats.experienceThresholds[stats.characterLevel - 1];
        hudParent.experienceList[i].maxValue = stats.experienceThresholds[stats.characterLevel];
        hudParent.levelList[i].text = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().characterLevel.ToString();
    }

    void LateUpdate()
    {
        if (quitting) return;
        if (PlayersManager.Instance)
        {
            int offset = 0;
            for (int i = 0; i < PlayersManager.Instance.localPlayerList.Count; i++)
            {
                if (i < PlayersManager.Instance.localPlayerList.Count && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
                {
                    offset++;
                    continue;
                }
                hudParent.healthList[i - offset].value = PlayersManager.Instance.localPlayerList[i].GetComponent<HealthManager>().health;
                hudParent.hungerList[i - offset].value = PlayersManager.Instance.localPlayerList[i].GetComponent<HungerManager>().stats.stomachValue;

                hudParent.experienceList[i - offset].value = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().experiencePoints;
                if (PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().experiencePoints >= hudParent.experienceList[i - offset].maxValue)
                {
                    PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().GenerateStats();
                    SetExpSlider(i - offset);
                }

                for (int j = 0; j < hudParent.toolBeltImages[i - offset].Count; j++)
                {
                    if (PlayersManager.Instance.localPlayerList[i].actorEquipment.inventoryManager.beltItems[j].isEmpty)
                    {
                        hudParent.toolBeltImages[i - offset][j].color = new Color(255, 255, 255, 0);
                    }
                    else
                    {
                        hudParent.toolBeltImages[i - offset][j].color = new Color(255, 255, 255, 1);
                        hudParent.toolBeltImages[i - offset][j].sprite = PlayersManager.Instance.localPlayerList[i].actorEquipment.inventoryManager.beltItems[j].item.icon;
                    }
                    hudParent.toolBeltItemCount[i - offset][j].text = PlayersManager.Instance.localPlayerList[i].actorEquipment.inventoryManager.beltItems[j].count.ToString();
                }
                if (PlayersManager.Instance.localPlayerList[i].actorEquipment.inventoryManager.selectedBeltItem == -1)
                {
                    hudParent.toolBeltCursors[i - offset].SetActive(false);
                }
                else
                {
                    hudParent.toolBeltCursors[i - offset].SetActive(true);
                    hudParent.toolBeltCursors[i - offset].transform.position = hudParent.toolBeltImages[i - offset][PlayersManager.Instance.localPlayerList[i].actorEquipment.inventoryManager.selectedBeltItem].gameObject.transform.position;
                }
            }
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
