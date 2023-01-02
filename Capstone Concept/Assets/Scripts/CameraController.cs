using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    public GameObject handObj;
    private Transform cameraTransform;
    private Vector3 handCenter;
    private float distance;
    private Vector3? mousePos;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = gameObject.GetComponent<Transform>();
        var handTransform = handObj.transform.GetChild(0);
        var mesh = handTransform.GetComponent<MeshFilter>().mesh;
        handCenter = GetCenter(mesh);
        cameraTransform.LookAt(handCenter);
        distance = Vector3.Distance(transform.position, handCenter);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetButton("Fire2"))
            RotateCamera();
        else if (Input.GetButtonUp("Fire2"))
            mousePos = null;

        ZoomCamera(Input.mouseScrollDelta.y);
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

    private void RotateCamera()
    {
        if (mousePos is null)
        {
            mousePos = Input.mousePosition;
            return;
        }

        var newMousePos = Input.mousePosition;
        var diffX = (mousePos?.x ?? 1) - newMousePos.x;
        var diffY = (mousePos?.y ?? 1) - newMousePos.y;
        var xRot = transform.eulerAngles.x;
        var rotationScale = distance / 200;

        transform.Translate(Vector3.right * diffX * rotationScale);

        var edgeCase1 = isBet(xRot, 70, 180) && diffY < 0;
        var edgeCase2 = isBet(xRot, 180, 290) && diffY > 0;
        if (isBet(xRot, 0, 70) || isBet(xRot, 290, 360) || edgeCase1 || edgeCase2) 
            transform.Translate(Vector3.up * diffY * rotationScale);

        cameraTransform.LookAt(handCenter);

        mousePos = newMousePos;
    }

    private void ZoomCamera(float scrollAmount) 
    {
        if(scrollAmount == 0)
            return;

        // bound checking
        if(distance >= 500 && scrollAmount < 0)
            return;
        else if(distance <= 50 && scrollAmount > 0)
            return;

        transform.Translate(Vector3.forward * scrollAmount * distance / 20);
        distance = Vector3.Distance(transform.position, handCenter);
    }

    private bool isBet(float num, int p1, int p2)
    {
        return num >= p1 && num <= p2;
    }
}
