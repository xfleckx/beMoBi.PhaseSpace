using UnityEngine;
using System.Collections;
using System;

public class BenchmarkPresenter : MonoBehaviour
{ 
	public GameObject prototypSample;

	public BenchmarkModel model;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{ 

	}

	Color GetFrom(float error)
	{
		double cValue = error - Math.Truncate(error);
		float cV = (float)cValue;
		Color c = new Color(cV, cV, cV);
		return c;
	}
	

	void Render(BenchmarkSample avgPosition)
	{
		var obj = GameObject.Instantiate(prototypSample); 

		obj.transform.position = avgPosition.position;
		obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		obj.GetComponent<Renderer>().material.color = GetFrom(avgPosition.condition);
		 
	}

	internal void RenderModel()
	{
		foreach (var sample in model.Samples)
		{
			Render(sample);
		}
	}
}
