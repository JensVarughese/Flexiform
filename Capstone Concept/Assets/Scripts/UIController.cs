using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//import other classes

public class UIController : MonoBehaviour
{
    public FileExplorer fe;
    public MeshCasing mc;
    public ScreenLineRenderer slr;

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button loadHandButton = root.Q<Button>("load-hand");
        Button generateButton = root.Q<Button>("generate-casing");
        Button cutButton = root.Q<Button>("cut");
        Button exportHandButton = root.Q<Button>("export-hand");

        loadHandButton.clicked += () => loadHandButtonPressed();
        generateButton.clicked += () => generateButtonPressed();
        cutButton.clicked += () => cutButtonPressed();
        exportHandButton.clicked += () => exportHandButtonPressed();
        
    }

    void loadHandButtonPressed() 
    {
        // do load hand code
        Debug.Log("Loading Hand..");
        fe.OpenFileBrowser();
    }

    void generateButtonPressed()
    {
        // do generate casing code
        Debug.Log("Generating Casing..");
        mc.openCasingPanel();
    }

    void cutButtonPressed()
    {
        // do mesh cutting code
        Debug.Log("Cutting..");
        slr.EnableSlicing();
    }

    void exportHandButtonPressed()
    {
        // do exporting code
        Debug.Log("Exporting Hand..");
    }

}
