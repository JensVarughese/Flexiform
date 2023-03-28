using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SFB;
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Linq;

public enum ExportType {Obj, Stl}
public class FileExporter : MonoBehaviour
{
    public GameObject modelInner;
    public GameObject modelOuter;
    private Mesh modelCombined;
    private Material material;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL
    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName,string methodName, string filename, byte[] byteArray, int byteArraySize);

    public void onClickSave(ExportType et)
    {
       modelInner = GameObject.FindGameObjectWithTag("Sliceable");
        modelOuter = GameObject.FindGameObjectWithTag("SliceableOuter");

        var meshInner = modelInner.GetComponentsInChildren<MeshFilter>()[0].mesh;
        var meshOuter = modelOuter.GetComponentsInChildren<MeshFilter>()[0].mesh;

        if(et == ExportType.Obj) {
            var bytes = Encoding.UTF8.GetBytes(MeshToStrObj(meshInner.vertices, meshInner.triangles, meshOuter.vertices, meshOuter.triangles, meshInner.vertices.Length,  "\n"));  
            DownloadFile(gameObject.name,"OnFileDownload", "model.obj", bytes, bytes.Length);
        }
        else {
            var bytes = Encoding.UTF8.GetBytes(MeshtoStrStl(meshInner, meshOuter));  
            DownloadFile(gameObject.name,"OnFileDownload", "model.stl", bytes, bytes.Length);
        }
    }

    // Called from browser
    public void OnFileDownload() {}

#else

    public void onClickSave(ExportType et)
    {
        modelInner = GameObject.FindGameObjectWithTag("Sliceable");
        modelOuter = GameObject.FindGameObjectWithTag("SliceableOuter");

        var meshInner = modelInner.GetComponentsInChildren<MeshFilter>()[0].mesh;
        var meshOuter = modelOuter.GetComponentsInChildren<MeshFilter>()[0].mesh;

        string path = "";
        switch(et) 
        {
            case ExportType.Obj:
                path = StandaloneFileBrowser.SaveFilePanel("save file", "", "model", "obj");
                if (string.IsNullOrEmpty(path))
                    break;
                File.WriteAllText(path, MeshToStrObj(meshInner.vertices, meshInner.triangles, meshOuter.vertices, meshOuter.triangles, meshInner.vertices.Length, "\n"));
                Debug.Log(path);
                break;
            case ExportType.Stl:
                path = StandaloneFileBrowser.SaveFilePanel("save file", "", "model", "stl");
                if (string.IsNullOrEmpty(path))
                    break;
                File.WriteAllText(path, MeshtoStrStl(meshInner, meshOuter));
                Debug.Log(path);
                break;
            default:
                break;
        }
    }

#endif

    public string VerticiesToStr(Vector3 [] v3Arr, string splitStr)
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

    public string FacesToStr(int[] intArr, string splitStr, int length = 0)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < intArr.Length; i+=3)
        {
            sb.Append(splitStr).Append((intArr[i] + 1+length) + " " + (intArr[i + 1] + 1 + length) + " " + (intArr[i + 2] + 1 + length) + "\n");
        }
        if(sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1); // remove the last " "
        }
        return sb.ToString();
    }

    public string MeshToStrObj(Vector3[] verticesInner, int[] trianglesInner, Vector3[] verticesOuter, int[] trianglesOuter, int innerLength, string splitStr)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(VerticiesToStr(verticesInner, "v ")).Append(splitStr);
        sb.Append(VerticiesToStr(verticesOuter, "v ")).Append(splitStr);
        sb.Append(FacesToStr(trianglesInner, "f ")).Append(splitStr);
        sb.Append(FacesToStr(trianglesOuter, "f ", innerLength)).Append(splitStr);

        return sb.ToString();
    }

    public string MeshtoStrStl(Mesh meshInner, Mesh meshOuter) {
        StringBuilder sb = new StringBuilder();
        var innerVerticies = meshInner.vertices;
        var innerNormals = meshInner.normals;
        var innerTraingles = meshInner.triangles;
        var outerVertices = meshOuter.vertices;
        var outerNormals = meshOuter.normals;
        var outerTraingles = meshOuter.triangles;
        sb.Append("solid model.stl\n");
        for(var i = 0; i < innerTraingles.Length; i +=3) {
            var normal = Vector3.Normalize(innerNormals[innerTraingles[i]] + innerNormals[innerTraingles[i + 1]] + innerNormals[innerTraingles[i + 2]]);
            sb.Append(" facet normal " + normal.x + " " + normal.y + " " + normal.z + "\n");
            sb.Append("  outer loop\n");
            sb.Append("   vertex " + innerVerticies[innerTraingles[i]].x + " " + innerVerticies[innerTraingles[i]].y + " " + innerVerticies[innerTraingles[i]].z + "\n");
            sb.Append("   vertex " + innerVerticies[innerTraingles[i + 1]].x + " " + innerVerticies[innerTraingles[i + 1]].y + " " + innerVerticies[innerTraingles[i + 1]].z + "\n");
            sb.Append("   vertex " + innerVerticies[innerTraingles[i + 2]].x + " " + innerVerticies[innerTraingles[i + 2]].y + " " + innerVerticies[innerTraingles[i + 2]].z + "\n");
            sb.Append("  endloop\n");
            sb.Append(" endfacet\n");
        }
        for(var i = 0; i < outerTraingles.Length; i +=3) {
            var normal = Vector3.Normalize(outerNormals[outerTraingles[i]] + outerNormals[outerTraingles[i + 1]] + outerNormals[outerTraingles[i + 2]]);
            sb.Append(" facet normal " + normal.x + " " + normal.y + " " + normal.z + "\n");
            sb.Append("  outer loop\n");
            sb.Append("   vertex " + outerVertices[outerTraingles[i]].x + " " + outerVertices[outerTraingles[i]].y + " " + outerVertices[outerTraingles[i]].z + "\n");
            sb.Append("   vertex " + outerVertices[outerTraingles[i + 1]].x + " " + outerVertices[outerTraingles[i + 1]].y + " " + outerVertices[outerTraingles[i + 1]].z + "\n");
            sb.Append("   vertex " + outerVertices[outerTraingles[i + 2]].x + " " + outerVertices[outerTraingles[i + 2]].y + " " + outerVertices[outerTraingles[i + 2]].z + "\n");
            sb.Append("  endloop\n");
            sb.Append(" endfacet\n");
        }
        sb.Append("endsolid model.stl\n");
        return sb.ToString();
    }
}
