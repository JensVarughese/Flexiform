using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_STANDALONE_WIN
using AnotherFileBrowser.Windows;
#endif
using UnityEngine.Networking;
using System.IO;
using System.Text;
using Dummiesman;
using SFB;
using System.Runtime.InteropServices;

public class FileExplorer: MonoBehaviour
{
    private RawImage rawImage;
    private Transform handModel;

    [System.NonSerialized]
    public GameObject model;
    public Material defaultMaterial;
    CameraController cameraController;
    bool handLoaded = false;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OpenFileBrowser()
    {
        if(handLoaded)
            return;
        
        UploadFile(gameObject.name, "OnFileUpload", ".obj", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(LoadFile(url));
    }

#else
    public void OpenFileBrowser()
    {
        if(handLoaded)
            return;

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "obj", false);
        if (paths.Length > 0)
            StartCoroutine(LoadFile(new System.Uri(paths[0]).AbsoluteUri));
    }
#endif

    IEnumerator LoadFile(string path)
    {
        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW ERROR: " + www.error);
        }
        else
        {
            //textMeshPro.text = www.downloadHandler.text;
            MemoryStream textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.downloadHandler.text));
            if (model != null)
                Destroy(model);

            model = new OBJLoader().Load(textStream);
            handLoaded = true;
            model.name = "hand";
            model.transform.position = Vector3.zero;
            model.transform.localScale = new Vector3(1, 1, 1);
            
            handModel = model.transform.GetChild(0);
            handModel.GetComponent<MeshRenderer>().material = defaultMaterial;
            cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
            cameraController.StartUp();
        }
    }

    public void EditVertex(Slider slider)
    {
        var mesh = handModel.GetComponent<MeshFilter>().mesh;
        var verts = mesh.vertices;
        verts[4].y = slider.value;
        mesh.vertices = verts;
        mesh.RecalculateBounds();
    }
}
