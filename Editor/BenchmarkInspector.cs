using UnityEngine;
using System.Collections;
using UnityEditor; 
using System.IO;
using System;

[CustomEditor(typeof(PhaseSpaceBenchmark))]
public class BenchmarkInspector : Editor {

    private const string enableRecordingMessage = "Connection to PhaseSpace must be established to enable recording options!";

    private const string fileNamePattern = "bnchmrk_{0}";

    private const string benchmarkFileExtension = "xml";

    private PhaseSpaceBenchmark controller;
    private OWLInterface tracker;

    private bool savingABenchmark = false;
    private bool loadingABenchmark = false;

    private string savefileName = string.Empty;
    private string loadFileName = string.Empty;

    public override void OnInspectorGUI()
    {
        if (null == controller)
        {
            controller = (PhaseSpaceBenchmark)target;
            tracker = controller.tracker;

        }
        else if (tracker == null)
            tracker = controller.tracker;
         

        RenderConfigProperties();

        RenderControlButtons();

    }

    private void RenderConfigProperties()
    {
       controller.tracker = (OWLInterface) EditorGUILayout.ObjectField("Owl Tracker Interface", controller.tracker, typeof(OWLInterface), true);
       controller.activeBenchmarkPresenter = (BenchmarkPresenter)EditorGUILayout.ObjectField("Active Benchmark", controller.activeBenchmarkPresenter, typeof(BenchmarkPresenter), true);
       controller.samplePrototype = (GameObject)EditorGUILayout.ObjectField("Sample Prototype", controller.samplePrototype, typeof(GameObject), true);
       controller.minAveraginCount = EditorGUILayout.IntField("Averaging Count", controller.minAveraginCount);
       controller.expectedMarkerCount = EditorGUILayout.IntField("Expected marker", controller.expectedMarkerCount);
       controller.previewRadius = EditorGUILayout.FloatField("Preview radius", controller.previewRadius);
    }

    private void RenderControlButtons()
    {
        if (tracker == null)
            return;
        
        EditorGUILayout.BeginHorizontal();

        if (controller.recording && !tracker.OWL.Connected())
            controller.StopRecording();

        if (!tracker.OWL.Connected())
            EditorGUILayout.HelpBox(enableRecordingMessage, MessageType.Info, true);

        if (tracker.OWL.Connected() && !controller.recording && GUILayout.Button("Start Recording"))
        {
            controller.StartRecording();
        }
        
        if (controller.recording && GUILayout.Button("Stop Recording"))
        {
            controller.StopRecording();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical();

        savefileName = EditorGUILayout.TextField("Save File Name:", savefileName);

        if (controller.activeBenchmarkPresenter != null && !savingABenchmark && GUILayout.Button("Save latest Benchmark"))
        {
            //if(savefileName.Equals(string.Empty))
            //    EditorGUILayout.HelpBox("Filename empty!", MessageType.Error, true);

            savefileName = EditorUtility.SaveFilePanelInProject("Save Benchmark file", string.Format(fileNamePattern, DateTime.Now.ToString("MMddTHHmmss")), benchmarkFileExtension, "Save the current Benchmark");
            
            var test = Environment.UserName;

            BenchmarkIO.Save(controller.activeBenchmarkPresenter.model, savefileName);

        } 

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        loadFileName = EditorGUILayout.TextField("Load File Name:", loadFileName);

        if (controller.activeBenchmarkPresenter != null && !loadingABenchmark && GUILayout.Button("Load Benchmark"))
        {
            //if (loadFileName.Equals(string.Empty))
            //    EditorGUILayout.HelpBox("Filename empty!", MessageType.Error, true);

            loadFileName = EditorUtility.OpenFilePanel("Open existing benchmark", Environment.CurrentDirectory, benchmarkFileExtension);

            BenchmarkIO.Load(loadFileName);
             

            // todo attach loaded benchmark as new presenter instance
        }

        EditorGUILayout.EndVertical();
         
    }

}
