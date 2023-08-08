using System.IO;
using UnityEngine;

public class DevKeyManager : MonoBehaviour
{
    private string persistentPath;
    private string charactersPath;
    private string levelsPath;

    private void Start()
    {
        persistentPath = Application.persistentDataPath;
        charactersPath = Path.Combine(persistentPath, "characters");
        levelsPath = Path.Combine(persistentPath, "levels");
    }

    private void Update()
    {
        // Dev key for deleting character stats files
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DeleteCharacterStatsFiles(charactersPath);
        }

        // Dev key for deleting character inventory files
        if (Input.GetKeyDown(KeyCode.F2))
        {
            DeleteCharacterInventoryFiles(charactersPath);
        }

        // Dev key for deleting level directories
        if (Input.GetKeyDown(KeyCode.F3))
        {
            DeleteLevelDirectories(levelsPath);
        }
    }

    private void DeleteCharacterStatsFiles(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] statsFiles = Directory.GetFiles(dirPath, "*-stats.*");
            foreach (string file in statsFiles)
            {
                DeleteFile(file);
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteCharacterInventoryFiles(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] allFiles = Directory.GetFiles(dirPath);
            foreach (string file in allFiles)
            {
                if (!file.EndsWith("-stats"))
                {
                    DeleteFile(file);
                }
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteLevelDirectories(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            string[] directories = Directory.GetDirectories(dirPath);
            foreach (string directory in directories)
            {
                DeleteDirectory(directory);
            }
        }
        else
        {
            Debug.LogWarning("Directory does not exist: " + dirPath);
        }
    }

    private void DeleteFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
            Debug.Log("Deleted file: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting file: " + filePath + ". Error: " + e.Message);
        }
    }

    private void DeleteDirectory(string dirPath)
    {
        try
        {
            Directory.Delete(dirPath, true);  // The 'true' parameter allows for recursive deletion
            Debug.Log("Deleted directory: " + dirPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error deleting directory: " + dirPath + ". Error: " + e.Message);
        }
    }
}
