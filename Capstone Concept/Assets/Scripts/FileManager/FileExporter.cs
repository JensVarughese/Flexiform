using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public class FileExporter : MonoBehaviour
{
    public GameObject modelInner;
    public GameObject modelOuter;
    private Mesh modelCombined;
    private Material material;
    //public GameObject modelFinal;
    //public Mesh modelCombined;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName,string methodName, string filename, byte[] byteArray, int byteArraySize);

    public void onClickSave()
    {
       modelInner = GameObject.FindGameObjectWithTag("SliceableInner");
        modelOuter = GameObject.FindGameObjectWithTag("SliceableOuter");

        var meshInner = modelInner.GetComponentsInChildren<MeshFilter>()[0].mesh;
        var meshOuter = modelOuter.GetComponentsInChildren<MeshFilter>()[0].mesh;


        var bytes = Encoding.UTF8.GetBytes(MeshToStr(meshInner.vertices, meshInner.triangles, meshOuter.vertices, meshOuter.triangles, meshInner.vertices.Length,  "\n"));  
        DownloadFile(gameObject.name,"OnFileDownload", "model.obj", bytes, bytes.Length);
    }

    // Called from browser
    public void OnFileDownload() {}

#else

    public void onClickSave()
    {
        modelInner = GameObject.FindGameObjectWithTag("SliceableInner");
        modelOuter = GameObject.FindGameObjectWithTag("SliceableOuter");

        var meshInner = modelInner.GetComponentsInChildren<MeshFilter>()[0].mesh;
        var meshOuter = modelOuter.GetComponentsInChildren<MeshFilter>()[0].mesh;

        //var TrianglesOuter = new int[meshOuter.triangles.Length];

        // adding the triangles
        //for (int i = 0; i < meshOuter.triangles.Length; i++)
        //{
        //    TrianglesOuter[i] = meshOuter.triangles[i] + meshInner.vertices.Length;
        //}
        //meshOuter.triangles = TrianglesOuter;

        string path = StandaloneFileBrowser.SaveFilePanel("save file", "", "model", "obj");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, MeshToStr(meshInner.vertices, meshInner.triangles, meshOuter.vertices, meshOuter.triangles, meshInner.vertices.Length, "\n"));
            //ModelExporter.ExportObjects(path, modelInner);
            Debug.Log(path);
        }

    }

#endif

    public void ReadModels()
    {
        //modelCombined = new Mesh();
        //modelInner = GameObject.FindGameObjectWithTag("SliceableInner");
        //modelOuter = GameObject.FindGameObjectWithTag("SliceableOuter");

        //var meshInner = modelInner.GetComponentsInChildren<MeshFilter>()[0].mesh;
        //var meshOuter = modelOuter.GetComponentsInChildren<MeshFilter>()[0].mesh;

        //var normalsInner = meshInner.normals;
        //var VertsInner = meshInner.vertices;
        //var TrianglesInner = meshInner.triangles;
        //var TrianglesOuter = new int[meshOuter.triangles.Length];

        //// adding the triangles
        //for (int i = 0; i < meshOuter.triangles.Length; i++)
        //{
        //    TrianglesOuter[i] = meshOuter.triangles[i] + meshInner.vertices.Length;
        //}


        //modelFinal.
        //modelCombined.vertices = VertsInner.Concat(meshOuter.vertices).ToArray();
        //modelCombined.normals = normalsInner.Concat(meshOuter.normals).ToArray();
        //modelCombined.triangles = TrianglesInner.Concat(TrianglesOuter).ToArray();
        //material = modelInner.GetComponentsInChildren<MeshRenderer>()[0].material;
    }


    public string V3ArrToStr(Vector3 [] v3Arr, string splitStr)
    {
        StringBuilder sb = new StringBuilder();
        foreach(Vector3 v3 in v3Arr)
        {
            sb.Append(splitStr).Append(v3.x + " " + v3.y + " " + v3.z + "\n");//.Append(" ").Append(v3.y).Append(" ").Append(v3.z).Append("\n");
        }
        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1); // remove last new line
        }
        return sb.ToString();
    }

    public string intArrToStr(int[] intArr, string splitStr, int length)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < intArr.Length; i+=3)
        {
            sb.Append(splitStr).Append((intArr[i] + 1+length) + " " + (intArr[i + 1] + 1 + length) + " " + (intArr[i + 2] + 1 + length) + "\n");//.Append("// ").Append(intArr[i+1]+1).Append("// ").Append(intArr[i+2]+1).Append("//").Append("\n");
        }
        if(sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1); // remove the last " "
        }
        return sb.ToString();
    }

    public string ColourToStr (Color colour)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(colour.r).Append(" ").Append(colour.g).Append(" ").Append(colour.b).Append(" ").Append(colour.a);
        return sb.ToString();
    }
    


    //public string ModelToStr(ref GameObject modelInner, string splitStr)
    //{
    //    StringBuilder sb = new StringBuilder();
    //    sb.Append("# Binary STL generated by MSoft\n");
    //    sb.Append("o edited mesh\n");
    //    for (int i =0; i <modelInner.GetComponentsInChildren<Renderer>().Length; i++)
    //    {
    //        Mesh mesh = modelInner.GetComponentsInChildren<MeshFilter>()[i].mesh;
    //        Material material = modelInner.GetComponentsInChildren<MeshRenderer>()[i].material;
    //        sb.Append(MeshToStr(mesh, material.color, "\n")).Append(splitStr);
    //    }
    //    if (sb.Length > 0)
    //    {
    //        sb.Remove(sb.Length - 1, 1);
    //    }
    //    return sb.ToString();
    //}

    public string MeshToStr(Vector3[] verticesInner, int[] trianglesInner, Vector3[] verticesOuter, int[] trianglesOuter, int innerLength, string splitStr)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(V3ArrToStr(verticesInner, "v ")).Append(splitStr);//.Append(splitStr);//.Append(intArrToStr(mesh.triangles)).Append(splitStr)
        //sb.Append("s 1").Append(splitStr);
        //Debug.Log(mesh.GetTopology(0));
        sb.Append(V3ArrToStr(verticesOuter, "v ")).Append(splitStr);

        sb.Append(intArrToStr(trianglesInner, "f ", 0)).Append(splitStr);
        sb.Append(intArrToStr(trianglesOuter, "f ", innerLength)).Append(splitStr);
        //for (int i = 0; i < mesh.subMeshCount; i++) { 
        //    sb.Append(intArrToStr(mesh.GetIndices(i), "f "));
        //}

        //sb.Append(splitStr);//.Append(mesh.GetTopology(0)).Append(splitStr).Append(ColourToStr(meshColour));
        return sb.ToString();
    }

}
