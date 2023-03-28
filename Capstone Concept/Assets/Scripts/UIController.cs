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
    public TransformController transformController;

    VisualElement casingPanel;
    Slider thicknessSlider;

    Slider xSlider;
    Slider ySlider;
    Slider zSlider;
    const int positionRange = 100;
    bool isPositionChanging = false;

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

        // handle Sliders
        xSlider = root.Q<Slider>("x-slider");
        ySlider = root.Q<Slider>("y-slider");
        zSlider = root.Q<Slider>("z-slider");
        UpdatePositionSliders();
        xSlider.RegisterValueChangedCallback(v => OnPositionChange(v, xSlider));
        ySlider.RegisterValueChangedCallback(v => OnPositionChange(v, ySlider));
        zSlider.RegisterValueChangedCallback(v => OnPositionChange(v, zSlider));

        loadHandButton.clicked += () => loadHandButtonPressed();
        generatePanelButton.clicked += () => generateButtonPressed();
        cancelCasingButton.clicked += () => CancelCasing();
        generateCasingButton.clicked += () => GenerateCasing();
        cutButton.clicked += () => cutButtonPressed();
        exportHandButton.clicked += () => exportHandButtonPressed();
        thicknessSlider.RegisterValueChangedCallback(v => OnSliderChange(v));

    }

    void Update()
    {
        if(isPositionChanging && Input.GetMouseButtonUp(0)) {
            UpdatePositionSliders();
        }
    }

    void UpdatePositionSliders()
    {
        var position = transformController.GetPosition();
        xSlider.highValue = position.x + positionRange;
        xSlider.lowValue = position.x - positionRange;
        xSlider.value = position.x;

        ySlider.highValue = position.y + positionRange;
        ySlider.lowValue = position.y - positionRange;
        ySlider.value = position.y;

        ySlider.highValue = position.y + positionRange;
        ySlider.lowValue = position.y - positionRange;
        ySlider.value = position.y;

        zSlider.highValue = position.z + positionRange;
        zSlider.lowValue = position.z - positionRange;
        zSlider.value = position.z;

        isPositionChanging = false;
    }
    void OnPositionChange(ChangeEvent<float> v, Slider slider) {
        isPositionChanging = true;
        if(slider.value / slider.highValue > 0.95f) 
            slider.highValue++;
        if(slider.lowValue / slider.value > 0.95f) 
            slider.lowValue--;
        transformController.ChangePosition(new Vector3(xSlider.value, ySlider.value, zSlider.value));
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
        fileExporter.onClickSave(ExportType.Stl);
    }

}
