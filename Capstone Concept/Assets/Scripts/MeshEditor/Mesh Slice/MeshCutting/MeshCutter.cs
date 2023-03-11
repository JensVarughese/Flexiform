using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCutter
{

    public TempMesh PositiveMesh { get; private set; }
    public TempMesh NegativeMesh { get; private set; }

    private List<Vector3> addedPairsInner;
    private List<Vector3> addedPairsOuter;

    private readonly List<Vector3> ogVertices;
    private readonly List<int> ogTriangles;
    private readonly List<Vector3> ogNormals;
    private readonly List<Vector2> ogUvs;

    private readonly Vector3[] intersectPair;
    private readonly Vector3[] tempTriangle;

    private Intersections intersect;

    private readonly float threshold = 1e-6f;

    public MeshCutter(int initialArraySize)
    {
        PositiveMesh = new TempMesh(initialArraySize);
        NegativeMesh = new TempMesh(initialArraySize);

        addedPairsInner = new List<Vector3>(initialArraySize);
        addedPairsOuter = new List<Vector3>(initialArraySize);
        ogVertices = new List<Vector3>(initialArraySize);
        ogNormals = new List<Vector3>(initialArraySize);
        ogUvs = new List<Vector2>(initialArraySize);
        ogTriangles = new List<int>(initialArraySize * 3);

        intersectPair = new Vector3[2];
        tempTriangle = new Vector3[3];

        intersect = new Intersections();
    }

    /// <summary>
    /// Slice a mesh by the slice plane.
    /// We assume the plane is already in the mesh's local coordinate frame
    /// Returns posMesh and negMesh, which are the resuling meshes on both sides of the plane 
    /// (posMesh on the same side as the plane's normal, negMesh on the opposite side)
    /// </summary>
    public bool SliceMesh(Mesh mesh, ref Plane slice, string tag)
    {

        // Let's always fill the vertices array so that we can access it even if the mesh didn't intersect
        mesh.GetVertices(ogVertices);

        // 1. Verify if the bounds intersect first
        if (!Intersections.BoundPlaneIntersect(mesh, ref slice))
            return false;

        mesh.GetTriangles(ogTriangles, 0);
        mesh.GetNormals(ogNormals);
        mesh.GetUVs(0, ogUvs);

        PositiveMesh.Clear();
        NegativeMesh.Clear();
        //addedPairs.Clear();

        // 2. Separate old vertices in new meshes
        for(int i = 0; i < ogVertices.Count; ++i)
        {
            if (slice.GetDistanceToPoint(ogVertices[i]) >= 0)
                PositiveMesh.AddVertex(ogVertices, ogNormals, ogUvs, i);
            else
                NegativeMesh.AddVertex(ogVertices, ogNormals, ogUvs, i);
        }

        // 2.5 : If one of the mesh has no vertices, then it doesn't intersect
        if (NegativeMesh.vertices.Count == 0 || PositiveMesh.vertices.Count == 0)
            return false;

        // 3. Separate triangles and cut those that intersect the plane
        for (int i = 0; i < ogTriangles.Count; i += 3)
        {
            if (intersect.TrianglePlaneIntersect(ogVertices, ogUvs, ogTriangles, i, ref slice, PositiveMesh, NegativeMesh, intersectPair))
            {
                //Debug.Log(tag + " " + intersectPair[0] + " " + intersectPair[1]);
                if (tag == "SliceableOuter")
                    addedPairsOuter.AddRange(intersectPair);
                else
                    addedPairsInner.AddRange(intersectPair);
            }
        }

        if (addedPairsOuter.Count > 0 || addedPairsInner.Count > 0)
        {
            //FillBoundaryGeneral(addedPairs);
            FillBoundaryFace(addedPairsOuter, addedPairsInner);
            return true;
        } else
        {
            throw new UnityException("Error: if added pairs is empty, we should have returned false earlier");
        }
    }

    public Vector3 GetFirstVertex()
    {
        if (ogVertices.Count == 0)
            throw new UnityException(
                "Error: Either the mesh has no vertices or GetFirstVertex was called before SliceMesh.");
        else
            return ogVertices[0];
    }

    #region Boundary fill method


    private void FillBoundaryGeneral(List<Vector3> added)
    {
        // 1. Reorder added so in order ot their occurence along the perimeter.
        MeshUtils.ReorderList(added);

        Vector3 center = MeshUtils.FindCenter(added);

        //Create triangle for each edge to the center
        tempTriangle[2] = center;

        for (int i = 0; i < added.Count; i += 2)
        {
            // Add fronface triangle in meshPositive
            tempTriangle[0] = added[i];
            tempTriangle[1] = added[i + 1];

            PositiveMesh.AddTriangle(tempTriangle);

            // Add backface triangle in meshNegative
            tempTriangle[0] = added[i + 1];
            tempTriangle[1] = added[i];

            NegativeMesh.AddTriangle(tempTriangle);
        }
    }


    private void FillBoundaryFace(List<Vector3> addedOuter, List<Vector3> addedInner)
    {
        if (addedOuter.Count == 0 || addedInner.Count == 0)
        {
            return;
        }
        // 1. Reorder added so in order ot their occurence along the perimeter.
        //MeshUtils.ReorderList(addedOuter);
        //MeshUtils.ReorderList(addedInner);


        var innerList2 = findneighbor(addedInner);
        var outerList2 = findneighbor(addedOuter);


        List<int> list;

        if (addedInner.Count > addedOuter.Count)
        {
            list = FindOrderForList(addedInner, addedOuter);
        }
        else
        {
            list = FindOrderForList(addedOuter, addedInner);
        }

        //------------------------
        List<int> list2;

        if (innerList2.Count > outerList2.Count)
        {
            list2 = FindOrderForList(innerList2, outerList2);
        }
        else
        {
            list2 = FindOrderForList(outerList2, innerList2);
        }
        //-----------------------------

        //Debug.Log(list.Count);

        // 2. Find actual face vertices
        var face = FindRealPolygon(addedOuter, addedInner, list);
        //face.AddRange(FindRealPolygon(outerList2, innerList2, list2));
        var face2 = FindRealPolygon(outerList2, innerList2, list2);

        // 3. Create triangle fans
        //int t_fwd = 0,
        //    t_bwd = face.Count - 1,
        //    t_new = 1;
        //bool incr_fwd = true;
        //while (t_new != t_fwd && t_new != t_bwd)
        //{
        //    AddTriangle(face, t_bwd, t_fwd, t_new);

        //    if (incr_fwd) t_fwd = t_new;
        //    else t_bwd = t_new;

        //    incr_fwd = !incr_fwd;
        //    t_new = incr_fwd ? t_fwd + 1 : t_bwd - 1;
        //}


        for (int i = 0; i < face.Count; i += 3)
        {
            AddTriangle(face, i, i + 1, i + 2);
        }

        for (int i = 0; i < face2.Count; i += 3)
        {
            AddTriangle(face2, i, i + 1, i + 2);
        }

        addedPairsInner.Clear();
        addedPairsOuter.Clear();

    }

    private List<Vector3> findneighbor(List<Vector3> list)
    {
        List<Vector3> morePairs = new List<Vector3>();

        for (int i = 1; i < list.Count; i+=2)
        {
            int match = 0;
            float MinDist = Mathf.Infinity;
            for (int j = 0; j < list.Count; j += 2)
            {

                if (j == i - 1 || list[i] == list[j])
                {
                    continue;
                }
                
                //Debug.Log(i +" "+j + " " + largerList.Count + " " + smallerList.Count);
                float dist = Vector3.Distance(list[i], list[j]);
                if (Vector3.Distance(list[i-1], list[j]) < dist)
                {
                    continue;
                }
                //Debug.Log(dist);
                if (dist < MinDist)
                {
                    MinDist = dist;
                    match = j;
                }
                
            }
            morePairs.Add(list[i]);
            morePairs.Add(list[match]);
            //Debug.Log(list[i] +" "+ list[match]);
        }

        return morePairs;
    }

    private List<int> FindOrderForList(List<Vector3> largerList, List<Vector3> smallerList)
    {
        List<int> closestPoint = new List<int>(new int[smallerList.Count]);
        //Debug.Log(closestPoint.Count);
        for (int i =0; i < smallerList.Count; i+=2)
        {
            float MinDist = Mathf.Infinity; 
            for (int j =0; j < largerList.Count; j+=2)
            {
                //Debug.Log(i +" "+j + " " + largerList.Count + " " + smallerList.Count);
                float dist = Vector3.Distance(smallerList[i], largerList[j]);
                //Debug.Log(dist);
                if (dist < MinDist)
                {
                    MinDist = dist;
                    closestPoint[i] = j;
                }
            }
            //Debug.Log("Final min"+MinDist);

        }

        //for (int i = 0; i < closestPoint.Count; i += 1)
            //Debug.Log(closestPoint[i]);
        return closestPoint;
    }

    /// <summary>
    /// Extract polygon from the pairs of vertices.
    /// Per example, two vectors that are colinear is redundant and only forms one side of the polygon
    /// </summary>
    private List<Vector3> FindRealPolygon(List<Vector3> pairsInner, List<Vector3> pairsOuter, List<int> list)
    {
        List<Vector3> vertices = new List<Vector3>();
        Vector3 edge1, edge2;

        int n;
        bool innerSmaller = false;

        if (pairsOuter.Count < pairsInner.Count)
        {
            n = pairsOuter.Count;
        }
        else
        {
            n = pairsInner.Count;
            innerSmaller = true;
        }

        // List should be ordered in the correct way
        for (int i = 0; i < n; i += 2)
        {
            //edge1 = (pairs[i + 1] - pairs[i]);
            //if (i == pairs.Count - 2)
            //    edge2 = pairs[1] - pairs[0];
            //else
            //    edge2 = pairs[i + 3] - pairs[i + 2];

            //// Normalize edges
            //edge1.Normalize();
            //edge2.Normalize();

            //if (Vector3.Angle(edge1, edge2) > threshold)
            //    // This is a corner
            //    vertices.Add(pairs[i + 1]);

            if (i + 1 < n)
            {
                Debug.Log(innerSmaller);
                if (innerSmaller == true)
                {
                    //Debug.Log(i + " " + list[i]);

                    vertices.Add(pairsOuter[list[i] + 1]);
                    vertices.Add(pairsInner[i + 1]);
                    vertices.Add(pairsOuter[list[i]]);

                    vertices.Add(pairsInner[i]);
                    vertices.Add(pairsInner[i + 1]);
                    vertices.Add(pairsOuter[list[i] + 1]);

                }
                else
                {
                    //Debug.Log(i + " " + list[i]);
                    vertices.Add(pairsOuter[i+1]);
                    vertices.Add(pairsInner[list[i]+1]);
                    vertices.Add(pairsOuter[i]);

                    vertices.Add(pairsInner[list[i]]);
                    vertices.Add(pairsInner[list[i] + 1]);
                    vertices.Add(pairsOuter[i + 1]);

                }
                
            }
        }

        return vertices;
    }

    private void AddTriangle(List<Vector3> face, int t1, int t2, int t3)
    {
        tempTriangle[0] = face[t1];
        tempTriangle[1] = face[t2];
        tempTriangle[2] = face[t3];
        PositiveMesh.AddTriangle(tempTriangle);

        tempTriangle[1] = face[t3];
        tempTriangle[2] = face[t2];
        NegativeMesh.AddTriangle(tempTriangle);
    }
    #endregion
    
}

