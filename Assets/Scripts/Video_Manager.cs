using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProMovieCapture;
using UnityEngine.Video;

public class Video_Manager : MonoBehaviour
{
    [SerializeField]
    private RawImage recordFeed;

    [SerializeField]
    private RawImage outputFeed;

    [SerializeField]
    private UI_Manager uiManager;
    [SerializeField]
    private GameObject selectionUI;
    [SerializeField]
    private State_Manager stateMachine;

    [SerializeField]
    private float recordCountDown = 3.0f;
    [SerializeField]
    private float recordTimer = 5.0f;

    private bool recordingComplete = false;

    CaptureFromTexture captureDevice;


    private List<GameObject> videoSelections;

    private WebCamDevice[] deviceList;

    WebCamTexture webcam;

    private Dictionary<Toggle, bool> selectedVideos;

    private Dictionary<Toggle, GameObject> videoPreviews;


    [SerializeField]
    private string videoPath = @"G:\Users\RowanofRohan\Documents\NextNow\Projects\ARVideo Greenscreen Demo\Videos";
    [SerializeField]
    private string outputPath = @"G:\Users\RowanofRohan\Documents\NextNow\Projects\ARVideo Greenscreen Demo\Output";

    private DirectoryInfo videoFolder;
    private DirectoryInfo outputFolder;

    [SerializeField]
    GameObject blankFeedPrefab;
    [SerializeField]
    GameObject VideoStitcher;

    [SerializeField]
    private RenderTexture finalVideoTexture;
    [SerializeField]
    private RawImage originalVideoFeed;
    [SerializeField]
    private RenderTexture originalVideoTexture;

    [SerializeField]
    private List<VideoClip> videoClips;

    private List<RenderTexture> previewClips;

    void Start()
    {
        captureDevice = this.transform.GetComponent<CaptureFromTexture>();
        if(captureDevice == null) {Debug.LogError("Capture Device is NULL!");}
        
        videoFolder = new DirectoryInfo(videoPath);
        outputFolder = new DirectoryInfo(outputPath);

        InitializeSelections();
        SetSelectedVideos();
    }

    public void GetCameras()
    {
        deviceList = WebCamTexture.devices;
        for(int i = 0; i < deviceList.Length; i++)
        {
            //Debug.Log(deviceList[i].name);
        }
        webcam = new WebCamTexture();
        recordFeed.texture = webcam;
        recordFeed.material.mainTexture = webcam;
        webcam.deviceName = deviceList[0].name;
        webcam.Play();
        captureDevice.SetSourceTexture(webcam);
    }

    private void InitializeSelections()
    {
        selectedVideos = new Dictionary<Toggle, bool>();
        videoPreviews = new Dictionary<Toggle, GameObject>();

        int i = 0;

        foreach (Transform child in selectionUI.transform)
        {
            RawImage rawImage = child.GetChild(0).GetComponent<RawImage>();
            if(rawImage != null)
            {   
                Toggle toggle = child.GetComponent<Toggle>();
                if(toggle)
                {
                    VideoPlayer videoPlayer = child.GetComponent<VideoPlayer>();
                    if (videoPlayer != null)
                    {
                        videoPlayer.clip = videoClips[i];
                        i++;

                        RenderTexture newTexture = new RenderTexture(1920, 1080, 16);
                        videoPlayer.targetTexture = newTexture;
                        rawImage.texture = newTexture;
                        rawImage.material.mainTexture = newTexture;
                    }

                    toggle.isOn = false;
                    selectedVideos.Add(toggle, false);
                    GameObject videoLayer = Instantiate(blankFeedPrefab, VideoStitcher.transform);
                    RawImage layerImage = videoLayer.GetComponent<RawImage>();
                    layerImage.texture = rawImage.texture;
                    layerImage.material.mainTexture = rawImage.material.mainTexture;
                    //videoLayer.width = 1920;
                    //videoLayer.height = 1080;
                    videoPreviews.Add(toggle, videoLayer);
                }
            }
        }
    }

    public void UpdateSelections()
    {
        foreach (Transform child in selectionUI.transform)
        {
            Toggle toggle = child.GetComponent<Toggle>();
            if(toggle)
            {
                selectedVideos[toggle] = toggle.isOn;
            }
        }
        SetSelectedVideos();
    }

    private void SetSelectedVideos()
    {
        foreach (KeyValuePair<Toggle, bool> item in selectedVideos)
        {
            videoPreviews[item.Key].SetActive(item.Value);
        }
    }

    public void ClearOptions()
    {
        //Clears recorded file
        //Clears selected videos
    }

    public void StartRecording()
    {
        //uiManager.SetRecordText("Looking for Camera...");
        StartCoroutine(RecordingTimer());
    }

    public void MakeFinal()
    {
        //ON VIDEO MANAGER:
        //Set target texture as original feed
        //pull original recording from filepath
        FileInfo videoFile = null;

        foreach(FileInfo vidFile in videoFolder.GetFiles())
        {
            if(videoFile == null || vidFile.LastWriteTime > videoFile.LastWriteTime)
            {
                videoFile = vidFile;
            }
        }

        //this.transform.GetComponent<VideoPlayer>().url = videoFile.FullPath;
        //this.transform.GetComponent<VideoPlayer>().url = "file://" + videoFile.Name;
        this.transform.GetComponent<VideoPlayer>().url = @"file://G:\Users\RowanofRohan\Documents\NextNow\Projects\ARVideo Greenscreen Demo\Assets\Videos\2017-09-18 11-12-04.mp4";

        this.transform.GetComponent<VideoPlayer>().Play();

        //JUST FOR DEBUGGING
        recordingComplete = true;

        if(recordingComplete == false)
        {
            Debug.LogError("Must record a source video first!");
            stateMachine.SetState("Menu");
        }
        else
        {
            originalVideoFeed.texture = originalVideoTexture;
            originalVideoFeed.material.mainTexture = originalVideoTexture;
            captureDevice.SetSourceTexture(finalVideoTexture);

            //Remember to set capture device FILE NAME and OUTPUT FOLDER here

        }

    }

    private IEnumerator RecordingTimer()
    {
        float timeLeft = recordCountDown;

        while(timeLeft > 0.0f)
        {
            uiManager.SetRecordText(Mathf.RoundToInt(timeLeft) + "...");
            timeLeft -= 1.0f;
            yield return new WaitForSeconds(1.0f);
        }

        uiManager.SetRecordText("Recording...");
        
        captureDevice.StartCapture();

        yield return new WaitForSeconds(recordTimer);

        captureDevice.StopCapture();


        uiManager.SetRecordText("Saving...");

        yield return new WaitForSeconds(1.0f);

        LeaveRecording();
    }

    private IEnumerator FinalRecording()
    {
        captureDevice.StartCapture();

        yield return new WaitForSeconds(recordTimer);

        captureDevice.StopCapture();

        stateMachine.SetState("Thanks");
    }

    public void LeaveRecording()
    {
        webcam.Stop();
        stateMachine.SetState("Menu");
    }
}
