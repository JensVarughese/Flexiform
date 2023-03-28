using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangePosition(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public void ChangeRotation(Quaternion newRotation)
    {
        transform.rotation = newRotation;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
