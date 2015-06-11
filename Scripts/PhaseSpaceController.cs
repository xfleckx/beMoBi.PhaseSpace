using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

/// <summary>
/// Deprecated class
/// </summary>
public class PhaseSpaceController : MonoBehaviour {

    public int TrackerID;
    public int TrackingIndex;

    public OWLInterface tracker;

    public bool AcceptOWLData = false;

    public Vector3 MarkerPosition;

    public Vector3 FirstMarkerUpdate;

    public GameObject UpdateTarget;

    private const string MarkerNotAvailableFormat = "Expected Marker {0} not available";

    public int AvailableMarker; 
    public List<PSMarker> Marker = new List<PSMarker>();

    public List<Vector3> markerVectorList = new List<Vector3>();

    private Queue<Vector3> smoothingWindow = new Queue<Vector3>();
    public int SmoothingWindowSize = 4;

    public OWLWrapper OWL = new OWLWrapper();

    private Stopwatch stopWatch = new Stopwatch();
    
    void Start()
    {

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
    } 

    private void UpdateMarkerList()
    {
        AvailableMarker = OWL.NumMarkers;

        for (int i = 0; i < AvailableMarker; i++)
        {
            PSMarker marker = OWL.GetMarker(TrackerID, i);

            if (marker != null)
            {
                if (i >= Marker.Count)
                {
                    Marker.Add(marker);
                    markerVectorList.Add(marker.position);
                }
                else
                {
                    Marker[i] = marker;
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
