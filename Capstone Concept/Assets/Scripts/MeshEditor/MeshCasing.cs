using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class MeshCasing : MonoBehaviour
{
    [Range(1.0f, 10.0f)]
    public float thicknessInMillimeters = 1;
    public GameObject handObj;
    public Material casingMaterial;
    private bool isCasingGenerated = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void generateCasing()
    {
        if(isCasingGenerated) {
            Debug.Log("not again");
            return;
        }
        
        var handtransform = handObj.transform.GetChild(0);
        var casing = Instantiate(handObj, new Vector3(0, 0, 0), Quaternion.identity);

        var mesh = casing.transform.GetChild(0).GetComponent<MeshFilter>().mesh;
        var normals = mesh.normals;
        var innerVerts = mesh.vertices;
        var innerTriangles = mesh.triangles;
        var vertsSize = innerVerts.Length;

        // create outer mesh
        var outerVerts = new Vector3[innerVerts.Length];
        var outerTraingles = new int[innerTriangles.Length];
        var normalsCollection = new Dictionary<Vector3, Vector3>();
        for(var i = 0; i < outerVerts.Length; i++) {
            var n = GetNormal(ref normalsCollection, normals[i], innerVerts[i]);
            outerVerts[i] = innerVerts[i] + (n * thicknessInMillimeters);
        }
        for(var i = 0; i < outerTraingles.Length; i++) {
            outerTraingles[i] = innerTriangles[i] + innerVerts.Length;
        }

        float f = 10.123456f;
        var fc = (float)Math.Round(f, 2);

        // flip inner mesh
        for(var i = 0; i < innerTriangles.Length; i += 3) {
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

        isCasingGenerated = true;
    }

    private Vector3 GetNormal(ref Dictionary<Vector3, Vector3> normalsCollection, Vector3 normal, Vector3 vert) {
        var roundedVert = new Vector3(
            (float)Math.Round(vert.x, 1), 
            (float)Math.Round(vert.y, 1), 
            (float)Math.Round(vert.z, 1));
        
        if(!normalsCollection.ContainsKey(roundedVert)) {
            normalsCollection.Add(roundedVert, normal);
            return normal;
        }

        return normalsCollection[roundedVert];
    }
}
