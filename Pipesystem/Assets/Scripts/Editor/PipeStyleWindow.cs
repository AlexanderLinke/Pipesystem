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
    private PrefabCollector controlPointPrefabCollector;

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
    private bool showControlPointGroup;
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

            //ControlPoint
            showControlPointGroup = EditorGUILayout.Foldout(showControlPointGroup, "ControlPoint");
            if (showControlPointGroup)
                ControlPoint();

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
        controlPointPrefabCollector = (PrefabCollector)CreateInstance("PrefabCollector");

        if (pipesystem!=null)
        {
            segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, usedStyle.segmentPrefab, usedStyle.segmentProbability);
            segmentPrefabCollector.SetupOldProbability();

            interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, usedStyle.interjacentPrefab, usedStyle.interjacentProbability);
            interjacentPrefabCollector.SetupOldProbability();

            endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
            endPointPrefabCollector.SetupOldProbability();

            snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
            snappablePrefabCollector.SetupOldProbability();

            controlPointPrefabCollector.Setup(pipesystem.controlPointPrefab, pipesystem.controlPointProbability, usedStyle.controlPointPrefab, usedStyle.controlPointProbability);
            controlPointPrefabCollector.SetupOldProbability();
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
            segmentPrefabCollector.SetupOldProbability();

            interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, usedStyle.interjacentPrefab, usedStyle.interjacentProbability);
            interjacentPrefabCollector.SetupOldProbability();

            endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
            endPointPrefabCollector.SetupOldProbability();

            snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
            snappablePrefabCollector.SetupOldProbability();

            controlPointPrefabCollector.Setup(pipesystem.controlPointPrefab, pipesystem.controlPointProbability, usedStyle.controlPointPrefab, usedStyle.controlPointProbability);
            controlPointPrefabCollector.SetupOldProbability();
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

            controlPointPrefabCollector.Setup(pipesystem.controlPointPrefab, pipesystem.controlPointProbability, usedStyle.controlPointPrefab, usedStyle.controlPointProbability);
            controlPointPrefabCollector.RevertChanges();
            controlPointPrefabCollector.SetupOldProbability();
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

    public void ControlPoint()
    {
        controlPointPrefabCollector.PrefabList();
        controlPointPrefabCollector.DragNewArea();

        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.Label("BendPoints: ");
        pipesystem.controlPointBendPoints = EditorGUILayout.IntField(pipesystem.controlPointBendPoints);
        GUILayout.EndHorizontal();

        //recalculate all if controlPointBendPoints has been changed
        if (EditorGUI.EndChangeCheck())
        {
            for (int i = 0; i < pipesystem.controlPointPrefab.Count; i++)
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(pipesystem.modifiedControlPointPrefab[i]));

            pipesystem.modifiedControlPointPrefab.Clear();

            for (int i = 0; i < pipesystem.controlPointPrefab.Count; i++)
                CreateControlPoint(i);

            pipesystem.recalculateAll = true;

        }

        //delete modified if base is removed
        if (controlPointPrefabCollector.deleteAt != -1)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(pipesystem.modifiedControlPointPrefab[controlPointPrefabCollector.deleteAt]));
            pipesystem.modifiedControlPointPrefab.RemoveAt(controlPointPrefabCollector.deleteAt);
            controlPointPrefabCollector.deleteAt = -1;
        }

        //add new
        if (pipesystem.modifiedControlPointPrefab.Count < pipesystem.controlPointPrefab.Count)
            CreateControlPoint(pipesystem.modifiedControlPointPrefab.Count);


        GUILayout.Label("CurveStrength:");
        GUILayout.BeginHorizontal();
        pipesystem.controlPointCurveStrength = GUILayout.HorizontalSlider(pipesystem.controlPointCurveStrength, 0, 1);
        pipesystem.controlPointCurveStrength = EditorGUILayout.FloatField(pipesystem.controlPointCurveStrength);
        GUILayout.EndHorizontal();
    }

    public void CreateControlPoint(int index)
    {
        //GUI.changed = false;
        GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.controlPointPrefab[index]);
        SkinnedMeshRenderer meshRenderer;
        Mesh mesh;

        //get mesh
        meshRenderer = prefab.AddComponent<SkinnedMeshRenderer>();
        meshRenderer.sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        mesh = meshRenderer.sharedMesh;
        mesh.boneWeights = null;
        mesh.bindposes = null;

        //weigth paint
        Transform[] bones = new Transform[pipesystem.controlPointBendPoints];
        Matrix4x4[] bindPoses = new Matrix4x4[pipesystem.controlPointBendPoints];
        BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
        //bones

        for (int i = 0; i < pipesystem.controlPointBendPoints; i++)
        {
            bones[i] = new GameObject().transform;
            bones[i].name = i.ToString();
            bones[i].localRotation = Quaternion.identity;

            float zPosition = -pipesystem.distanceSegmentsControlPoint + ((pipesystem.distanceSegmentsControlPoint * 2) / (pipesystem.controlPointBendPoints - 1) * i);
            bones[i].localPosition = new Vector3(0, 0, zPosition);

            bindPoses[i] = bones[i].worldToLocalMatrix * prefab.transform.localToWorldMatrix;
            bones[i].transform.parent = prefab.transform;
        }

        //weights
        for (int i = 0; i < mesh.vertexCount; i++)
        {

            float distanceToPoint = 0;
            float distanceToClosestPoint = bones[0].transform.position.z * -2;
            float distanceBetweenPoints = bones[1].transform.position.z - bones[0].transform.position.z;
            int closestPoint = 0;
            int secondClosestPoint = 0;
            float weigthOfClosestPoint;

            for (int j = 0; j < pipesystem.controlPointBendPoints; j++)
            {
                distanceToPoint = mesh.vertices[i].z - bones[j].transform.position.z;

                if (distanceToPoint < 0)
                    distanceToPoint = distanceToPoint * -1;

                if (distanceToPoint <= distanceToClosestPoint)
                {
                    secondClosestPoint = closestPoint;
                    closestPoint = j;
                    distanceToClosestPoint = distanceToPoint;
                }
            }

            weigthOfClosestPoint = (distanceBetweenPoints - (bones[closestPoint].transform.position.z - mesh.vertices[i].z)) / distanceBetweenPoints;

            weights[i].boneIndex0 = closestPoint;
            weights[i].weight0 = weigthOfClosestPoint;

            weights[i].boneIndex1 = secondClosestPoint;
            weights[i].weight1 = 1 - weigthOfClosestPoint;

        }



        meshRenderer.bones = bones;
        mesh.bindposes = bindPoses;
        mesh.boneWeights = weights;

        //save
        string datapath = AssetDatabase.GetAssetPath(pipesystem.controlPointPrefab[index]);
        datapath = datapath.Replace(pipesystem.controlPointPrefab[index].name + ".prefab", string.Empty);
        datapath = datapath + pipesystem.controlPointPrefab[index].name + "Modified" + ".prefab";

        GameObject gg = (GameObject)AssetDatabase.LoadAssetAtPath(datapath, typeof(GameObject));

        if(gg==null)
        {
            PrefabUtility.SaveAsPrefabAsset(prefab, datapath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        

        GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(datapath, typeof(GameObject));
        //Debug.Log(pipesystem.modifiedControlPointPrefab.Count);
        pipesystem.modifiedControlPointPrefab.Add(go);
        pipesystem.modifiedControlPointPrefab[pipesystem.modifiedControlPointPrefab.Count-1] = (GameObject)AssetDatabase.LoadAssetAtPath(datapath,typeof(GameObject));
        //if (GUI.changed)
        //    EditorUtility.SetDirty(mesh);
        DestroyImmediate(prefab);

    }

    public void SavePipestyleAsNew()
    {
        //create a new pipestyle instance with the current values
        PipeStyle newStyle = (PipeStyle)CreateInstance("PipeStyle");

        newStyle.segmentPrefab = new List<GameObject>();
        newStyle.segmentProbability = new List<int>();
        newStyle.interjacentPrefab = new List<GameObject>();
        newStyle.interjacentProbability = new List<int>();
        newStyle.endPointPrefab = new List<GameObject>();
        newStyle.endPointProbability = new List<int>();
        newStyle.snappablePrefab = new List<GameObject>();
        newStyle.snappableProbability = new List<int>();
        newStyle.controlPointPrefab = new List<GameObject>();
        newStyle.controlPointProbability = new List<int>();
        newStyle.gizmoColors = new List<Color32>();

        segmentPrefabCollector.Setup(pipesystem.segmentPrefab, pipesystem.segmentProbability, newStyle.segmentPrefab, newStyle.segmentProbability);
        interjacentPrefabCollector.Setup(pipesystem.interjacentPrefab, pipesystem.interjacentProbability, newStyle.interjacentPrefab, newStyle.interjacentProbability);
        endPointPrefabCollector.Setup(pipesystem.endPointPrefab, pipesystem.endPointProbability, usedStyle.endPointPrefab, usedStyle.endPointProbability);
        snappablePrefabCollector.Setup(pipesystem.snappablePrefab, pipesystem.snappableProbability, usedStyle.snappablePrefab, usedStyle.snappableProbability);
        controlPointPrefabCollector.Setup(pipesystem.controlPointPrefab, pipesystem.controlPointProbability, usedStyle.controlPointPrefab, usedStyle.controlPointProbability);


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
        int tempInt;
        
        Material tempMat = new Material(Shader.Find("Diffuse"));

        tempFloat = pipesystem.segmentLength;
        style.segmentLength = tempFloat;

        tempFloat = pipesystem.segmentDiameter;
        style.segmentDiameter = tempFloat;

        tempFloat = pipesystem.distanceSegmentsControlPoint;
        style.distanceSegmentsControlPoint = tempFloat;

        tempInt = pipesystem.controlPointBendPoints;
        style.controlPointBendPoints = tempInt;

        tempFloat = pipesystem.controlPointCurveStrength;
        style.controlPointCurveStrength = tempFloat;

        segmentPrefabCollector.ApplyChanges();
        interjacentPrefabCollector.ApplyChanges();
        endPointPrefabCollector.ApplyChanges();
        snappablePrefabCollector.ApplyChanges();
        controlPointPrefabCollector.ApplyChanges();

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
        tempMat = pipesystem.mainMaterial;
        style.material = tempMat;
    }

    public void RevertChanges()
    {
        //get the attached pipestyle and change all values to theirs
        Color32 tempColor;

        pipesystem.segmentLength = usedStyle.segmentLength;
        pipesystem.segmentDiameter = usedStyle.segmentDiameter;
        pipesystem.distanceSegmentsControlPoint = usedStyle.distanceSegmentsControlPoint;
        pipesystem.controlPointBendPoints = usedStyle.controlPointBendPoints;
        pipesystem.controlPointCurveStrength = usedStyle.controlPointCurveStrength;

        segmentPrefabCollector.RevertChanges();
        interjacentPrefabCollector.RevertChanges();
        endPointPrefabCollector.RevertChanges();
        snappablePrefabCollector.RevertChanges();
        controlPointPrefabCollector.RevertChanges();

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
        pipesystem.recalculateAll = true;
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
