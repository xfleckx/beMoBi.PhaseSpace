using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Should not require a OWL Interface component!
/// Cause multiple tracking procedures are possible (Rigidbody & Point tracking)
/// 
/// </summary>
public class PSRigidbodyController : MonoBehaviour {
	
	public OWLInterface tracker;

	public string rigidBodyFileName;

	private PSRigid expectedRigidBody;

	public TextAsset rigidBodyDefinition;

	public int rigidBodyTrackerID;

	public bool useRigidBodyOrientation = false;

	public void Awake()
	{
		if (tracker == null)
			throw new MissingReferenceException("OWL Interface missing");

		tracker.OwlConnectedCallbacks += CreateRigidbodyTracker;
		tracker.OwlUpdateCallbacks += UpdateRigidbodyTarget;
	}

	// Use this for initialization
	void Start () {

	}

	void CreateRigidbodyTracker()
	{
		if (rigidBodyDefinition != null){ 
			Debug.Log(string.Format("Create Rigidbody Tracker {0}", rigidBodyTrackerID));
			tracker.OWL.CreateRigidTrackerFrom(rigidBodyTrackerID, rigidBodyDefinition);
		}
		else
			Debug.LogError("No rigidbody file available...");
	}

	/// <summary>
	/// Used by the Update function of the OWLInterface
	/// </summary>
	void UpdateRigidbodyTarget()
	{
		var rigids = tracker.OWL.GetRigids();
		if(rigids.Length == 0)
			return;

		expectedRigidBody = rigids[rigidBodyTrackerID];

		transform.position = expectedRigidBody.position;
		
		if(useRigidBodyOrientation)
			transform.rotation = expectedRigidBody.rotation;
	}
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos()
	{
		if (expectedRigidBody != null) { 

			var rbPos = expectedRigidBody.position;
			
			var floorPosition = new Vector3(rbPos.x, 0, rbPos.z);

			Gizmos.DrawWireSphere(rbPos, 0.02f);


			Gizmos.DrawLine(rbPos, floorPosition);

#if UNITY_EDITOR


			Handles.DrawSolidDisc(floorPosition, gameObject.transform.up, 0.2f);

#endif

		}
	}

	public void OnDisable()
	{

	}

	public void OnDestroy()
	{
		tracker.OwlConnectedCallbacks -= CreateRigidbodyTracker;
		tracker.OwlUpdateCallbacks -= UpdateRigidbodyTarget;
	}
}
