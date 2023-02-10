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
    private bool isCasingGenerated = false;
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

        var casing = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);
        casing.transform.GetChild(0).tag = "Sliceable";

        var mesh = casing.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var normals = mesh.normals;
        var innerVerts = mesh.vertices;
        var innerTriangles = mesh.triangles;

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
            outerTraingles[i] = innerTriangles[i] + innerVerts.Length;
        }

        // flip inner mesh
        for (var i = 0; i < innerTriangles.Length; i += 3)
        {
            var temp = innerTriangles[i + 1];
            innerTriangles[i + 1] = innerTriangles[i + 2];
            innerTriangles[i + 2] = temp;
        }

        mesh.vertices = innerVerts.Concat(outerVerts).ToArray();
        mesh.triangles = innerTriangles.Concat(outerTraingles).ToArray();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        casing.name = "Hand Casing";
        casing.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        slice = GameObject.Find("SliceManager").GetComponent<MouseSlice>();
        slice.ObjectContainer = casing.transform;

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
