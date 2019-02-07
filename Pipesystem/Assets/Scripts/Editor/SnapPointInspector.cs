using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SnapPoint))]
public class SnapPointInspector : Editor
{
    private string[] pathStyleArray;
    private PipeStyle[] styleArray;
    private bool[] boolArray;
    SnapPoint snapPoint;

    public override void OnInspectorGUI()
    {

        for (int i = 0; i < styleArray.Length; i++)
        {
            GUILayout.BeginHorizontal();
            boolArray[i] = GUILayout.Toggle(boolArray[i],"");
            GUILayout.Label(styleArray[i].name);
            GUILayout.EndHorizontal();
        }

        snapPoint.connectablePipestyles.Clear();
        for (int i = 0; i < styleArray.Length; i++)
        {
            if (boolArray[i] == true)
                snapPoint.connectablePipestyles.Add(styleArray[i]);
        }
    }

    void OnEnable()
    {
        snapPoint = (SnapPoint)target;
        GetAllPipestyls();
    }

    public void GetAllPipestyls()
    {
        //Gets all PipesystemStyle intances in the project and puts them in an array
        string fullPath;
        pathStyleArray = AssetDatabase.FindAssets("t:PipeStyle");

        styleArray = new PipeStyle[pathStyleArray.Length];
        for (int i = 0; i < pathStyleArray.Length; i++)
        {
            fullPath = AssetDatabase.GUIDToAssetPath(pathStyleArray[i]);
            styleArray[i] = (PipeStyle)AssetDatabase.LoadAssetAtPath(fullPath, typeof(PipeStyle));
        }

        boolArray = new bool[styleArray.Length];

        for (int i = 0; i < styleArray.Length; i++)
        {
            if (snapPoint.connectablePipestyles.Contains(styleArray[i]))
                boolArray[i] = true;
        }
    }
}
