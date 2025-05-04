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
    public GameObject miniMapObject;
    public GameObject[] controlsUi;
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
    public bool quitting = false;
    public GameObject[] tabs;
    Vector3 scaleUp;
    PhotonView pv;
    void Awake()
    {
        previousButton = GameObject.Find("Prev Page").GetComponent<Button>();
        nextButton = GameObject.Find("Next Page").GetComponent<Button>();
        Transform tabParent = GameObject.Find("Tabs").transform;
        pv = GetComponent<PhotonView>();
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

        journalPages[0].SetActive(true);
        tabs[currentTab].GetComponent<RectTransform>().localScale = Vector3.one;
        currentTab = tab;
        SetPage(0);
        tabs[currentTab].GetComponent<RectTransform>().localScale = scaleUp;
    }

    public void Initialize()
    {
        gameController = GetComponent<GameStateManager>();
        miniMapObject = transform.GetChild(transform.childCount - 1).gameObject;
        bossHealthBarCanvasObject = transform.GetChild(transform.childCount - 2).gameObject;
        raidCounterCanvasObject = transform.GetChild(transform.childCount - 3).gameObject;
        pauseScreen = GameObject.Find("Canvas_PauseScreen").transform.GetChild(0).gameObject;
        failScreen = GameObject.Find("Canvas_FailScreen").transform.GetChild(0).gameObject;
        loadingScreen = GameObject.Find("Canvas_LoadingScreen");
        hudParent = transform.GetComponentInChildren<HUDParent>();
        bossHealthSlider = transform.GetChild(transform.childCount - 3).GetChild(0).GetComponent<Slider>();
        bossHealthBarCanvasObject.SetActive(false);
        raidCounter = transform.GetChild(transform.childCount - 2).GetChild(0).GetComponent<TMP_Text>();
        raidCounterCanvasObject.SetActive(false);
        hudParent.InitializeBars();
        InitSliders();
        pauseScreen.SetActive(false);
        initialized = true;
    }

    public void OnChangeTab(int tab)
    {
        Debug.Log("Tab: " + tab);
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
    }

    public void EnablePauseScreen(bool _enabled)
    {
        isPaused = _enabled;
        pauseScreen.SetActive(_enabled);
        GameStateManager.Instance.UpdateSettingsValues();
        if (_enabled)
        {
            UpdateOnScreenControls();
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
        GameStateManager.Instance.OnQuit();
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
                hudParent.healthRatioSlider[i].minValue = 0;
                hudParent.healthRatioSlider[i].maxValue = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().maxHealth;
                hudParent.healthRatioText[i].text = PlayersManager.Instance.localPlayerList[i].GetComponent<HealthManager>().health.ToString("F0");
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
                hudParent.hungerRatioSlider[i].minValue = 0;
                hudParent.hungerRatioSlider[i].maxValue = PlayersManager.Instance.localPlayerList[i].GetComponent<CharacterStats>().stomachCapacity;
                hudParent.hungerRatioText[i].text = PlayersManager.Instance.localPlayerList[i].GetComponent<HungerManager>().stats.stomachValue.ToString("F0");
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
                hudParent.backgrounds[(int)PlayersManager.Instance.localPlayerList[i].GetComponent<ThirdPersonUserControl>().playerColorIndex].SetActive(true);
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
        if (!initialized) return;
        if (quitting) return;
        if (GameStateManager.Instance.isTeleporting) return;
        if (PlayersManager.Instance)
        {
            int offset = 0;
            for (int i = 0; i < PlayersManager.Instance.localPlayerList.Count; i++)
            {
                if (i < PlayersManager.Instance.localPlayerList.Count && PlayersManager.Instance.localPlayerList[i] != null && !PlayersManager.Instance.localPlayerList[i].GetComponent<PhotonView>().IsMine)
                {

                    offset++;
                    continue;
                }
                //This should trigger when a player leaves
                if (i - offset < 4 && i - offset >= 0)
                {
                    HealthManager health = PlayersManager.Instance.localPlayerList[i].GetComponent<HealthManager>();
                    hudParent.healthList[i - offset].value = health.health;
                    hudParent.healthRatioSlider[i - offset].value = health.health;
                    hudParent.healthRatioText[i - offset].text = health.health.ToString("F0"); ;

                    HungerManager hunger = PlayersManager.Instance.localPlayerList[i].GetComponent<HungerManager>();
                    hudParent.hungerList[i - offset].value = hunger.stats.stomachValue;
                    hudParent.hungerRatioSlider[i - offset].value = hunger.stats.stomachValue;
                    hudParent.hungerRatioText[i - offset].text = hunger.stats.stomachValue.ToString("F0");

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
