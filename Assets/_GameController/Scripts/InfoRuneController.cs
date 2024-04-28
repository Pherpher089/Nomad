using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;
public class InfoRuneController : InteractionManager
{
    float selfCloseTime = 5;
    GameObject m_UiParent;
    TMP_Text m_TextMesh;
    Canvas m_Canvas;
    float counter;
    [TextArea] public string textContent;
    public Sprite infoImage;
    public bool fullScreenPrompt = false;
    // Start is called before the first frame update
    void Start()
    {
        m_UiParent = transform.GetChild(0).gameObject;
        int uiIndex = fullScreenPrompt ? 1 : 0;
        int offCanvasIndex = fullScreenPrompt ? 0 : 1;
        m_Canvas = m_UiParent.transform.GetChild(uiIndex).GetComponent<Canvas>();
        m_UiParent.transform.GetChild(offCanvasIndex).gameObject.SetActive(false);
        if (fullScreenPrompt)
        {
            m_TextMesh = m_Canvas.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            m_Canvas.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = infoImage;
        }
        else
        {
            m_TextMesh = m_Canvas.transform.GetChild(0).GetComponent<TMP_Text>();
        }
        m_TextMesh.text = textContent;
    }

    private void Update()
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
        Debug.Log("### " + this.gameObject.name);
        m_UiParent.SetActive(!m_UiParent.activeSelf);
        if (m_UiParent.activeSelf)
        {
            GameStateManager.Instance.activeInfoPrompts.Add(this);
        }
        else
        {
            if (GameStateManager.Instance.activeInfoPrompts.Contains(this))
            {
                GameStateManager.Instance.activeInfoPrompts.Remove(this);
            }
        }
        GetComponent<HoverSpinEffect>().enabled = !m_UiParent.activeSelf;
    }
    public bool ShowInfo(GameObject i)
    {
        m_UiParent.SetActive(!m_UiParent.activeSelf);
        if (m_UiParent.activeSelf)
        {
            GameStateManager.Instance.activeInfoPrompts.Add(this);
        }
        else
        {
            if (GameStateManager.Instance.activeInfoPrompts.Contains(this))
            {
                GameStateManager.Instance.activeInfoPrompts.Remove(this);
            }
        }
        GetComponent<HoverSpinEffect>().enabled = !m_UiParent.activeSelf;
        return true;
    }
}
