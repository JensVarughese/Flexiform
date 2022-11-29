using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileImporter : MonoBehaviour
{
    public static void LoadObject(string fileName)
    {
        Instantiate(LoadObjectFromFile(fileName), Vector3.zero, Quaternion.identity);
    }

    private static Object LoadObjectFromFile(string fileName)
    {
        Debug.Log("Trying to load LevelPrefab from file (" + fileName + ")...");
        var loadedObject = Resources.Load(fileName);
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file found - please check the configuration");
        }
        return loadedObject;
    }
}
