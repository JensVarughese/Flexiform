using UnityEngine;
using RuntimeHandle;

public class TransformController : MonoBehaviour
{
    public GameObject FingerSocket;
    public GameObject HookSocket;

    [HideInInspector]
    public GameObject SelectedSocket { get; private set; }
    [HideInInspector]
    public GameObject runtimeTransformGameObj { get; private set; }
    private RuntimeTransformHandle runtimeTransformHandle;
    private int runtimeTransformLayer = 6;
    private int runtimeTransformLayerMask;

    //private RuntimeTransformHandle RuntimeTransformHandle;
    // Start is called before the first frame update
    void Start()
    {
        SelectedSocket = FingerSocket;

        runtimeTransformGameObj = new GameObject();
        runtimeTransformHandle = runtimeTransformGameObj.AddComponent<RuntimeTransformHandle>();
        runtimeTransformGameObj.layer = runtimeTransformLayer;
        runtimeTransformLayerMask = 1 << runtimeTransformLayer; //Layer number represented by a single bit in the 32-bit integer using bit shift
        runtimeTransformHandle.type = HandleType.POSITION;
        runtimeTransformHandle.autoScale = true;
        runtimeTransformHandle.autoScaleFactor = 1.0f;
        runtimeTransformGameObj.SetActive(false);
    }

    public void LoadSocket(string selection, Vector3 position)
    {
        switch(selection)
        {
            case "Finger Socket":
                SelectedSocket = FingerSocket;
                break;
            case "Hook":
                SelectedSocket = HookSocket;
                break;
            default:
                Debug.LogError("Error with socket selection");
                break;
        }

        SelectedSocket.SetActive(true);
        SelectedSocket.transform.position = position;
        runtimeTransformHandle.target = SelectedSocket.transform;
        runtimeTransformGameObj.SetActive(true);
    }

    public void ChangePosition(Vector3 newPos)
    {
        SelectedSocket.transform.position = newPos;
    }

    public void ChangeRotation(Quaternion newRotation)
    {
        SelectedSocket.transform.rotation = newRotation;
    }

    public void ChangeHandleType(HandleType type)
    {
        runtimeTransformHandle.type = type;
    }

    public Vector3 GetPosition()
    {
        return SelectedSocket.transform.position;
    }

    public Quaternion GetRotation()
    {
        return SelectedSocket.transform.rotation;
    }

    public Vector3 GetEulers()
    {
        return SelectedSocket.transform.rotation.eulerAngles;
    }
}
