using System.Collections.Generic;
using UnityEngine;

public class MouseSlice : MonoBehaviour {

    public GameObject plane;
    public Transform ObjectContainerInner;
    public Transform ObjectContainerOuter;

    // How far away from the slice do we separate resulting objects
    public float separation;

    // Do we draw a plane object associated with the slice
    private Plane slicePlane = new Plane();
    public bool drawPlane;
    
    // Reference to the line renderer
    public ScreenLineRenderer lineRenderer;

    private MeshCutter meshCutter;
    private TempMesh biggerMesh, smallerMesh;
    MeshCasing meshCasing;
    MeshCasing.MeshCasings casing = new MeshCasing.MeshCasings();

    #region Utility Functions

    void DrawPlane(Vector3 start, Vector3 end, Vector3 normalVec)
    {
        Quaternion rotate = Quaternion.FromToRotation(Vector3.up, normalVec);

        plane.transform.localRotation = rotate;
        plane.transform.position = (end + start) / 2;
        plane.SetActive(true);
    }

    #endregion

    // Use this for initialization
    void Start () {
        // Initialize a somewhat big array so that it doesn't resize
        meshCutter = new MeshCutter(256);
        meshCasing = GameObject.Find("MeshEditor").GetComponent<MeshCasing>();
    }

    private void OnEnable()
    {
        lineRenderer.OnLineDrawn += OnLineDrawn;
    }

    private void OnDisable()
    {
        lineRenderer.OnLineDrawn -= OnLineDrawn;
    }

    private void OnLineDrawn(Vector3 start, Vector3 end, Vector3 depth)
    {
        var planeTangent = (end - start).normalized;

        // if we didn't drag, we set tangent to be on x
        if (planeTangent == Vector3.zero)
            planeTangent = Vector3.right;

        var normalVec = Vector3.Cross(depth, planeTangent);

        if (drawPlane) DrawPlane(start, end, normalVec);

        SliceObjects(start, normalVec);
    }
    

    void SliceObjects(Vector3 point, Vector3 normal)
    {
        // hashirs code edited
        var slicable = new List<GameObject>();
        slicable.AddRange(GameObject.FindGameObjectsWithTag("SliceableInner"));
        slicable.AddRange(GameObject.FindGameObjectsWithTag("SliceableOuter"));
        var toSlice = slicable.ToArray();
        //
        // Put results in positive and negative array so that we separate all meshes if there was a cut made
        List<Transform> positive = new List<Transform>(),
            negative = new List<Transform>();

        GameObject obj;
        bool slicedAny = false;
        //for (int i = 0; i < toSlice.Length; ++i)
        //{
        //    obj = toSlice[i];
        //    // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
        //    // Since this is how normal are transformed
        //    var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

        //    //Convert plane in object's local frame
        //    slicePlane.SetNormalAndPosition(
        //        transformedNormal,
        //        obj.transform.InverseTransformPoint(point));

        //    slicedAny = SliceObject(ref slicePlane, obj, positive, negative, obj.tag) || slicedAny;
        //}

        if (toSlice.Length > 1)
        {
            obj = toSlice[0];
            if (obj.tag == "SliceableInner")
            {
                // We multiply by the inverse transpose of the worldToLocal Matrix, a.k.a the transpose of the localToWorld Matrix
                // Since this is how normal are transformed
                var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Convert plane in object's local frame
                slicePlane.SetNormalAndPosition(
                    transformedNormal,
                    obj.transform.InverseTransformPoint(point));

                slicedAny = SliceObject(ref slicePlane, obj, positive, negative, obj.tag) || slicedAny;

                obj = toSlice[1];

                transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Convert plane in object's local frame
                slicePlane.SetNormalAndPosition(
                    transformedNormal,
                    obj.transform.InverseTransformPoint(point));

                slicedAny = SliceObject(ref slicePlane, obj, positive, negative, obj.tag) || slicedAny;
            }
            else
            {
                obj = toSlice[1];

                var transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Convert plane in object's local frame
                slicePlane.SetNormalAndPosition(
                    transformedNormal,
                    obj.transform.InverseTransformPoint(point));

                slicedAny = SliceObject(ref slicePlane, obj, positive, negative, obj.tag) || slicedAny;

                obj = toSlice[0];

                transformedNormal = ((Vector3)(obj.transform.localToWorldMatrix.transpose * normal)).normalized;

                //Convert plane in object's local frame
                slicePlane.SetNormalAndPosition(
                    transformedNormal,
                    obj.transform.InverseTransformPoint(point));

                slicedAny = SliceObject(ref slicePlane, obj, positive, negative, obj.tag) || slicedAny;
            }
        }


        // Separate meshes if a slice was made
        //if (slicedAny)
        //    SeparateMeshes(positive, negative, normal);
    }

