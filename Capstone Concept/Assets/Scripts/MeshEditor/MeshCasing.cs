using UnityEngine;

public class MeshCasing : MonoBehaviour
{
    public GameObject handObj;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void generateCasing()
    {
        var handtransform = handObj.transform.GetChild(0);
        var mesh = handtransform.GetComponent<MeshFilter>().mesh;
        var normals = mesh.normals;
        var verts = mesh.vertices;
        Debug.Log(verts.Length);
        Debug.Log(normals.Length);
    }
}
