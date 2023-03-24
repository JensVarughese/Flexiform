using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//import other classes

public class UIController : MonoBehaviour
{
    public FileExplorer fileExplorer;
    public FileExporter fileExporter;
    public MeshCasing meshCasing;
    public ScreenLineRenderer screenLineRenderer;

    VisualElement casingPanel;
    Slider thicknessSlider;

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button loadHandButton = root.Q<Button>("load-hand");

        Button generatePanelButton = root.Q<Button>("generate-casing");
        Button cancelCasingButton = root.Q<Button>("cancel-casing");
        Button generateCasingButton = root.Q<Button>("generate-button");

        Button cutButton = root.Q<Button>("cut");
        Button exportHandButton = root.Q<Button>("export-hand");
        thicknessSlider = root.Q<Slider>("thickness-slider");
        thicknessSlider.label = "Thickness: " + thicknessSlider.value + "mm";
        casingPanel = root.Q<VisualElement>("casing-panel");

        loadHandButton.clicked += () => loadHandButtonPressed();
        generatePanelButton.clicked += () => generateButtonPressed();
        cancelCasingButton.clicked += () => CancelCasing();
        generateCasingButton.clicked += () => GenerateCasing();
        cutButton.clicked += () => cutButtonPressed();
        exportHandButton.clicked += () => exportHandButtonPressed();
        thicknessSlider.RegisterValueChangedCallback(v => OnSliderChange(v));

    }

    void OnSliderChange(ChangeEvent<float> v)
    {
        thicknessSlider.label = "Thickness: " + v.newValue.ToString("n2") + "mm";
        meshCasing.thicknessInMillimeters = v.newValue;
    }

    void loadHandButtonPressed() 
    {
        // do load hand code
        fileExplorer.OpenFileBrowser();
    }

    void generateButtonPressed()
    {
        // do generate casing code
        //meshCasing.openCasingPanel();
        casingPanel.style.display = DisplayStyle.Flex;
        thicknessSlider.label = "Thickness: " + meshCasing.thicknessInMillimeters.ToString("n2") + "mm";
    }

    void CancelCasing()
    {
        casingPanel.style.display = DisplayStyle.None;
    }

    void GenerateCasing()
    {
        casingPanel.style.display = DisplayStyle.None;
        meshCasing.generateCasing();
    }

    void cutButtonPressed()
    {
        // do mesh cutting code
        screenLineRenderer.EnableSlicing();
    }

    void exportHandButtonPressed()
    {
        // do exporting code
        Debug.Log("Exporting Hand..");
        fileExporter.onClickSave(ExportType.Obj);
    }

}