    bool SliceObject(ref Plane slicePlane, GameObject obj, List<Transform> positiveObjects, List<Transform> negativeObjects, string tag)
    {
        var mesh = obj.GetComponent<MeshFilter>().mesh;

        if (!meshCutter.SliceMesh(mesh, ref slicePlane, tag))
        {
            // Put object in the respective list
            if (slicePlane.GetDistanceToPoint(meshCutter.GetFirstVertex()) >= 0)
                positiveObjects.Add(obj.transform);
            else
                negativeObjects.Add(obj.transform);

            return false;
        }

        // TODO: Update center of mass

        // Silly condition that labels which mesh is bigger to keep the bigger mesh in the original gameobject
        bool posBigger = meshCutter.PositiveMesh.surfacearea > meshCutter.NegativeMesh.surfacearea;
        if (posBigger)
        {
            biggerMesh = meshCutter.PositiveMesh;
            smallerMesh = meshCutter.NegativeMesh;
        }
        else
        {
            biggerMesh = meshCutter.NegativeMesh;
            smallerMesh = meshCutter.PositiveMesh;
        }

        // Create new Sliced object with the other mesh
        //GameObject newObject = Instantiate(obj, ObjectContainer);
        //newObject.transform.SetPositionAndRotation(obj.transform.position, obj.transform.rotation);
        //var newObjMesh = newObject.GetComponent<MeshFilter>().mesh;

        // Put the bigger mesh in the original object
        // TODO: Enable collider generation (either the exact mesh or compute smallest enclosing sphere)
        if (tag == "SliceableInner")
        {
            //ReplaceMesh(casing.Inner,biggerMesh);
            if (casing.Inner == null) {
                casing.Inner = new Mesh();
            }
            casing.Inner.Clear();
            casing.Inner.SetVertices(biggerMesh.vertices);
            casing.Inner.SetTriangles(biggerMesh.triangles, 0);
            casing.Inner.SetNormals(biggerMesh.normals);
            casing.Inner.SetUVs(0, biggerMesh.uvs);

            //mesh.RecalculateNormals();
            casing.Inner.RecalculateTangents();

        }
        else
        {
            //ReplaceMesh(casing.Outer, biggerMesh);
            if (casing.Outer == null)
            {
                casing.Outer = new Mesh();
            }
            casing.Outer.Clear();
            casing.Outer.SetVertices(biggerMesh.vertices);
            casing.Outer.SetTriangles(biggerMesh.triangles, 0);
            casing.Outer.SetNormals(biggerMesh.normals);
            casing.Outer.SetUVs(0, biggerMesh.uvs);

            //mesh.RecalculateNormals();
            casing.Outer.RecalculateTangents();

        }

        if (casing.Inner!=null && casing.Outer!=null)
        {
            meshCasing.undoList.Push(casing);
            casing.Inner = null;
            casing.Outer = null;
        }
        
        ReplaceMesh(mesh, biggerMesh);
        //ReplaceMesh(newObjMesh, smallerMesh);

        (posBigger ? positiveObjects : negativeObjects).Add(obj.transform);
        //(posBigger ? negativeObjects : positiveObjects).Add(newObject.transform);

        return true;
    }


    /// <summary>
    /// Replace the mesh with tempMesh.
    /// </summary>
    void ReplaceMesh(Mesh mesh, TempMesh tempMesh, MeshCollider collider = null)
    {
        mesh.Clear();
        mesh.SetVertices(tempMesh.vertices);
        mesh.SetTriangles(tempMesh.triangles, 0);
        mesh.SetNormals(tempMesh.normals);
        mesh.SetUVs(0, tempMesh.uvs);
        
        //mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if (collider != null && collider.enabled)
        {
            collider.sharedMesh = mesh;
            collider.convex = true;
        }
    }

    void SeparateMeshes(Transform posTransform, Transform negTransform, Vector3 localPlaneNormal)
    {
        // Bring back normal in world space
        Vector3 worldNormal = ((Vector3)(posTransform.worldToLocalMatrix.transpose * localPlaneNormal)).normalized;

        Vector3 separationVec = worldNormal * separation;
        // Transform direction in world coordinates
        posTransform.position += separationVec;
        negTransform.position -= separationVec;
    }

    void SeparateMeshes(List<Transform> positives, List<Transform> negatives, Vector3 worldPlaneNormal)
    {
        int i;
        var separationVector = worldPlaneNormal * separation;

        for(i = 0; i <positives.Count; ++i)
            positives[i].transform.position += separationVector;

        for (i = 0; i < negatives.Count; ++i)
            negatives[i].transform.position -= separationVector;
    }
}
