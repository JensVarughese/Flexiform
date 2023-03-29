using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    public GameObject handObj;
    private Transform cameraTransform;
    private Transform parentTransform;
    [HideInInspector]
    public Vector3 handCenter { get; private set; }
    private Vector3? mousePos;
    private float distance;
    FileExplorer fileExplorer;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = gameObject.GetComponent<Transform>();
        parentTransform = cameraTransform.parent;
        //var handTransform = handObj.transform.GetChild(0);
        //var mesh = handTransform.GetComponent<MeshFilter>().mesh;
        handCenter = new Vector3(0,0,0);
        //parentTransform.position = handCenter;
        //cameraTransform.LookAt(handCenter);
        //distance = Vector3.Distance(transform.position, handCenter);
        parentTransform.position = new Vector3(0,0,0);
        cameraTransform.LookAt(new Vector3(0, 0, 0));
        distance = Vector3.Distance(transform.position, new Vector3(0, 0, 0));
        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();
    }

    // Putting all start up code in new function
    public void StartUp()
    {
        handObj = fileExplorer.model;

        cameraTransform = gameObject.GetComponent<Transform>();
        parentTransform = cameraTransform.parent;
        var handTransform = handObj.transform.GetChild(0);
        var mesh = handTransform.GetComponent<MeshFilter>().mesh;
        handCenter = GetCenter(mesh);
        parentTransform.position = handCenter;
        cameraTransform.LookAt(handCenter);
        distance = Vector3.Distance(transform.position, handCenter);
    }

    // Update is called once per frame
    void Update()
    {
        //Fire2 = Right Click
        if (Input.GetButton("Fire2"))
        {
            RotateCamera();
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            mousePos = null;
        }

        //allows zooming
        ZoomCamera(Input.mouseScrollDelta.y);
    }

    public Vector3 GetCameraPostion()
    {
        return transform.position;
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

        // mouse position after the rotation is done
        var newMousePos = Input.mousePosition;

        //calculate mouse position differences and angle to rotate
        var diffX = (mousePos?.x ?? 1) - newMousePos.x;
        var diffY = (mousePos?.y ?? 1) - newMousePos.y;
        var xRot = transform.eulerAngles.x;

        //edge cases for boundaries
        var edgeCase1 = isBet(xRot, 70, 180) && diffY < 0;
        var edgeCase2 = isBet(xRot, 180, 290) && diffY > 0;

        if (isBet(xRot, -1, 70) || isBet(xRot, 290, 360) || edgeCase1 || edgeCase2) 
        {
            parentTransform.Rotate(-diffY / 2.0f, -diffX / 2.0f, 0.0f, Space.Self);
        }

        cameraTransform.LookAt(handCenter);

        //set mouse position
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
        if (handCenter == null)
        {
            distance = Vector3.Distance(transform.position, new Vector3(0,0,0));
        }
        else
        {
            distance = Vector3.Distance(transform.position, handCenter);
        }
    }

    private bool isBet(float num, int p1, int p2)
    {
        return num >= p1 && num <= p2;
    }
}
