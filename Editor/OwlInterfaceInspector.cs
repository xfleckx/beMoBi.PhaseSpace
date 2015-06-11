using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OWLInterface))]
public class OwlInterfaceInspector : Editor
{
    private OWLInterface instance;

    private Ping ping;
    private float lastPingTime;

    public override void OnInspectorGUI()
    {
        instance = target as OWLInterface;

        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Ping"))
        {
            ping = new Ping(instance.OWLHost);
        }

        GUILayout.Label( string.Format("Ping: {0} ms", lastPingTime), EditorStyles.boldLabel);

        if (ping != null && ping.isDone)
        {
            lastPingTime = ping.time;
            ping.DestroyPing();
        }

        EditorGUILayout.EndHorizontal();
    }

}
