using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipeStyleWindow : EditorWindow
{
    public PipeStyle usedStyle;
    public Pipesystem pipesystem;

    private string[] pathStyleArray;
    private PipeStyle[] styleArray;
    private int positionStyleChoser = -1;
    private string pipestyleDatapath;
    private string newPipestyleName;

    private PrefabCollector segmentPrefabCollector;
    private PrefabCollector interjacentPrefabCollector;
    private PrefabCollector endPointPrefabCollector;
    private PrefabCollector snappablePrefabCollector;

    private int colorfieldWidth = 160;

    //Material
    private float materialMetallicValue;
    private float materialSmoothnessValue;
    //private Shader tempShader;

    private float oldGizmoSizeControlPoints;
    private float oldGizmoSizeSnapPoints;

    private bool showSegmentGroup;
    private bool showInterjacentGroup;
    private bool showEndPointGroup;
    private bool showSnappableGroup;
    private bool showGizmoGroup;
    private bool showMaterialGroup;

    [MenuItem("Window/Pipesystem Style")]
    public static void ShowWindow()
    {
        GetWindow<PipeStyleWindow>("Style");
    }

    void OnGUI()
    {
        if (pipesystem != null)
        {
            EditorGUILayout.LabelField(usedStyle.ToString(), EditorStyles.boldLabel);
            StyleChoser();

            GUILayout.BeginHorizontal();

            newPipestyleName = GUILayout.TextField(newPipestyleName);

            if (GUILayout.Button("Save as New"))
                SavePipestyleAsNew();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Save for All"))
                ApplyPipestyle(usedStyle);


            if (GUILayout.Button("Revert"))
                RevertChanges();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
                DeletePipestyle();

            if (GUILayout.Button("Recalculate All"))
                RecalculateAll();
            GUILayout.EndHorizontal();


            //Segment
            showSegmentGroup = EditorGUILayout.Foldout(showSegmentGroup, "Segments");
            if (showSegmentGroup)
                Segment();

            //Interjasents
            showInterjacentGroup = EditorGUILayout.Foldout(showInterjacentGroup, "Interjacents");
            if (showInterjacentGroup)
                Interjacent();

            //EndPoints
            showEndPointGroup = EditorGUILayout.Foldout(showEndPointGroup, "Endpoints");
            if (showEndPointGroup)
                EndPoints();

            //Snappable
            showSnappableGroup = EditorGUILayout.Foldout(showSnappableGroup, "Snappable");
            if (showSnappableGroup)
                Snappable();

            //GizmoColors
            showGizmoGroup = EditorGUILayout.Foldout(showGizmoGroup, "Gizmo");
            if (showGizmoGroup)
                Gizmos();
            
            //Material
            showMaterialGroup = EditorGUILayout.Foldout(showMaterialGroup, "Matrial");
            if (showMaterialGroup)
                MainMaterial();
        }
    }

    void OnEnable()
    {
        GetAllPipestyls();

        segmentPrefabCollector = (PrefabCollector)CreateInstance("PrefabCollector");
        interjacentPrefabCollector = (PrefabCollector)CreateInstance("PrefabCollector");
        endPointPrefabCollector = (PrefabCollector)CreateInstance("PrefabCollector");
        snappablePrefabCollector = (PrefabCollector)CreateInstance("PrefabCollector");

        if (pipesystem!=null)
        {
            segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, usedStyle.segmentPrefab, usedStyle.segmentProbability);
            segmentPrefabCollector.RevertChanges();
            segmentPrefabCollector.SetupOldProbability();

            interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, usedStyle.interjacentPrefab, usedStyle.interjacentProbability);
            interjacentPrefabCollector.RevertChanges();
            interjacentPrefabCollector.SetupOldProbability();

            endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
            endPointPrefabCollector.RevertChanges();
            endPointPrefabCollector.SetupOldProbability();

            snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
            snappablePrefabCollector.RevertChanges();
            snappablePrefabCollector.SetupOldProbability();
        }
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
    }

    public void StyleChoser()
    {
        //initial set
        if(positionStyleChoser<0)
        {
            for (int i = 0; i < styleArray.Length; i++)
            {
                if (styleArray[i] == pipesystem.pipeStyle)
                {
                    positionStyleChoser = i;
                    break;
                }
            }
            segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, usedStyle.segmentPrefab, usedStyle.segmentProbability);
            segmentPrefabCollector.RevertChanges();
            segmentPrefabCollector.SetupOldProbability();

            interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, usedStyle.interjacentPrefab, usedStyle.interjacentProbability);
            interjacentPrefabCollector.RevertChanges();
            interjacentPrefabCollector.SetupOldProbability();

            endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
            endPointPrefabCollector.RevertChanges();
            endPointPrefabCollector.SetupOldProbability();

            snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
            snappablePrefabCollector.RevertChanges();
            snappablePrefabCollector.SetupOldProbability();
        }

        //if scrollbar is moved replace usedStyle
        EditorGUI.BeginChangeCheck();
        positionStyleChoser = (int)GUILayout.HorizontalScrollbar(positionStyleChoser, 1, 0, styleArray.Length);

        if(EditorGUI.EndChangeCheck())
        {
            usedStyle = styleArray[positionStyleChoser];
            pipesystem.pipeStyle = styleArray[positionStyleChoser];

            segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, usedStyle.segmentPrefab, usedStyle.segmentProbability);
            segmentPrefabCollector.RevertChanges();
            segmentPrefabCollector.SetupOldProbability();

            interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, usedStyle.interjacentPrefab, usedStyle.interjacentProbability);
            interjacentPrefabCollector.RevertChanges();
            interjacentPrefabCollector.SetupOldProbability();

            endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
            endPointPrefabCollector.RevertChanges();
            endPointPrefabCollector.SetupOldProbability();

            snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
            snappablePrefabCollector.RevertChanges();
            snappablePrefabCollector.SetupOldProbability();
        }
    }

    public void Segment()
    {
        //Segment properties
        GUILayout.BeginHorizontal();
        GUILayout.Label("Length:");
        pipesystem.segmentLength = EditorGUILayout.FloatField(pipesystem.segmentLength);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Diameter:");
        pipesystem.segmentDiameter = EditorGUILayout.FloatField(pipesystem.segmentDiameter);
        GUILayout.EndHorizontal();

        GUILayout.Label("Distance to ControlPoint:");
        GUILayout.BeginHorizontal();
        pipesystem.distanceSegmentsControlPoint = GUILayout.HorizontalSlider(pipesystem.distanceSegmentsControlPoint, 0, 50);
        pipesystem.distanceSegmentsControlPoint = EditorGUILayout.FloatField(pipesystem.distanceSegmentsControlPoint);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        //Spare segment probability
        GUILayout.Label("Spare Segment Probabilty: " + segmentPrefabCollector.sparePrefabProbability.ToString());

        segmentPrefabCollector.PrefabList();
        segmentPrefabCollector.DragNewArea();
    }

    public void Interjacent()
    {
        interjacentPrefabCollector.PrefabList();
        interjacentPrefabCollector.DragNewArea();
    }

    public void EndPoints()
    {
        endPointPrefabCollector.PrefabList();
        endPointPrefabCollector.DragNewArea();
    }

    public void Snappable()
    {
        snappablePrefabCollector.PrefabList();
        snappablePrefabCollector.DragNewArea();
    }

    public void SavePipestyleAsNew()
    {
        //create a new pipestyle instance with the current values
        PipeStyle newStyle = (PipeStyle)CreateInstance("PipeStyle");

        newStyle.segmentPrefab = new List<GameObject>();
        newStyle.segmentProbability = new List<int>();
        newStyle.interjacentPrefab = new List<GameObject>();
        newStyle.interjacentProbability = new List<int>();
        newStyle.gizmoColors = new List<Color32>();

        segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, newStyle.segmentPrefab, newStyle.segmentProbability);
        interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, newStyle.interjacentPrefab, newStyle.interjacentProbability);
        endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
        snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);

        ApplyPipestyle(newStyle);

        //Get the path and place the new asset
        pipestyleDatapath = AssetDatabase.GetAssetPath(usedStyle);
        pipestyleDatapath = pipestyleDatapath.Replace(usedStyle.name + ".asset", string.Empty);
        string path = AssetDatabase.GenerateUniqueAssetPath(pipestyleDatapath + newPipestyleName + ".asset");
        Debug.Log(path);
        AssetDatabase.CreateAsset(newStyle, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        //select new style and clear the name field
        pipesystem.pipeStyle = newStyle;
        usedStyle = newStyle;
        GetAllPipestyls();
        newPipestyleName = string.Empty;


        for (int i = 0; i < styleArray.Length; i++)
        {
            if (styleArray[i] == usedStyle)
            {
                positionStyleChoser = i;
                break;
            }

        }
    }

    public void ApplyPipestyle(PipeStyle style)
    {
        Color32 tempColor = Color.red;
        float tempFloat;
        
        //Material tempMat = new Material(tempShader);

        tempFloat = pipesystem.segmentLength;
        style.segmentLength = tempFloat;

        tempFloat = pipesystem.segmentDiameter;
        style.segmentDiameter = tempFloat;

        tempFloat = pipesystem.distanceSegmentsControlPoint;
        style.distanceSegmentsControlPoint = tempFloat;


        segmentPrefabCollector.ApplyChanges();
        interjacentPrefabCollector.ApplyChanges();
        endPointPrefabCollector.ApplyChanges();
        snappablePrefabCollector.ApplyChanges();

        //GizmoColors
        while (style.gizmoColors.Count < pipesystem.gizmoColors.Count)
        {
            style.gizmoColors.Add(tempColor);
        }

        for (int i = 0; i < pipesystem.gizmoColors.Count; i++)
        {
            tempColor = pipesystem.gizmoColors[i];
            style.gizmoColors[i] = tempColor;
        }

        tempFloat = pipesystem.gizmoSizeControlPoints;
        style.gizmoSizeControlPoints = tempFloat;

        //material
        //tempMat = pipesystem.mainMaterial;
        //style.material = tempMat;
    }

    public void RevertChanges()
    {
        //get the attached pipestyle and change all values to theirs
        Color32 tempColor;

        pipesystem.segmentLength = usedStyle.segmentLength;
        pipesystem.segmentDiameter = usedStyle.segmentDiameter;
        pipesystem.distanceSegmentsControlPoint = usedStyle.distanceSegmentsControlPoint;

        segmentPrefabCollector.RevertChanges();
        interjacentPrefabCollector.RevertChanges();
        endPointPrefabCollector.RevertChanges();
        snappablePrefabCollector.RevertChanges();

        //gizmos
        for (int i = 0; i < pipesystem.gizmoColors.Count; i++)
        {
            tempColor = usedStyle.gizmoColors[i];
            pipesystem.gizmoColors[i] = tempColor;
        }

        pipesystem.gizmoSizeControlPoints = usedStyle.gizmoSizeControlPoints;
    }

    public void DeletePipestyle()
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(pathStyleArray[positionStyleChoser]));

        GetAllPipestyls();
        StyleChoser();
    }

    public void RecalculateAll()
    {
        foreach (ControlPoint controlPoint in pipesystem.controlPoints)
            controlPoint.oldPosition.x += 1;
    }

    public void Gizmos()
    {
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("ControlPoint Size");
        //pipesystem.gizmoSizeControlPoints = GUILayout.HorizontalSlider(pipesystem.gizmoSizeControlPoints,0,10, GUILayout.Width(colorfieldWidth));
        //GUILayout.EndHorizontal();

        //if(pipesystem.gizmoSizeControlPoints!=oldGizmoSizeControlPoints)
        //{
        //    foreach (ControlPoint controlPoint in pipesystem.controlPoints)
        //        controlPoint.gizmoSize = pipesystem.gizmoSizeControlPoints;

        //    oldGizmoSizeControlPoints = pipesystem.gizmoSizeControlPoints;
        //}

        GUILayout.Space(10);
        GUILayout.Label("Control Point", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unselected Outer");
        pipesystem.gizmoColors[0] = EditorGUILayout.ColorField(pipesystem.gizmoColors[0], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unselected Inner");
        pipesystem.gizmoColors[1] = EditorGUILayout.ColorField(pipesystem.gizmoColors[1], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Outer");
        pipesystem.gizmoColors[2] = EditorGUILayout.ColorField(pipesystem.gizmoColors[2], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected Inner");
        pipesystem.gizmoColors[3] = EditorGUILayout.ColorField(pipesystem.gizmoColors[3], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Connection Line");
        pipesystem.gizmoColors[4] = EditorGUILayout.ColorField(pipesystem.gizmoColors[4], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Segment Hover");
        pipesystem.gizmoColors[5] = EditorGUILayout.ColorField(pipesystem.gizmoColors[5], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Snap Point", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Available");
        pipesystem.gizmoColors[6] = EditorGUILayout.ColorField(pipesystem.gizmoColors[6], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Selected");
        pipesystem.gizmoColors[7] = EditorGUILayout.ColorField(pipesystem.gizmoColors[7], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unavailable");
        pipesystem.gizmoColors[8] = EditorGUILayout.ColorField(pipesystem.gizmoColors[8], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();
    }

    public void MainMaterial()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Color");
        pipesystem.mainMaterial.color = EditorGUILayout.ColorField(pipesystem.mainMaterial.color, GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Metallic");
        materialMetallicValue = GUILayout.HorizontalSlider(materialMetallicValue, 0, 1);
        pipesystem.mainMaterial.SetFloat("_Metallic", materialMetallicValue);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Smoothness");
        materialSmoothnessValue = GUILayout.HorizontalSlider(materialSmoothnessValue, 0, 1);
        pipesystem.mainMaterial.SetFloat("_Glossiness", materialSmoothnessValue);
        GUILayout.EndHorizontal();

    }
}
