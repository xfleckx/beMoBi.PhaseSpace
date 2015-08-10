using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SubjectController))]
public class SubjectInspector: Editor 
{
    private SubjectController instance;

    public override void OnInspectorGUI()
    {
        instance = target as SubjectController;

        base.DrawDefaultInspector();

        if (GUILayout.Button("Reset Tracker"))
        {
            instance.ResetTracker();
        }
    }

} 