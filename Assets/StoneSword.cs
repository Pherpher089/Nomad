using UnityEngine;

public class StoneSword : Tool
{
    // Start is called before the first frame update
    void Start()
    {
        icon = Resources.Load<Sprite>("Sprites/CutlassIcon");
        name = "Stone Sword";
    }
}
