using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Manager : MonoBehaviour
{
    
    [SerializeField]
    private float idleTimeout = 30.0f;

    private float timeWaited = 0.0f;

    private enum StateChart {Idle, Menu, Recording, Selection, Final, Thanks, Output}

    private StateChart currentState;

    private Dictionary<string, StateChart> stringConversion;

    [SerializeField]
    private UI_Manager uiManager;

    [SerializeField]
    private Video_Manager videoManager;

    void Start()
    {
        stringConversion = new Dictionary<string, StateChart>();
        foreach (int state in Enum.GetValues(typeof(StateChart)))
        {
            stringConversion.Add(Enum.GetName(typeof(StateChart), state), (StateChart)state);
        }

        if(uiManager == null) {Debug.LogError("Could not find UI Manager!");}

        SetState("Idle");
    }

    void Update()
    {
        if(Input.anyKey)
        {
            timeWaited = 0.0f;
            if(GetState() == "Idle")
            {
                SetState("Menu");
            }
        }
        else
        {
            timeWaited += Time.deltaTime;
            if(timeWaited >= idleTimeout) {SetState("Idle");}
        }
    }

    public void SetState(string newState)
    {
        currentState = stringConversion[newState];
        uiManager.SetUIState(newState);
        ChangeState();
    }

    public string GetState()
    {
        return (currentState.ToString());
    }

    private void ChangeState()
    {
        switch(currentState)
        {
            case StateChart.Idle:
                videoManager.ClearOptions();
                break;

            case StateChart.Menu:
                //Go to Menu
                break;

            case StateChart.Recording:
                videoManager.GetCameras();
                videoManager.StartRecording();
                break;

            case StateChart.Selection:
                //Go to Video Selection
                break;

            case StateChart.Final:
                //Go to Final Display
                break;

            case StateChart.Thanks:
                //Go to Thank You Page
                break;

            case StateChart.Output:
                //Go to Output Panel
                break;

            default:
                break;
        }
    }

    public void SCRAM()
    {
        Application.Quit();
    }
}
