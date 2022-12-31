using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    public GameObject handObj;
    // Start is called before the first frame update

    private Transform cameraTransform;
    private Vector3 handCenter;
    void Start()
    {
        cameraTransform = gameObject.GetComponent<Transform>();
        var handTransform = handObj.transform.GetChild(0);
        var mesh = handTransform.GetComponent<MeshFilter>().mesh;
        handCenter = GetCenter(mesh);  
    }

    // Update is called once per frame
    void Update()
    {
        cameraTransform.LookAt(handCenter);
    }

    private Vector3 GetCenter(Mesh mesh)
    {
        var verts = mesh.vertices.ToList();
        var minX = verts.Select(v => v.x).Min();
        var maxX = verts.Select(v => v.x).Max();
        var minY = verts.Select(v => v.y).Min();
        var maxY = verts.Select(v => v.y).Max(); 
        var minZ = verts.Select(v => v.z).Min(); 
        var maxZ = verts.Select(v => v.z).Max();

        return new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
    }
}
