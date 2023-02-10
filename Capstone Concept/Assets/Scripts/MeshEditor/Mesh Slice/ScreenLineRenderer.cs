using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenLineRenderer : MonoBehaviour {

    // Line Drawn event handler
    public delegate void LineDrawnHandler(Vector3 begin, Vector3 end, Vector3 depth);
    public event LineDrawnHandler OnLineDrawn;

    bool dragging;
    Vector3 start;
    Vector3 end;
    Camera cam;

    public Material lineMaterial;
    bool canSlice = false;

    public void EnableSlicing() {
        canSlice = true;
    }

    // Use this for initialization
    void Start () {
        cam = Camera.main;
        dragging = false;
    }

    private void OnEnable()
    {
        Camera.onPostRender += PostRenderDrawLine;
    }

    private void OnDisable()
    {
        Camera.onPostRender -= PostRenderDrawLine;
    }

    // Update is called once per frame
    void Update () {
        if(!canSlice)
            return;
        
        if (!dragging && Input.GetMouseButtonDown(0))
        {
            start = cam.ScreenToViewportPoint(Input.mousePosition);
            dragging = true;
        }

        if (dragging)
        {
            end = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (dragging && Input.GetMouseButtonUp(0))
        {
            // Finished dragging. We draw the line segment
            end = cam.ScreenToViewportPoint(Input.mousePosition);
            dragging = false;

            var startRay = cam.ViewportPointToRay(start);
            var endRay = cam.ViewportPointToRay(end);

            canSlice = false;
            // Raise OnLineDrawnEvent
            OnLineDrawn?.Invoke(
                startRay.GetPoint(cam.nearClipPlane),
                endRay.GetPoint(cam.nearClipPlane),
                endRay.direction.normalized);
        }
    }
    

    /// <summary>
    /// Draws the line in viewport space using start and end variables
    /// </summary>
    private void PostRenderDrawLine(Camera cam)
    {
        var thickness = 0.006f;
        if (dragging && lineMaterial)
        {
            GL.PushMatrix();
            lineMaterial.SetPass(0);
            GL.LoadOrtho();
            GL.Begin(GL.QUADS);
            GL.Color(Color.black);
            GL.Vertex(new Vector3(start.x, start.y - thickness / 2.0f, start.z));
            GL.Vertex(new Vector3(start.x, start.y + thickness / 2.0f, start.z));
            GL.Vertex(new Vector3(end.x, end.y + thickness / 2.0f, end.z));
            GL.Vertex(new Vector3(end.x, end.y - thickness / 2.0f, end.z));

            GL.Vertex(new Vector3(start.x - thickness / 2.0f, start.y, start.z));
            GL.Vertex(new Vector3(start.x + thickness / 2.0f, start.y, start.z));
            GL.Vertex(new Vector3(end.x + thickness / 2.0f, end.y, end.z ));
            GL.Vertex(new Vector3(end.x - thickness / 2.0f, end.y, end.z ));
            GL.End();
            GL.PopMatrix();
        }
    }
}
