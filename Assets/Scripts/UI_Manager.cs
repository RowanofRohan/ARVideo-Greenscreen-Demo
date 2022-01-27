using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [SerializeField]
    private State_Manager stateMachine;

    private Dictionary<string, GameObject> canvasNames;

    [SerializeField]
    private Text recordingText;

    void Start()
    {
        canvasNames = new Dictionary<string, GameObject>();
        foreach (Transform child in this.transform)
        {
            canvasNames.Add(child.name, child.gameObject);
        }

        SetUIState("Idle");
    }

    public void SetUIState(string newState)
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
        canvasNames[newState].SetActive(true);
    }

    public void SetRecordText(string newText)
    {
        recordingText.text = newText;
    }
}
