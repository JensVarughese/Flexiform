using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_STANDALONE_WIN
// using AnotherFileBrowser.Windows;
#endif
using UnityEngine.Networking;
using System.IO;
using System.Text;
using Dummiesman;
using SFB;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FileExplorer: MonoBehaviour
{
    private RawImage rawImage;
    private Transform handModel;

    [System.NonSerialized]
    public GameObject model;
    public Material defaultMaterial;
    CameraController cameraController;
    public bool handLoaded = false;

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

        var extensions = new ExtensionFilter[] { new ExtensionFilter("Hand Model", "obj", "stl") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (paths.Length > 0)
            StartCoroutine(LoadFile(new Uri(paths[0]).AbsoluteUri));
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
            if (model != null)
                Destroy(model);

            if (path.Substring(path.Length - 4).Equals(".stl"))
            {
                 model = LoadStl(www.downloadHandler.data);
            }
            else
            {
                MemoryStream textStream = new MemoryStream(Encoding.UTF8.GetBytes(www.downloadHandler.text));
                model = new OBJLoader().Load(textStream);
            }

            handModel = model.transform.GetChild(0);
            handLoaded = true;
            model.name = "hand";
            model.transform.position = Vector3.zero;
            model.transform.localScale = new Vector3(1, 1, 1);
            handModel.GetComponent<MeshRenderer>().material = defaultMaterial;

            handModel.GetComponent<MeshRenderer>().material = defaultMaterial;

            cameraController = GameObject.Find("Main Camera").GetComponent<CameraController>();
            cameraController.LookAtHand(handModel);
        }
    }

    private GameObject LoadStl(byte[] data)
    {
        var hand = new GameObject();
        var model = new GameObject();
        model.transform.parent = hand.transform;
        model.AddComponent<MeshRenderer>();
        model.AddComponent<MeshFilter>();
        var mesh = model.GetComponent<MeshFilter>().mesh;
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        var vertices = new List<Vector3>();
        var triangleCountBinary = new byte[4];
        triangleCountBinary[0] = data[80];
        triangleCountBinary[1] = data[81];
        triangleCountBinary[2] = data[82];
        triangleCountBinary[3] = data[83];

        var triangleCount = BitConverter.ToUInt32(triangleCountBinary, 0);

        var bIndex = 84;
        for(int i = 0; i < triangleCount; i++)
        {
            var normal = Vector3.zero;
            var v1 = Vector3.zero;
            var v2 = Vector3.zero;
            var v3 = Vector3.zero;

            normal.x = ReadFloat(data, ref bIndex);
            normal.y = ReadFloat(data, ref bIndex);
            normal.z = ReadFloat(data, ref bIndex);

            v1.x = ReadFloat(data, ref bIndex);
            v1.y = ReadFloat(data, ref bIndex);
            v1.z = ReadFloat(data, ref bIndex);
            vertices.Add(v1);

            v2.x = ReadFloat(data, ref bIndex);
            v2.y = ReadFloat(data, ref bIndex);
            v2.z = ReadFloat(data, ref bIndex);
            vertices.Add(v2);

            v3.x = ReadFloat(data, ref bIndex);
            v3.y = ReadFloat(data, ref bIndex);
            v3.z = ReadFloat(data, ref bIndex);
            vertices.Add(v3);

            bIndex += 2;
        }

        var triangles = new int[vertices.Count];
        for(var i = 0; i < vertices.Count; i++)
        {
            triangles[i] = i;
        }
        var uvs = new Vector2[vertices.Count];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return hand;
    }

    private float ReadFloat(byte[] data, ref int index)
    {
        var result = BitConverter.ToSingle(new byte[] {
            data[index], data[index + 1], data[index + 2], data[index + 3]
        }, 0);

        index += 4;
        return result;
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
