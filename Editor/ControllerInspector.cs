using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PhaseSpaceController))]
public class ControllerInspector : Editor {

	private Ping ping;
	private float lastPingTime;

	private float leftBorderMargin = 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnSceneGUI()
	{
		var controller = target as PhaseSpaceController;

		// draw a UI tip in scene view informing user how to draw & erase tiles
		Handles.BeginGUI();

		GUI.Label(new Rect(leftBorderMargin, 90, 100, 15), string.Format("Marker: {0}", controller.AvailableMarkers));
		GUI.Label(new Rect(leftBorderMargin, 75, 100, 15), string.Format("Position X: {0}", controller.MarkerPosition.x));
		GUI.Label(new Rect(leftBorderMargin, 60, 100, 15), string.Format("Position Y: {0}", controller.MarkerPosition.y));
		GUI.Label(new Rect(leftBorderMargin, 45, 100, 15), string.Format("Position Z: {0}", controller.MarkerPosition.z));

		if (GUI.Button(new Rect(leftBorderMargin, 105, 75, 15), "Ping"))
		{
			ping = new Ping(controller.OWLHost);
		}

		GUI.Label(new Rect(leftBorderMargin, 120, 250, 15), string.Format("Ping: {0} ms", lastPingTime));

		if (ping != null && ping.isDone)
		{
			lastPingTime = ping.time;
			ping.DestroyPing();
		}
		//int offset = 105;
		
		//foreach (var item in controller.markerList)
		//{
		//    GUI.Label(new Rect(10, Screen.height - offset, 350, 100), string.Format("Position: {0}  {1}  {2}", item.position.x, item.position.y, item.position.z));
		//    Handles.SphereCap(offset, item.position, Quaternion.identity, 0.1f);
		//    offset += 15;
		//}

		
		//GUI.Label(new Rect(10, Screen.height - 120, 250, 100), string.Format("Marker: {0} {1} {2}", focusedMaze.MarkerPosition.x, focusedMaze.MarkerPosition.y, focusedMaze.MarkerPosition.z));
		Handles.EndGUI();

		int i = 0;
		foreach (var item in controller.markerList)
		{
			Handles.SphereCap(++i, item.position, Quaternion.identity, 0.1f); 
		}
	}
}
