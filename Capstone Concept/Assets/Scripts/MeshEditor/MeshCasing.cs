using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class MeshCasing : MonoBehaviour
{
    private GameObject handObj;
    public Material casingMaterial;
    public GameObject CasingPanel;
    public GameObject CasingOuter;
    public GameObject CasingInner;
    private GameObject CasingOuterOriginal;
    private GameObject CasingInnerOriginal;
    public bool isCasingGenerated = false;
    private float thicknessInMillimeters = 1;
    FileExplorer fileExplorer;
    MouseSlice slice;
    public Stack<MeshCasings> undoList = new Stack<MeshCasings>();

    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
        {
            Debug.Log("Key Z hit");
            Debug.Log(undoList.Count);
            if (undoList.Count>=2)
            {
                Debug.Log("More than two undos");
                undoList.Pop();
                var temp = undoList.Peek();
                CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp.Inner;
                CasingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp.Outer;
                //undoList.Pop();
            }else if (undoList.Count == 1)
            {
                Debug.Log("Only one undo");
                var temp = undoList.Peek();
                CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp.Inner;
                CasingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh = temp.Outer;
            }
            
        }
    }

    public struct MeshCasings
    {
        public Mesh Inner;
        public Mesh Outer;
    }
    /// <summary>
    /// 
    /// </summary>
    public void generateCasing()
    {
        //if (isCasingGenerated)
        //{
        //    return;
        //}

        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();

        handObj = fileExplorer.model;
        if (isCasingGenerated == true)
        {
            Destroy(CasingInner);
            Destroy(CasingOuter);
        }
        CasingInner = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);
        CasingOuter = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);

        // hashir code edited
        CasingInner.transform.GetChild(0).tag = "SliceableInner";
        CasingOuter.transform.GetChild(0).tag = "SliceableOuter";
        //
        


        var meshInner = CasingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var meshOuter = CasingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var normals = meshInner.normals;
        var innerVerts = meshInner.vertices;
        var innerTriangles = meshInner.triangles;

        // create outer mesh
        var outerVerts = new Vector3[innerVerts.Length];
        var outerTraingles = new int[innerTriangles.Length];
        var normalsCollection = new Dictionary<Vector3, Vector3>();
        for (var i = 0; i < outerVerts.Length; i++)
        {
            var n = GetNormal(ref normalsCollection, normals[i], innerVerts[i]);
            outerVerts[i] = innerVerts[i] + (n * thicknessInMillimeters);
        }
        for (var i = 0; i < outerTraingles.Length; i++)
        {
            outerTraingles[i] = innerTriangles[i];// + innerVerts.Length;
        }


        // flip inner mesh
        for (var i = 0; i < innerTriangles.Length; i += 3)
        {
            var temp = innerTriangles[i + 1];
            innerTriangles[i + 1] = innerTriangles[i + 2];
            innerTriangles[i + 2] = temp;
        }


        meshInner.vertices = innerVerts.ToArray();//.Concat(outerVerts).ToArray();
        meshInner.triangles = innerTriangles.ToArray();//.Concat(outerTraingles).ToArray();

        //mesh.vertices = innerVerts.Concat(outerVerts).ToArray();
        //mesh.triangles = innerTriangles.Concat(outerTraingles).ToArray();
        meshInner.RecalculateTangents();
        meshInner.RecalculateBounds();
        meshInner.RecalculateNormals();

        CasingInner.name = "Hand Casing Inner";
        CasingInner.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        meshOuter.triangles = outerTraingles.ToArray();
        meshOuter.vertices = outerVerts.ToArray();
        //meshOuter.triangles = outerTraingles.ToArray();

        meshOuter.RecalculateTangents();
        meshOuter.RecalculateBounds();
        meshOuter.RecalculateNormals();

        CasingOuter.name = "Hand Casing Outer";
        CasingOuter.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;
        
        slice = GameObject.Find("SliceManager").GetComponent<MouseSlice>();
        slice.ObjectContainerInner = CasingInner.transform;
        slice.ObjectContainerOuter = CasingOuter.transform;

        MeshCasings casing = new MeshCasings();

        casing.Inner = new Mesh();
        casing.Inner.vertices = meshInner.vertices;
        casing.Inner.triangles = meshInner.triangles;
        casing.Inner.uv = meshInner.uv;
        casing.Inner.normals = meshInner.normals;
        casing.Inner.colors = meshInner.colors;
        casing.Inner.tangents = meshInner.tangents;


        casing.Outer = new Mesh();
        casing.Outer.vertices = meshOuter.vertices;
        casing.Outer.triangles = meshOuter.triangles;
        casing.Outer.uv = meshOuter.uv;
        casing.Outer.normals = meshOuter.normals;
        casing.Outer.colors = meshOuter.colors;
        casing.Outer.tangents = meshOuter.tangents;

        undoList.Push(casing);

        isCasingGenerated = true;
        CasingPanel.SetActive(false);
    }

    public void resetCase()
    {
        if (isCasingGenerated == false) { return; }
        generateCasing();
    }

    public void setThickness(Slider slider)
    {
        thicknessInMillimeters = slider.value;
    }
    public void updateSliderValue(GameObject text)
    {
        var textValue = text.GetComponent<TextMeshProUGUI>();
        textValue.text = Math.Round(thicknessInMillimeters, 2).ToString() + "mm";
    }

    public void openCasingPanel()
    {
        //if (isCasingGenerated) { return; }

        CasingPanel.SetActive(true);
    }

    public void closeCasingPanel()
    {
        CasingPanel.SetActive(false);
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
}
