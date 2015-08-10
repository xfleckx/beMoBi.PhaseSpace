using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;


[CustomEditor(typeof(PSRigidbodyController))]
public class RigidbodyControllerInspector : Editor
{
    private PSRigidbodyController controller;

    public override void OnInspectorGUI()
    {
        if (null == controller)
        {
            controller = (PSRigidbodyController)target;  
        }

        EditorGUILayout.BeginVertical();

        controller.tracker = (OWLInterface)EditorGUILayout.ObjectField("Owl Interface", controller.tracker, typeof(OWLInterface), true);

        controller.useRigidBodyOrientation = EditorGUILayout.ToggleLeft("Use RB orientation", controller.useRigidBodyOrientation);

        EditorGUILayout.Space();

        controller.rigidBodyTrackerID = EditorGUILayout.IntField("Tracker ID", controller.rigidBodyTrackerID);

        EditorGUILayout.LabelField("Rigidbody Defintion:");

        controller.rigidBodyDefinition = EditorGUILayout.ObjectField(controller.rigidBodyDefinition, typeof(TextAsset)) as TextAsset;

        if (GUILayout.Button("Import rb file"))
        {
            var fileName = EditorUtility.OpenFilePanel("Open Rigidbody file", Environment.CurrentDirectory, "rb");

            if (fileName == null || fileName == String.Empty)
                return;

            var textAssetName = RigidBodyFileImporter.ImportFrom(fileName);

            controller.rigidBodyDefinition = textAssetName;
           
            PrefabType type = PrefabUtility.GetPrefabType(controller.gameObject);
            
            bool isAPrefab = type == PrefabType.PrefabInstance;

            if (isAPrefab)
            {
               PrefabUtility.ReplacePrefab(controller.gameObject, PrefabUtility.GetPrefabParent(controller.gameObject), ReplacePrefabOptions.ConnectToPrefab);
            }
        }

        EditorGUILayout.EndVertical();
    }
}

public static class RigidBodyFileImporter
{ 
    private const string rigidBodyDefinitionSubFolder = "RigidBodyDefinitions";

    public static TextAsset ImportFrom(string filePath)
    {  
        var fileContentsAsBytes = File.ReadAllBytes(filePath);
        
        var fileContentsAsUTF8 = System.Text.Encoding.UTF8.GetString(fileContentsAsBytes);
         
        var assetName = Path.GetFileNameWithoutExtension(filePath);

        string rbDirPath = string.Empty;
        string expectedFolder = string.Format("Assets/{0}", rigidBodyDefinitionSubFolder);

        if (!AssetDatabase.IsValidFolder(expectedFolder)) { 
          var guid = AssetDatabase.CreateFolder("Assets", rigidBodyDefinitionSubFolder);
          rbDirPath = AssetDatabase.GUIDToAssetPath(guid);
        }

        var targetPath =  string.Format("{0}/{1}.txt", expectedFolder, assetName);
         
        File.WriteAllText(targetPath, fileContentsAsUTF8);
        
        AssetDatabase.Refresh();

        var textAssetResult = AssetDatabase.LoadAssetAtPath(targetPath, typeof(TextAsset)) as TextAsset;

        return textAssetResult;
    }
}