using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class MeshCasing : MonoBehaviour
{
    private GameObject handObj;
    public Material casingMaterial;
    [HideInInspector]
    public GameObject CasingOuter;
    [HideInInspector]
    public GameObject CasingInner;
    [HideInInspector]
    public bool isCasingGenerated = false;
    public float thicknessInMillimeters = 1;
    FileExplorer fileExplorer;
    MouseSlice slice;
    public Stack<Mesh> undoList = new Stack<Mesh>();

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (undoList.Count>=2)
            {
                undoList.Pop();
                var temp = undoList.Peek();
                CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp;
                generateCasing();
            }else if (undoList.Count == 1)
            {
                var temp = undoList.Peek();
                CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp;
                generateCasing();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void generateCasing()
    {
        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();

        handObj = fileExplorer.model;
        if (isCasingGenerated == true)
        {
            Destroy(CasingOuter);
        }
        else {
            CasingInner = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);
        }
        
        CasingOuter = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);

        // hashir code edited
        CasingInner.transform.GetChild(0).tag = "Sliceable";
        CasingOuter.transform.GetChild(0).tag = "SliceableOuter";

        var meshInner = CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var meshOuter = CasingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        meshOuter.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // allows verticies limit to increase from 65535 to 4 billion

        
        var innerVerts = meshInner.vertices;
        var innerTriangles = meshInner.triangles;
        if(!isCasingGenerated) {
            // flip inner mesh
            for (var i = 0; i < innerTriangles.Length; i += 3)
            {
                var temp = innerTriangles[i + 1];
                innerTriangles[i + 1] = innerTriangles[i + 2];
                innerTriangles[i + 2] = temp;
            }

            meshInner.vertices = innerVerts.ToArray();
            meshInner.triangles = innerTriangles.ToArray();
            meshInner.RecalculateTangents();
            meshInner.RecalculateBounds();
            meshInner.RecalculateNormals();
        }
        var normals = meshInner.normals;
        
        // create outer mesh
        var outerVerts = new List<Vector3>();
        var outerTriangles = new List<int>();
        var normalsCollection = new Dictionary<Vector3, Vector3>();
        for(var i = 0; i < innerTriangles.Length; i +=3) {
            var v1 = innerVerts[innerTriangles[i]];
            var v2 = innerVerts[innerTriangles[i + 1]];
            var v3 = innerVerts[innerTriangles[i + 2]];
            var v4 = v1 + GetNormal(ref normalsCollection, normals[innerTriangles[i]], v1) * thicknessInMillimeters * -1;
            var v5 = v2 + GetNormal(ref normalsCollection, normals[innerTriangles[i + 1]], v2) * thicknessInMillimeters  * -1;
            var v6 = v3 + GetNormal(ref normalsCollection, normals[innerTriangles[i + 2]], v3) * thicknessInMillimeters  * -1;
            AddTriangle(outerVerts, outerTriangles, v4, v6, v5);
            AddTriangle(outerVerts, outerTriangles, v1, v3, v6);
            AddTriangle(outerVerts, outerTriangles, v1, v6, v4);
            AddTriangle(outerVerts, outerTriangles, v2, v1, v4);
            AddTriangle(outerVerts, outerTriangles, v2, v4, v5);
            AddTriangle(outerVerts, outerTriangles, v3, v2, v5);
            AddTriangle(outerVerts, outerTriangles, v3, v5, v6);
        }
        meshOuter.vertices = outerVerts.ToArray();
        meshOuter.triangles = outerTriangles.ToArray();
        meshOuter.RecalculateNormals();
        meshOuter.RecalculateTangents();
        meshOuter.RecalculateBounds();
        
        CasingInner.name = "Hand Casing Inner";
        CasingInner.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        CasingOuter.name = "Hand Casing Outer";
        CasingOuter.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;
        
        slice = GameObject.Find("SliceManager").GetComponent<MouseSlice>();
        slice.ObjectContainer = CasingInner.transform;

        // history logic
        if(undoList.Count > 0 && undoList.Peek().vertexCount == meshInner.vertexCount)
            return;
        var saveInnner = new Mesh();
        saveInnner.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        saveInnner.vertices = meshInner.vertices;
        saveInnner.triangles = meshInner.triangles;
        saveInnner.uv = meshInner.uv;
        saveInnner.normals = meshInner.normals;
        saveInnner.colors = meshInner.colors;
        saveInnner.tangents = meshInner.tangents;
        undoList.Push(saveInnner);

        isCasingGenerated = true;
    }

    public void IntegrateSocket(GameObject socket, GameObject handle)
    {
        var meshSocket = socket.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var meshOuter = CasingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var oLength = meshOuter.vertexCount;
        var outerVerts = meshOuter.vertices.ToList();
        var outerTriangles = meshOuter.triangles.ToList();
        for(var i = 0; i < meshSocket.vertexCount; i++)
        {
            outerVerts.Add(socket.transform.TransformVector(meshSocket.vertices[i]) + socket.transform.position);
        }
        for(var i = 0; i < meshSocket.triangles.Length; i++)
        {
            outerTriangles.Add(meshSocket.triangles[i] + oLength);
        }

        meshOuter.vertices = outerVerts.ToArray();
        meshOuter.triangles = outerTriangles.ToArray();
        meshOuter.RecalculateNormals();
        meshOuter.RecalculateTangents();
        meshOuter.RecalculateBounds();

        socket.transform.position = new Vector3(0, 0, 0);
        socket.transform.rotation = Quaternion.identity;
        socket.SetActive(false);
        handle.SetActive(false);

    }

    /// <summary>
    /// Gets a normal of a nearby vertex, or returns its current normal
    /// </summary>
    /// <param name="normalsCollection"> Dictionary of saved normal vector</param>
    /// <param name="normal">normal vector of vertex</param>
    /// <param name="vert">specified vertex for normal calculation</param>
    /// <returns>The ideal normal vector</returns>
    private Vector3 GetNormal(ref Dictionary<Vector3, Vector3> normalsCollection, Vector3 normal, Vector3 vert)
    {
        var roundedVert = new Vector3(
            (float)Math.Round(vert.x, 1),
            (float)Math.Round(vert.y, 1),
            (float)Math.Round(vert.z, 1));

        if (!normalsCollection.ContainsKey(roundedVert))
        {
            normalsCollection.Add(roundedVert, normal);
            return normal;
        }

        return normalsCollection[roundedVert];
    }

    private void AddTriangle(List<Vector3> vertices, List<int> traingles, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        vertices.AddRange(new Vector3[]{v1, v2, v3});
        var size = vertices.Count;
        traingles.AddRange(new int[]{size - 3, size - 2, size - 1});
    }
}
