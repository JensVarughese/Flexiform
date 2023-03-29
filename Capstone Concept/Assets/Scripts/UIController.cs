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

    Button generatePanelButton;
    Button cutButton;
    Button socketButton;
    Button exportHandButton;

    VisualElement casingPanel;
    VisualElement socketSelectionPanel;
    VisualElement socketTransformPanel;
    Slider thicknessSlider;

    // Start is called before the first frame update
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button loadHandButton = root.Q<Button>("load-hand");

        generatePanelButton = root.Q<Button>("generate-casing");
        Button cancelCasingButton = root.Q<Button>("cancel-casing");
        Button generateCasingButton = root.Q<Button>("generate-button");
        cutButton = root.Q<Button>("cut");
        socketButton = root.Q<Button>("socket");
        exportHandButton = root.Q<Button>("export-hand");

        generatePanelButton.SetEnabled(false);
        cutButton.SetEnabled(false);
        socketButton.SetEnabled(false);
        exportHandButton.SetEnabled(false);
        thicknessSlider = root.Q<Slider>("thickness-slider");
        thicknessSlider.label = "Thickness: " + thicknessSlider.value + "mm";
        casingPanel = root.Q<VisualElement>("casing-panel");
        thicknessSlider.RegisterValueChangedCallback(v => OnSliderChange(v));

        // socket selection panel
        socketSelectionPanel = root.Q<VisualElement>("socket-selection-panel");
        DropdownField socketSelectDropDown = root.Q<DropdownField>("socket-selection");
        Button socketSelectButton = root.Q<Button>("socket-selection-button");
        socketTransformPanel = root.Q<VisualElement>("socket-transform-panel");
        socketSelectButton.clicked += () => SelectSocket(socketSelectDropDown.value);

        // socket panel
        Button positionModeButton = root.Q<Button>("position-selection");
        Button rotationModeButton = root.Q<Button>("rotation-selection");
        Button addSocketButton = root.Q<Button>("add-socket");
        positionModeButton.clicked += () => transformController.ChangeHandleType(HandleType.POSITION);
        rotationModeButton.clicked += () => transformController.ChangeHandleType(HandleType.ROTATION);
        addSocketButton.clicked += () => AddSocket();
        

        loadHandButton.clicked += () => loadHandButtonPressed();
        generatePanelButton.clicked += () => generateButtonPressed();
        cancelCasingButton.clicked += () => CancelCasing();
        generateCasingButton.clicked += () => GenerateCasing();
        cutButton.clicked += () => cutButtonPressed();
        socketButton.clicked += () => socketButtonPressed();
        exportHandButton.clicked += () => exportHandButtonPressed();
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
        if(Input.GetKeyUp(KeyCode.C))
        {
            screenLineRenderer.EnableSlicing();
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

    void loadHandButtonPressed() 
    {
        // do load hand code
        fileExplorer.OpenFileBrowser();
        generatePanelButton.SetEnabled(true);
    }

    void OnSliderChange(ChangeEvent<float> v)
    {
        thicknessSlider.label = "Thickness: " + v.newValue.ToString("n2") + "mm";
        meshCasing.thicknessInMillimeters = v.newValue;
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
        cutButton.SetEnabled(true);
        socketButton.SetEnabled(true);
        exportHandButton.SetEnabled(true);
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
