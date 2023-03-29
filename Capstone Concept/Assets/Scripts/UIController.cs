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
    public CameraController cameraController;

    VisualElement casingPanel;
    VisualElement socketSelectionPanel;
    VisualElement socketTransformPanel;
    Slider thicknessSlider;

    Slider xSlider;
    Slider ySlider;
    Slider zSlider;
    Slider xRotSlider;
    Slider yRotSlider;
    Slider zRotSlider;

    const int positionRange = 50;
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
        Button socketButton = root.Q<Button>("socket");
        Button exportHandButton = root.Q<Button>("export-hand");
        thicknessSlider = root.Q<Slider>("thickness-slider");
        thicknessSlider.label = "Thickness: " + thicknessSlider.value + "mm";
        casingPanel = root.Q<VisualElement>("casing-panel");

        // socket selection panel
        socketSelectionPanel = root.Q<VisualElement>("socket-selection-panel");
        DropdownField socketSelectDropDown = root.Q<DropdownField>("socket-selection");
        Button socketSelectButton = root.Q<Button>("socket-selection-button");
        socketTransformPanel = root.Q<VisualElement>("socket-transform-panel");
        socketSelectButton.clicked += () => SelectSocket(socketSelectDropDown.value);

        // socket panel
        xSlider = root.Q<Slider>("x-slider");
        ySlider = root.Q<Slider>("y-slider");
        zSlider = root.Q<Slider>("z-slider");
        UpdatePositionSliders();
        xSlider.RegisterValueChangedCallback(v => OnPositionChange(v, xSlider));
        ySlider.RegisterValueChangedCallback(v => OnPositionChange(v, ySlider));
        zSlider.RegisterValueChangedCallback(v => OnPositionChange(v, zSlider));
        xRotSlider = root.Q<Slider>("x-rot-slider");
        yRotSlider = root.Q<Slider>("y-rot-slider");
        zRotSlider = root.Q<Slider>("z-rot-slider");
        xRotSlider.RegisterValueChangedCallback(v => OnRotationChange());
        yRotSlider.RegisterValueChangedCallback(v => OnRotationChange());
        zRotSlider.RegisterValueChangedCallback(v => OnRotationChange());
        Button addSocketButton = root.Q<Button>("add-socket");
        addSocketButton.clicked += () => AddSocket();

        loadHandButton.clicked += () => loadHandButtonPressed();
        generatePanelButton.clicked += () => generateButtonPressed();
        cancelCasingButton.clicked += () => CancelCasing();
        generateCasingButton.clicked += () => GenerateCasing();
        cutButton.clicked += () => cutButtonPressed();
        socketButton.clicked += () => socketButtonPressed();
        exportHandButton.clicked += () => exportHandButtonPressed();
        thicknessSlider.RegisterValueChangedCallback(v => OnSliderChange(v));
    }

    void Update()
    {
        if(isPositionChanging && Input.GetMouseButtonUp(0)) {
            UpdatePositionSliders();
        }
    }

    void SelectSocket(string selection)
    {
        var camCenter = cameraController.GetCameraPostion();
        var position = (cameraController.handCenter * 0.75f) + (camCenter * 0.25f);
        transformController.LoadSocket(selection, position);
        UpdatePositionSliders();
        UpdateRotationSliders();
        socketSelectionPanel.style.display = DisplayStyle.None;
        socketTransformPanel.style.display = DisplayStyle.Flex;
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
    void UpdateRotationSliders()
    {
        var rotation = transformController.GetEulers();
        xRotSlider.highValue = -360;
        xRotSlider.lowValue = 360;
        xRotSlider.value = rotation.x;
        yRotSlider.highValue = -360;
        yRotSlider.lowValue = 360;
        yRotSlider.value = rotation.y;
        zRotSlider.highValue = -360;
        zRotSlider.lowValue = 360;
        zRotSlider.value = rotation.z;
    }

    void OnPositionChange(ChangeEvent<float> v, Slider slider) {
        isPositionChanging = true;
        if(slider.value / slider.highValue > 0.95f) 
            slider.highValue++;
        if(slider.lowValue / slider.value > 0.95f) 
            slider.lowValue--;
        transformController.ChangePosition(new Vector3(xSlider.value, ySlider.value, zSlider.value));
    }

    void AddSocket()
    {
        meshCasing.IntegrateSocket(transformController.SelectedSocket);
        socketTransformPanel.style.display = DisplayStyle.None;
    }

    void OnRotationChange()
    {
        var rotation = Quaternion.Euler(xRotSlider.value, yRotSlider.value, zRotSlider.value);
        transformController.ChangeRotation(rotation);
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

    void socketButtonPressed()
    {
        if(socketSelectionPanel.style.display != DisplayStyle.Flex)
            socketSelectionPanel.style.display = DisplayStyle.Flex;
        else
            socketSelectionPanel.style.display = DisplayStyle.None;
    }

    void exportHandButtonPressed()
    {
        // do exporting code
        Debug.Log("Exporting Hand..");
        fileExporter.onClickSave(ExportType.Stl);
    }

}
