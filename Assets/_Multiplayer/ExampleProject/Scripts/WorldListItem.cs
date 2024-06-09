using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorldListItem : MonoBehaviour
{
    [SerializeField] public TMP_Text text;
    public void SetUp(string world)
    {
        text.text = world;
    }
    public void OnClick()
    {
        GetComponentInParent<WorldSelectControl>().SetWorld(text.text);
    }

    public void OnDelete()
    {
        MenuManager.Instance.DeleteWorld(text.text);
    }
}
