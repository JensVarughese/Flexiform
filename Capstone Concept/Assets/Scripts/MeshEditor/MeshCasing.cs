using UnityEngine;

public class MeshCasing : MonoBehaviour
{
    public GameObject handObj;
    public Material casingMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void generateCasing()
    {
        var handtransform = handObj.transform.GetChild(0);
        var mesh = handtransform.GetComponent<MeshFilter>().mesh;
        var normals = mesh.normals;
        var verts = mesh.vertices;

        var casing = Instantiate(handObj, new Vector3(100.0f, 0, 0), Quaternion.identity);
        casing.name = "Hand Casing";
        casing.transform.GetChild(0).GetComponent<MeshRenderer>().material = casingMaterial;

        Debug.Log("Casing Generated");
    }
}
