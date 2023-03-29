using UnityEngine;

public class TransformController : MonoBehaviour
{
    public GameObject FingerSocket;
    public GameObject HookSocket;

    [HideInInspector]
    public GameObject SelectedSocket { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        SelectedSocket = FingerSocket;
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
    }

    public void ChangePosition(Vector3 newPos)
    {
        SelectedSocket.transform.position = newPos;
    }

    public void ChangeRotation(Quaternion newRotation)
    {
        SelectedSocket.transform.rotation = newRotation;
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
