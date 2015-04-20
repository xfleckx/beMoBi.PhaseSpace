using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Xml.Serialization;

public class BenchmarkIO {

	public static void Save(BenchmarkModel benchmark, string fileName)
	{
		var serializer = new XmlSerializer(typeof(BenchmarkModel));
		var stream = new FileStream(fileName, FileMode.Create);
		serializer.Serialize(stream, benchmark);
		stream.Close();
	}

	public static void Load(string fileName)
	{ 
		if (!File.Exists(fileName)) {
			throw new FileNotFoundException("Missing file", fileName, null);
		}

		var serializer = new XmlSerializer(typeof(BenchmarkModel));
		using (var stream = new FileStream(fileName, FileMode.Open))
		{
			if (OnBenchmarkModelLoaded != null) { 
			   var model = serializer.Deserialize(stream) as BenchmarkModel;
			   OnBenchmarkModelLoaded(model);
			}
		} 
	}

	public static event Action<BenchmarkModel> OnBenchmarkModelLoaded;
}

[Serializable]
public class BenchmarkModel
{
	public DateTime DateOfRecording;

	public string Description;

	public Color color;

	[XmlArray("Samples"), XmlArrayItem("Sample")]
	public List<BenchmarkSample> Samples;

	[XmlArray("Cameras"), XmlArrayItem("Camera")]
	public PSCamera[] cameraConfig;
}

[Serializable]
public struct BenchmarkSample
{
	public Vector3 position;

	public float condition;

	public Vector4[] subsamples;
}