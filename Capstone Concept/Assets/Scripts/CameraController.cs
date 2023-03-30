using UnityEngine;
using System.Linq;

public class CameraController : MonoBehaviour
{
    [HideInInspector]
    public GameObject handObj;
    [HideInInspector]
    public Vector3? handCenter { get; private set; }
    FileExplorer fileExplorer;
    private Vector3 fixedPosition;
    public float distance = 100.0f;
    public float xSpeed = 5.0f;
    public float ySpeed = 5.0f;
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
    public float distanceMin = 10f;
    public float distanceMax = 400f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    private bool middleButtonPressed = false;


    void Start()
    {
        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();

        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;

        // Clone the target's position so that it stays fixed
        fixedPosition = Vector3.zero;
    }

    // Putting all start up code in new function
    public void LookAtHand()
    {
        handObj = fileExplorer.model;
        var handTransform = handObj.transform.GetChild(0);
        var mesh = handTransform.GetComponent<MeshFilter>().mesh;
        handCenter = GetCenter(mesh);
        fixedPosition = handCenter ?? Vector3.zero;
    }

    void LateUpdate()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            fixedPosition = handCenter ?? Vector3.zero;
            distance = 100.0f;
        } 

        if (Input.GetMouseButton(1))
        {
            rotationYAxis += xSpeed * Input.GetAxis("Mouse X");
            rotationXAxis -= ySpeed * Input.GetAxis("Mouse Y");
            rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);
        }

        if(Input.GetMouseButtonDown(2))
            middleButtonPressed = true;
        if (Input.GetMouseButtonUp(2))
            middleButtonPressed = false;
        if(middleButtonPressed)
        {
            var xTranslation = Vector3.Cross(transform.forward, transform.up) * Input.GetAxis("Mouse X") * distance / 100.0f;
            var yTranslation = transform.up * Input.GetAxis("Mouse Y") * - 1 * distance / 100.0f;
            fixedPosition += xTranslation + yTranslation;
        }

        Quaternion rotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * distance, distanceMin, distanceMax);
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + fixedPosition;

        transform.rotation = rotation;
        transform.position = position;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
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
}
