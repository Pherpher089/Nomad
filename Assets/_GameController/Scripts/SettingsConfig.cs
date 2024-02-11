using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Settings/SettingsConfigObj")]
public class SettingsConfig : ScriptableObject
{
    public bool peaceful;
    public bool friendlyFire;
    public bool showOnScreenControls;
    public bool firstPlayerGamePad;
}
