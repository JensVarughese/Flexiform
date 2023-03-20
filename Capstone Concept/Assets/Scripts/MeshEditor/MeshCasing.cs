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
    public bool isCasingGenerated = false;
    private float thicknessInMillimeters = 1;
    FileExplorer fileExplorer;
    MouseSlice slice;

    /// <summary>
    /// 
    /// </summary>
    public void generateCasing()
    {
        if (isCasingGenerated)
        {
            return;
        }

        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();

        handObj = fileExplorer.model;

        var casingInner = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);
        var casingOuter = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);
        // hashir code edited
        casingInner.transform.GetChild(0).tag = "SliceableInner";
        casingOuter.transform.GetChild(0).tag = "SliceableOuter";
        //

        var meshInner = casingInner.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var meshOuter = casingOuter.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
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

        casingInner.name = "Hand Casing Inner";
        casingInner.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        meshOuter.vertices = outerVerts.ToArray();
        meshOuter.triangles = outerTraingles.ToArray();

        meshOuter.RecalculateTangents();
        meshOuter.RecalculateBounds();
        meshOuter.RecalculateNormals();

        casingOuter.name = "Hand Casing Outer";
        casingOuter.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        CasingOuter = casingOuter;
        CasingInner = casingInner;
        
        slice = GameObject.Find("SliceManager").GetComponent<MouseSlice>();
        slice.ObjectContainerInner = casingInner.transform;
        slice.ObjectContainerOuter = casingOuter.transform;

        isCasingGenerated = true;
        CasingPanel.SetActive(false);
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
        if (isCasingGenerated) { return; }

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
