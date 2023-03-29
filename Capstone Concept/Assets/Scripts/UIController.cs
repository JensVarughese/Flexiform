using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RuntimeHandle;

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
        if(Input.GetKeyUp(KeyCode.W)) {
            transformController.ChangeHandleType(HandleType.POSITION);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            transformController.ChangeHandleType(HandleType.ROTATION);
        }
    }

    void SelectSocket(string selection)
    {
        var camCenter = cameraController.GetCameraPostion();
        var position = (cameraController.handCenter * 0.75f) + (camCenter * 0.25f);
        transformController.LoadSocket(selection, position);
        socketSelectionPanel.style.display = DisplayStyle.None;
        socketTransformPanel.style.display = DisplayStyle.Flex;
    }

    void AddSocket()
    {
        meshCasing.IntegrateSocket(transformController.SelectedSocket, transformController.runtimeTransformGameObj);
        socketTransformPanel.style.display = DisplayStyle.None;
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
