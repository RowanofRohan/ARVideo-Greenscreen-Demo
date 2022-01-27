using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class Video_Manager : MonoBehaviour
{
    [SerializeField]
    private RawImage recordFeed;

    [SerializeField]
    private RawImage outputFeed;

    [SerializeField]
    private GameObject selectionUI;

    [SerializeField]
    private UI_Manager uiManager;
    [SerializeField]
    private State_Manager stateMachine;

    [SerializeField]
    private float recordCountDown = 3.0f;
    [SerializeField]
    private float recordTimer = 5.0f;


    private List<GameObject> videoSelections;

    private WebCamDevice[] deviceList;

    private VideoCapture videoCapture = null;

    private bool isRecording = false;

    public void GetCameras()
    {
        deviceList = WebCamTexture.devices;
        for(int i = 0; i < deviceList.Length; i++)
        {
            //Debug.Log(deviceList[i].name);
        }
        WebCamTexture webcam = new WebCamTexture();
        recordFeed.texture = webcam;
        recordFeed.material.mainTexture = webcam;
        webcam.deviceName = deviceList[0].name;
        webcam.Play();
    }

    public void ClearOptions()
    {
        //Clears recorded file
        //Clears selected videos
    }

    public void StartRecording()
    {
        StartCoroutine(RecordingTimer());
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

        VideoCapture.CreateAsync(false, delegate(VideoCapture videoCap)
        {
            if(videoCap != null)
            {
                Resolution cameraResolution = VideoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
                
                float cameraFramerate = VideoCapture.GetSupportedFrameRatesForResolution(cameraResolution).OrderByDescending((fps) => fps).First();

                videoCapture = videoCap;
                CameraParameters cameraParameters = new CameraParameters();
                cameraParameters.hologramOpacity = 0.0f;
                cameraParameters.frameRate = cameraFramerate;
                cameraParameters.cameraResolutionWidth = cameraResolution.width;
                cameraParameters.cameraResolutionHeight = cameraResolution.height;
                cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;

                videoCapture.StartVideoModeAsync(cameraParameters, VideoCapture.AudioState.None, OnStartedVideoCaptureMode);
            }
            else
            {
                Debug.LogError("Failed to Start Video Capture!");
            }
        });

        while(isRecording == false)
        {
            yield return null;
        }

        yield return new WaitForSeconds(recordTimer);

        isRecording = false;
        if(videoCapture != null)
        {
            videoCapture.StopRecordingAsync(OnStoppedRecordingVideo);
        }

        uiManager.SetRecordText("Saving...");
    }

    void OnStartedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {
        string filepath = System.IO.Path.Combine(Application.persistentDataPath, "Original");
        filepath = filepath.Replace("/", @"\");
        videoCapture.StartRecordingAsync(filepath, OnStartedRecordingVideo);
    }

    void OnStoppedVideoCaptureMode(VideoCapture.VideoCaptureResult result)
    {

        videoCapture.Dispose();
        videoCapture = null;
        stateMachine.SetState("Menu");
    }

    void OnStartedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        isRecording = true;
    }

    void OnStoppedRecordingVideo(VideoCapture.VideoCaptureResult result)
    {
        videoCapture.StopVideoModeAsync(OnStoppedVideoCaptureMode);
    }



}
