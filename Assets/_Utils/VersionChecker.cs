using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class VersionChecker : MonoBehaviour
{
    // Start is called before the first frame update
    private string versionApiUrl = "https://realmwalker-server-93253e10de19.herokuapp.com/api/version";
    private string currentGameVersion; // Your game's current version.
    GameObject canvas;
    TMP_Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        canvas = transform.GetChild(0).gameObject;
        uiText = canvas.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        canvas.SetActive(false);
        currentGameVersion = Application.version;
        StartCoroutine(CheckVersion());
    }

    IEnumerator CheckVersion()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(versionApiUrl))
        {
            // Send the request and wait for a response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError(webRequest.error);
            }
            else
            {
                Debug.Log("### " + webRequest.downloadHandler.text);
                VersionData versionData = JsonUtility.FromJson<VersionData>(webRequest.downloadHandler.text);
                // Compare versions
                if (currentGameVersion != versionData.version)
                {
                    canvas.SetActive(true);
                    uiText.text = $"Version {currentGameVersion} is no longer playable";
                }
                else
                {
                    // If versions match, load main menu
                    SceneManager.LoadScene(1);
                }
            }
        }
    }
}
public class VersionData
{
    public int id;
    public string version;
}