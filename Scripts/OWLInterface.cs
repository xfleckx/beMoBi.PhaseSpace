using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System;

public enum OWLUpdateStratgy { FixedUpdate, Update, OnPreRender }

public delegate void OnPostOwlUpdate();
public delegate void OnOwlConnected();

public class OWLInterface : MonoBehaviour
{

    public OWLUpdateStratgy updateMoment = OWLUpdateStratgy.FixedUpdate;

    public string OWLHost;
    public bool isSlave = false;
    public bool autoConnectOnStart = false;
    //
    public OWLWrapper OWL = new OWLWrapper();
    private bool connected = false;

    public bool IsConnected { get { return connected; } }

    private Stopwatch stopWatch = new Stopwatch();

    public float OWLUpdateTook = 0f;

	public OnPostOwlUpdate OwlUpdateCallbacks;

	public OnOwlConnected OwlConnectedCallbacks;

	protected string message = String.Empty;

    public bool showDeprecatedOnGUI = false;
	
	//
	void Awake()
	{
		isSlave = PlayerPrefs.GetInt("owlInSlaveMode", 0) == 1;
	}
	 
	// Use this for initialization
	void Start () {
		 
		if (!OWLHost.Equals(string.Empty) && autoConnectOnStart)
		{
			ConnectToOWLInstance();
			connected = OWL.Connected();
		} 
	} 

	public void ConnectToOWLInstance()
	{
		if (OWL.Connect(OWLHost, isSlave)) { 
			
            UnityEngine.Debug.Log(string.Format("OWL connected to {0}", OWLHost), this);

			if (!isSlave)
			{
				if (OwlConnectedCallbacks != null)
				{
					OwlConnectedCallbacks.Invoke();
				}
				else
				{ 
					// create default point tracker
					int n = 128;
					int[] leds = new int[n];
					for (int i = 0; i < n; i++)
						leds[i] = i;
					OWL.CreatePointTracker(0, leds);
				}

			}

			// start streaming
			OWL.Start();

        }else
        {
            UnityEngine.Debug.LogWarning(string.Format("Connection to OWL Host {0} failed!", OWLHost), this);
        }
	}

    public void DisconnectFromOWLInstance()
    {
        OWL.Disconnect();
    }

	public bool HasConfigurationAvaiable()
	{
		return false;
	}

    private void PerformOwlUpdate()
    {
        stopWatch.Start();

        OWL.Update();

        stopWatch.Stop();

        OWLUpdateTook = stopWatch.ElapsedMilliseconds;

        stopWatch.Reset();

        if (OwlUpdateCallbacks != null)
        {
            OwlUpdateCallbacks.Invoke();
        }
    }

	void FixedUpdate()
	{
        if (OWL.Connected() && updateMoment == OWLUpdateStratgy.FixedUpdate)
        {
            PerformOwlUpdate();
		}
	}

	// Update is called once per frame
	void Update () {
        if (OWL.Connected() && updateMoment == OWLUpdateStratgy.Update)
            PerformOwlUpdate();
	}

	void OnPreRender()
	{
        if (OWL.Connected() && updateMoment == OWLUpdateStratgy.OnPreRender)
            PerformOwlUpdate();
	}

	// need to be merged into the CustomInspector
	void OnGUI()
	{
        if (!showDeprecatedOnGUI)
            return;

		GUILayout.BeginArea(new Rect(8, 8, Screen.width - 16, Screen.height / 4 + 4));
		GUILayout.BeginHorizontal();
		GUILayout.Label("Device", GUILayout.ExpandWidth(false));

		// disable controls if connected already
		if (connected) GUI.enabled = false;

		// reenable controls
		GUI.enabled = true;

		if (connected)
		{
			if (GUILayout.Button("Disconnect", GUILayout.ExpandWidth(false)))
				OWL.Disconnect();
		}
		else
		{
			OWLHost = GUILayout.TextField(OWLHost, GUILayout.Width(250));

			if (GUILayout.Button("Connect", GUILayout.ExpandWidth(false)))
			{
				ConnectToOWLInstance();

				connected = OWL.Connected();

				if(connected) 
					PlayerPrefs.SetString("OWLHost", OWLHost);
			}
		}
		GUILayout.EndHorizontal();

		// display avgCondition message or current frame number
		if (OWL.error != 0)
		{
			message = String.Format("owl message: 0x{0,0:X}", OWL.error);
		}
		else
		{
			message = String.Format("frame = {0}, m = {1}, r = {2}, c = {3}", OWL.frame, OWL.NumMarkers, OWL.NumRigids, OWL.NumCameras);
		}

		GUILayout.Label(message);

		GUILayout.EndArea();
	}

	//
	void OnDestroy()
	{

		// save user settings
		PlayerPrefs.SetString("OWLHost", OWLHost);
		PlayerPrefs.SetInt("owlInSlaveMode", Convert.ToInt32(isSlave));

		// disconnect from OWL server
		OWL.Disconnect();
	}
}


public class PhaseSpaceConfiguration : ScriptableObject
{
	public string OWLHost;

}