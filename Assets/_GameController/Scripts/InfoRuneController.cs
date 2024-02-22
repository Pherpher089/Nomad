using System;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class InfoRuneController : InteractionManager
{
    float selfCloseTime = 5;
    GameObject m_UiParent;
    TMP_Text textMesh;
    float counter;
    [TextArea] public string textContent;
    // Start is called before the first frame update
    void Start()
    {
        m_UiParent = transform.GetChild(0).gameObject;
        textMesh = m_UiParent.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        textMesh.text = textContent;
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

    public bool ShowInfo(GameObject i)
    {
        m_UiParent.SetActive(!m_UiParent.activeSelf);
        if (m_UiParent.activeSelf)
        {
            GameStateManager.Instance.activeInfoPrompts.Add(this);
            Debug.Log("### adding");
        }
        else
        {
            if (GameStateManager.Instance.activeInfoPrompts.Contains(this))
            {
                Debug.Log("### removing");
                GameStateManager.Instance.activeInfoPrompts.Remove(this);
            }
        }
        GetComponent<HoverSpinEffect>().enabled = !m_UiParent.activeSelf;
        return true;
    }
}
