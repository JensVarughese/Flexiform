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
using TMPro;
using System;
using SFB;

public class FileExplorer: MonoBehaviour
{
    private RawImage rawImage;
    private Transform handModel;
    public GameObject model;
    public Material defaultMaterial;
    CameraController cameraController;
    //public GameObject text;

    public void OpenFileBrowser()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "obj", false);
        if (paths.Length > 0)
        {
            StartCoroutine(LoadFile1(new System.Uri(paths[0]).AbsoluteUri));
        }
#if UNITY_STANDALONE_WIN
        //var bp = new BrowserProperties();
        //bp.filter = "Image files | *.obj";
        //bp.filterIndex = 0;

        //new FileBrowser().OpenFileBrowser(bp, path =>
        //{
        //    StartCoroutine(LoadFile1(path));
        //});
        
#endif
    }

    IEnumerator LoadImage(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path))
        {
            yield return uwr.SendWebRequest();

            if(uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var uwrTexture = DownloadHandlerTexture.GetContent(uwr);
                rawImage.texture = uwrTexture;
            }
        }
    }

    IEnumerator LoadFile1(string path)
    {
        //using (UnityWebRequest uwr = UnityWebRequest.Get(path))
        //{
        //    yield return uwr.SendWebRequest();

        //    if (uwr.result != UnityWebRequest.Result.Success)
        //    {
        //        Debug.Log(uwr.error);
        //    }
        //    else
        //    {
        //        var textStream = new MemoryStream(Encoding.UTF8.GetBytes(uwr.downloadHandler.text));
        //        var loadedObject = new OBJLoader().Load(textStream);
        //        loadedObject.name = "hand";
        //        loadedObject.transform.position = Vector3.zero;
        //        handModel = loadedObject.transform.GetChild(0);

        //        //verts[0] += new Vector3(0, 0.5f, 0);
        //        //mesh.vertices = verts;
        //        //mesh.RecalculateBounds();
        //    }

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
            {
                Destroy(model);
            }
            model = new OBJLoader().Load(textStream);
            model.name = "hand";
            model.transform.position = Vector3.zero;
            
            handModel = model.transform.GetChild(0);
            handModel.GetComponent<MeshRenderer>().material = defaultMaterial;
            cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
            cameraController.StartUp();
            //model.transform.localScale = new Vector3(-1, 1, 1); // set posiition of the parent model. Reverse X to show properly. THis may not be nessesary, the guy in the tutorial had it.
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
