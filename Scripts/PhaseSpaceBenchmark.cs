using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class PhaseSpaceBenchmark : MonoBehaviour
{

	public OWLInterface tracker;

	public BenchmarkPresenter activeBenchmarkPresenter;

	public GameObject samplePrototype;

	public BenchmarkIO benchmarkio;

	public bool recording;

	[SerializeField]
	public int minAveraginCount;

	[SerializeField]
	public int expectedMarkerCount;

	public Queue<PSMarker[]> markerQueue = new Queue<PSMarker[]>();

	private PSMarker[] latestMarker;

	private BenchmarkSample latestAvgResult;

	public float previewRadius = 0.01f;
	private int availableCameras;

	void Awake()
	{
		Debug.Log("adding listener");
		BenchmarkIO.OnBenchmarkModelLoaded += BenchmarkIO_OnBenchmarkModelLoaded;
	}

	void BenchmarkIO_OnBenchmarkModelLoaded(BenchmarkModel loadedModel)
	{
		var loadedBenchmark = GameObject.Instantiate<BenchmarkPresenter>(activeBenchmarkPresenter);
		loadedBenchmark.model = loadedModel;
		loadedBenchmark.RenderModel();
	}

	// Use this for initialization
	void Start () {

		markerQueue = new Queue<PSMarker[]>(minAveraginCount);

		if (tracker != null) { 
			tracker.OwlUpdateCallbacks += ProcessOwlUpdate;
		}
		else
		{
			throw new MissingReferenceException("OWL Tracker Interface Missing!");
		}
	}

	void ProcessOwlUpdate()
	{
		var marker = tracker.OWL.GetMarkers();

		latestMarker = marker;
		
		markerQueue.Enqueue(marker);
	}
	
	// Update is called once per frame
	void Update () {
		 
		if (markerQueue.Count >= minAveraginCount) {

			var avgPosition = AveragePosition(markerQueue.ToList());

			markerQueue.Clear();

			latestAvgResult = avgPosition;
			
			var currentSample = UpdatePreviewSample(avgPosition);

			if (recording)
				AppendToBenchmark(currentSample, avgPosition);
		}

	}

	GameObject UpdatePreviewSample(BenchmarkSample sample)
	{ 
		samplePrototype.transform.position = sample.position;
		var benchmarkColor = activeBenchmarkPresenter.model.color;
		samplePrototype.GetComponent<Renderer>().material.color = new Color(benchmarkColor.r, benchmarkColor.g, benchmarkColor.b, sample.condition / availableCameras);

		return samplePrototype;
	}

	void AppendToBenchmark(GameObject preview, BenchmarkSample sample)
	{ 
		var obj = GameObject.Instantiate(preview); 

		obj.transform.parent = activeBenchmarkPresenter.transform;
		
		activeBenchmarkPresenter.model.Samples.Add(sample); 

	}

	public void StartRecording() {

		recording = true;
		
		var cameraConfig = tracker.OWL.GetCameras();

		availableCameras = cameraConfig.Length;

		activeBenchmarkPresenter.model.cameraConfig = cameraConfig;

	}

	public void StopRecording()
	{
		recording = false; 
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(8, Screen.height / 4, Screen.width - 16, Screen.height / 3 + 4));

			GUILayout.Label(string.Format("X {0}", latestAvgResult.position.x));
			GUILayout.Label(string.Format("Y {0}", latestAvgResult.position.y));
			GUILayout.Label(string.Format("Z {0}", latestAvgResult.position.z));
			GUILayout.Label(string.Format("e {0}", latestAvgResult.condition));
		 
		GUILayout.EndArea();
	}

	void OnDrawGizmos()
	{
		if (latestMarker == null)
			return;

		foreach (var marker in latestMarker)
		{
			Gizmos.DrawSphere(marker.position, previewRadius);
		} 
	}

	void OnDestroy()
	{
		Debug.Log("Removing listener");
		BenchmarkIO.OnBenchmarkModelLoaded -= BenchmarkIO_OnBenchmarkModelLoaded;
	}

	#region Aggregation

	public BenchmarkSample AveragePosition(ICollection<PSMarker[]> markerSets)
	{
		var result = new BenchmarkSample();
		var subsamples = new List<Vector4>();

		float condition = 0f;

		Vector3 sumVector = Vector3.zero;

		float n = markerSets.Count;

		foreach (var set in markerSets)
		{
			var setAvg = AverageMarkerSet(set);

			condition += setAvg.condition;

			sumVector += setAvg.position;
			 
			subsamples.Add(new Vector4(setAvg.position.x, setAvg.position.y, setAvg.position.z, setAvg.condition));
		}


		result.subsamples = subsamples.ToArray();

		Vector3 avgVector = sumVector / n;

		float avgError = condition / n;

		result.position = avgVector;
		result.condition = avgError;

		return result;
	}

	private BenchmarkSample AverageMarkerSet(PSMarker[] markerSet)
	{
		var result = new BenchmarkSample();
		var subSampleSet = new List<Vector4>();

		float avgCondition = 0f; // bad, no cam sees the marker

		Vector3 avgVector = Vector3.zero;

		float n = markerSet.Length;

		foreach (var marker in markerSet)
		{
			avgCondition += marker.cond;
			avgVector += marker.position;
			subSampleSet.Add(new Vector4(marker.position.x, marker.position.y, marker.position.z, marker.cond));
		} 

		if (n < expectedMarkerCount)
			result.condition = 0; // If a marker is missing mark the whole sample as dirty
		
		result.subsamples = subSampleSet.ToArray();

		avgVector = avgVector / n;
		avgCondition = avgCondition / n;

		result.position = avgVector;
		result.condition = avgCondition; 

		return result;
	}
	 
	#endregion

}

// TODO
#region TODO Strategie abstraction

// Strategie pattern
public interface IBenchmarkAlgorithm
{
	// aggregation contdition matched

	BenchmarkSample Aggregate(PSMarker[] markers);

	bool AggregationConditionMatched();

	BenchmarkSample Aggregate(ICollection<PSMarker[]> samples);

	Color GetColorFor(float error);
}

public class MeanAggregation : IBenchmarkAlgorithm
{
	public int Cameras;

	public BenchmarkSample Aggregate(PSMarker[] markers)
	{
		throw new NotImplementedException();
	}

	public bool AggregationConditionMatched()
	{
		throw new NotImplementedException();
	}

	public BenchmarkSample Aggregate(ICollection<PSMarker[]> samples)
	{
		throw new NotImplementedException();
	}

	public Color GetColorFor(float error)
	{
		throw new NotImplementedException();
	}
}
#endregion