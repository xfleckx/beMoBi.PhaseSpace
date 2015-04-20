using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

/// <summary>
/// Deprecated class
/// </summary>
[Obsolete]
public class PhaseSpaceController : MonoBehaviour {

    public int TrackerID;
    public int TrackingIndex;

    public int ExpectedLEDCount = 3; 
    public string OWLHost = "192.168.1.81";
    public bool isSlave = false;

    public bool AcceptOWLData = false;

    public int AvailableMarkers; 
    public Vector3 MarkerPosition;

    public Vector3 FirstMarkerUpdate;

    public GameObject UpdateTarget;

    private const string MarkerNotAvailableFormat = "Expected Marker {0} not available";

    public List<PSMarker> markerList = new List<PSMarker>();
    public List<Vector3> markerVectorList = new List<Vector3>();

    private Queue<Vector3> smoothingWindow = new Queue<Vector3>();
    public int SmoothingWindowSize = 4;

    public OWLWrapper OWL = new OWLWrapper();

    public OWLInterface tracker;

    public bool WriteToLSL = true;
    private LSL.liblsl.StreamOutlet lslOutlet;
    private LSL.liblsl.StreamInfo lslStreamInfo;

    private Stopwatch stopWatch = new Stopwatch();

    public float OWLUpdateTime = 0f;

    private const int lslChannelCount = 6; // PositionVector, Speed, Acceleration, OWL update time
    
    private float[] lslSample = new float[lslChannelCount];

    private const string lslStreamName = "Unity_PhaseSpace_Client";
    private const string lslStreamType = "PhaseSpace_Coordinates";

    void Awake()
    {
        print("Creating OWLTracker...");

        if (OWL.Connect(OWLHost, isSlave)) {
            print("Connecting to " + OWLHost); 
            int[] leds = new int[ExpectedLEDCount];
            for (int i = 0; i < ExpectedLEDCount; i++)
                leds[i] = i;
            OWL.CreatePointTracker(TrackerID, leds);
        }

    }

    void Start()
    {
        lslStreamInfo = new LSL.liblsl.StreamInfo(
            lslStreamName,
            lslStreamType, 
            lslChannelCount, 
            Time.fixedTime,
            LSL.liblsl.channel_format_t.cf_float32);

        lslOutlet = new LSL.liblsl.StreamOutlet(lslStreamInfo);


        for (int i = 0; i < SmoothingWindowSize; i++)
        {
            smoothingWindow.Enqueue(Vector3.one);
        }
        
        if (UpdateTarget == null)
        {
            UnityEngine.Debug.LogError("No update target for PhaseSpace data");
        }

        if (tracker == null) { 
            UnityEngine.Debug.LogError("No tracker instance available!");
            return;

        }

        tracker.OwlUpdateCallbacks += ProcessOwlUpdate;
    }

    void ProcessOwlUpdate()
    { 
        UpdateMarkerList();

        if (WriteToLSL)
        {
            var chunk = GenerateChunkFromMarkerList();

            lslOutlet.push_chunk(chunk);
        }
    } 

    private float[,] GenerateChunkFromMarkerList()
    {
        float[,] chunk = new float[AvailableMarkers, lslChannelCount];

        for (int i = 0; i < AvailableMarkers; i++)
        {
            var marker = markerList[i];

            chunk[i, 0] = marker.position.x;
            chunk[i, 1] = marker.position.y;
            chunk[i, 2] = marker.position.z;

            chunk[i, 3] = OWLUpdateTime;
        }

        return chunk;
    }

    private void UpdateMarkerList()
    {
        AvailableMarkers = OWL.NumMarkers;

        for (int i = 0; i < AvailableMarkers; i++)
        {
            PSMarker marker = OWL.GetMarker(TrackerID, i);

            if (marker != null)
            {
                if (i >= markerList.Count)
                {
                    markerList.Add(marker);
                    markerVectorList.Add(marker.position);
                }
                else
                {
                    markerList[i] = marker;
                    markerVectorList[i] = marker.position;
                }
            }
            else
            {
                UnityEngine.Debug.LogWarningFormat(MarkerNotAvailableFormat, i);
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.O)) AcceptOWLData = !AcceptOWLData;

        if (OWL.Connected() && AcceptOWLData)
        {   
            var center = CenterOfVectors(markerVectorList.ToArray());

            if (smoothingWindow.Count == SmoothingWindowSize){
                smoothingWindow.Dequeue();
                smoothingWindow.Enqueue(center);
            }
            else if(smoothingWindow.Count < SmoothingWindowSize)
            {
                smoothingWindow.Enqueue(center);
            }
            else
            {
                while(smoothingWindow.Count > SmoothingWindowSize){
                    smoothingWindow.Dequeue();
                }
            }

            var smoothedPosition = CenterOfVectors(this.smoothingWindow);
            
            MarkerPosition = smoothedPosition;

            UpdateTarget.transform.position = MarkerPosition;
        } 
    }

    //
    void OnDestroy()
    {
        // disconnect from OWL server
        OWL.Disconnect();

    } 

    public static Vector3 CenterOfVectors(IEnumerable<Vector3> vectors)
    {
        float sumX = 0;
        float sumY = 0;
        float sumZ = 0;

        float n = vectors.Count();

        foreach (var v in vectors)
        {
            sumX += v.x;
            sumY += v.y;
            sumZ += v.z;
        }

        return new Vector3(sumX / n, sumY / n, sumZ / n);
    }

}
