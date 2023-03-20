using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hierarchy : MonoBehaviour
{
    //public GameObject buttonModel;
    //public GameObject buttonCasing;
    FileExplorer fileExplorer;
    MeshCasing meshCasing;
    private GameObject handObj;
    private GameObject casingOuter;
    private GameObject casingInner;
    private bool hand = false;
    private bool casing = false;

    private void Start()
    {
        fileExplorer = GameObject.Find("FileManager").GetComponent<FileExplorer>();
        meshCasing = GameObject.Find("MeshEditor").GetComponent<MeshCasing>();
    }
    public void OnClickModel()
    {
        
        if (fileExplorer.handLoaded == true) {
            handObj = fileExplorer.model;
            if (hand == false)
            {

                handObj.SetActive(false);
                hand = true;
            }
            else
            {

                handObj.SetActive(true);
                hand = false;
            }
        }
    }

    public void OnClickCasing()
    {
        
        if (meshCasing.isCasingGenerated == true)
        {
            casingInner = meshCasing.CasingInner;
            casingOuter = meshCasing.CasingOuter;
            //casingObj = meshCasing.CasingPanel;
            if (casing == false)
            {

                casingInner.SetActive(false);
                casingOuter.SetActive(false);
                casing = true;
            }
            else
            {

                casingInner.SetActive(true);
                casingOuter.SetActive(true);
                casing = false;
            }
        }
    }
}
