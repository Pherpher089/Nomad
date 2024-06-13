using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoRuneController : InteractionManager
{
    [TextArea] public string[] textContent;
    public Sprite[] infoImage;
    public bool fullScreenPrompt = true;
    public bool isOpen = false;
    GameObject m_UiParent;
    TMP_Text m_TextMesh;
    Canvas m_Canvas;
    Button nextButton, prevButton;
    TMP_Text m_PageText;
    float selfCloseTime = 5;
    float counter;
    public string runeId;
    int currentPage = 0;
    // Start is called before the first frame update
    void Start()
    {
        runeId = runeId.ToString(); // Ensure runeId is treated as a string
        List<Dictionary<string, object>> rows = CSVReader.Read("InfoRunes");
        List<Dictionary<string, object>> relatedRows = new();

        for (int i = 0; i < rows.Count; i++)
        {
            string rowRuneNumber = rows[i]["RuneNumber"].ToString();
            if (rowRuneNumber == runeId)
            {
                relatedRows.Add(rows[i]);
            }
        }
        Transform screenSpacePromptTransform = transform.GetChild(0).GetChild(1).GetChild(0);
        m_UiParent = transform.GetChild(0).gameObject;
        m_PageText = screenSpacePromptTransform.GetChild(screenSpacePromptTransform.childCount - 1).GetComponent<TMP_Text>();
        int uiIndex = fullScreenPrompt ? 1 : 0;
        int offCanvasIndex = fullScreenPrompt ? 0 : 1;
        m_Canvas = m_UiParent.transform.GetChild(uiIndex).GetComponent<Canvas>();
        m_UiParent.transform.GetChild(offCanvasIndex).gameObject.SetActive(false);
        if (relatedRows.Count > 0)
        {
            textContent = new string[relatedRows.Count];
            infoImage = new Sprite[relatedRows.Count];
            fullScreenPrompt = true;
            int i = 0;
            foreach (Dictionary<string, object> row in relatedRows)
            {
                textContent[i] = row["DisplayText"].ToString();

                string imageName = row["Image"].ToString();
                infoImage[i] = LoadSprite("InfoRuneImages/" + imageName);
                Debug.Log("### image " + infoImage[i]);
                i++;
            }
        }

        if (fullScreenPrompt)
        {
            m_TextMesh = m_Canvas.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            if (infoImage.Length > 0) m_Canvas.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = infoImage[currentPage];
        }
        else
        {
            m_TextMesh = m_Canvas.transform.GetChild(0).GetComponent<TMP_Text>();
        }
        if (textContent.Length > 0) m_TextMesh.text = textContent[currentPage];
        if (fullScreenPrompt) m_PageText.text = $"{currentPage + 1}/{textContent.Length}";
    }

    private void Update()
    {
        AutoClose();
    }

    private void AutoClose()
    {
        if (!fullScreenPrompt)
        {
            if (m_UiParent.activeSelf)
            {
                counter += Time.deltaTime;
            }
            else
            {
                counter = 0;
            }
            if (counter > selfCloseTime)
            {
                ShowInfo(this.gameObject);
            }
        }
    }

    public void OnEnable()
    {
        OnInteract += ShowInfo;
    }

    public void OnDisable()
    {
        OnInteract -= ShowInfo;
    }

    public void ShowInfo()
    {
        m_UiParent.SetActive(!m_UiParent.activeSelf);
        if (m_UiParent.activeSelf)
        {
            isOpen = true;

            GameStateManager.Instance.activeInfoPrompts.Add(this);
        }
        else
        {
            isOpen = false;

            ThirdPersonUserControl[] allUsers = FindObjectsOfType<ThirdPersonUserControl>();
            foreach (ThirdPersonUserControl user in allUsers)
            {
                user.infoPromptUI = false;
            }
            if (GameStateManager.Instance.activeInfoPrompts.Contains(this))
            {
                GameStateManager.Instance.activeInfoPrompts.Remove(this);
            }
        }
        GetComponent<HoverSpinEffect>().enabled = !m_UiParent.activeSelf;
    }

    public bool ShowInfo(GameObject i)
    {
        Debug.Log("### game object " + i.name);

        m_UiParent.SetActive(!m_UiParent.activeSelf);
        if (m_UiParent.activeSelf)
        {
            isOpen = true;
            if (i.TryGetComponent<ThirdPersonUserControl>(out var thirdPersonUserControl))
            {
                thirdPersonUserControl.infoPromptUI = true;
            }
            GameStateManager.Instance.activeInfoPrompts.Add(this);
        }
        else
        {
            isOpen = false;
            ThirdPersonUserControl[] allUsers = FindObjectsOfType<ThirdPersonUserControl>();
            foreach (ThirdPersonUserControl user in allUsers)
            {
                user.infoPromptUI = false;
            }
            if (GameStateManager.Instance.activeInfoPrompts.Contains(this))
            {
                GameStateManager.Instance.activeInfoPrompts.Remove(this);
            }
        }
        GetComponent<HoverSpinEffect>().enabled = !m_UiParent.activeSelf;
        return true;
    }
    public void OnNextPage()
    {
        if (currentPage + 1 < textContent.Length)
        {
            currentPage++;
        }
        else
        {
            currentPage = 0;
        }
        if (fullScreenPrompt)
        {
            if (infoImage.Length > 0) m_Canvas.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = infoImage[currentPage];
        }
        if (fullScreenPrompt) m_PageText.text = $"{currentPage + 1}/{textContent.Length}";
        m_TextMesh.text = textContent[currentPage];
    }
    public void OnPrevPage()
    {
        if (currentPage - 1 >= 0)
        {
            currentPage--;
        }
        else
        {
            currentPage = textContent.Length - 1;
        }
        if (fullScreenPrompt)
        {
            if (infoImage.Length > 0) m_Canvas.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = infoImage[currentPage];
        }
        if (fullScreenPrompt) m_PageText.text = $"{currentPage + 1}/{textContent.Length}";
        m_TextMesh.text = textContent[currentPage];
    }

    private Sprite LoadSprite(string imageName)
    {
        return Resources.Load<Sprite>(imageName);
    }
}
