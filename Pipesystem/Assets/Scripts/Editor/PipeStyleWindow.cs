using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipeStyleWindow : EditorWindow
{

    public PipeStyle usedStyle;
    public GameObject pipesystemObject;
    public Pipesystem pipesystem;

    public GameObject emptyPlaceholder;

    private string[] pathStyleArray;
    private PipeStyle[] styleArray;
    private int positionStyleChoser = 0;
    private int oldPositionStyleChoser = 1;
    private string pipestyleDatapath;
    private string newPipestyleName;

    private GameObject newGameObject;

    private List<int> oldSegmentProbability;
    private List<int> oldInterjacentProbability;

    private int spareSegmentProbability;
    private int spareInterjacentProbability;

    private int segmentCount;
    private int interjacentCount;

    private int colorfieldWidth = 160;

    //Material
    private float materialMetallicValue;
    private float materialSmoothnessValue;
    //private Shader tempShader;

    private bool showSegmentGroup;
    private bool showInterjacentGroup;
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

            if (GUILayout.Button("Save for All"))
                ApplyPipestyle(usedStyle);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Revert"))
                RevertChanges();

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

            //GizmoColors
            showGizmoGroup = EditorGUILayout.Foldout(showGizmoGroup, "Gizmo");
            if (showGizmoGroup)
                GizmoColors();
            
            //Material
            showMaterialGroup = EditorGUILayout.Foldout(showMaterialGroup, "Matrial");
            if (showMaterialGroup)
                MainMaterial();



        }
        else if (pipesystem == null && pipesystemObject != null)
        {
            pipesystem = pipesystemObject.GetComponent<Pipesystem>();
        }
    }

    void OnEnable()
    {
        GetAllPipestyls();

        for (int i = 0; i < styleArray.Length; i++)
        {
            if (styleArray[i] == usedStyle)
            {
                positionStyleChoser = i;
                break;
            }
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
        //if scrollbar is moved replace usedStyle
        positionStyleChoser = (int)GUILayout.HorizontalScrollbar(positionStyleChoser, 1, 0, styleArray.Length);

        if (positionStyleChoser != oldPositionStyleChoser)
        {
            usedStyle = styleArray[positionStyleChoser];
            pipesystem.pipeStyle = styleArray[positionStyleChoser];

            oldSegmentProbability = new List<int>();
            foreach (int i in usedStyle.segmentProbability)
            {
                int temp = usedStyle.segmentProbability[1];
                oldSegmentProbability.Add(temp);
            }

            oldInterjacentProbability = new List<int>();
            for(int i =0; i < usedStyle.interjacentProbability.Count; i++)
            {
                Debug.Log(i);
                int temp = usedStyle.interjacentProbability[i];
                oldInterjacentProbability.Add(temp);
            }

            RevertChanges();
        }
        oldPositionStyleChoser = positionStyleChoser;
    }

    public void Segment()
    {
        Event e = Event.current;
        segmentCount = pipesystem.segmentPrefab.Count;

        if (pipesystem.segmentProbability.Count > 0)
            CalculateSegmentProbability();

        //Segment Length
        GUILayout.BeginHorizontal();
        GUILayout.Label("Segment Length");
        pipesystem.segmentLength = EditorGUILayout.FloatField(pipesystem.segmentLength);
        GUILayout.EndHorizontal();

        //Segment Diameter
        GUILayout.BeginHorizontal();
        GUILayout.Label("Segment Diameter");
        pipesystem.segmentDiameter = EditorGUILayout.FloatField(pipesystem.segmentDiameter);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);


        //Spare segment probability
        GUILayout.Label("Spare Segment Probabilty: " + spareSegmentProbability.ToString());

        //Update Segment UI
        if (segmentCount != 0)
        {
            for (int i = 0; i < segmentCount; i++)
            {
                GUILayout.BeginHorizontal();

                pipesystem.segmentPrefab[i] = (GameObject)EditorGUILayout.ObjectField(pipesystem.segmentPrefab[i], typeof(GameObject), false);
                pipesystem.segmentProbability[i] = EditorGUILayout.IntSlider(pipesystem.segmentProbability[i], 0, 100);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    DeleteSegment(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }
        }

        //Add Segment
        Rect newArea = GUILayoutUtility.GetRect(10, 50);
        GUI.Box(newArea, "Add new Segment");

        switch (e.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:

                if (!newArea.Contains(e.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (GameObject draggedObject in DragAndDrop.objectReferences)
                    {
                        newGameObject = draggedObject;
                        if (!newGameObject)
                            continue;

                        pipesystem.segmentPrefab.Add(newGameObject);
                        pipesystem.segmentProbability.Add(spareSegmentProbability);
                        oldSegmentProbability.Add(spareSegmentProbability);
                    }
                }
                Event.current.Use();
                break;
        }

        //Delete all
        if (GUILayout.Button("Delete All"))
        {
            pipesystem.segmentPrefab.Clear();
            pipesystem.segmentProbability.Clear();
            oldSegmentProbability.Clear();
        }
    }

    public void CalculateSegmentProbability()
    {
        spareSegmentProbability = 100;


        //subtract all probabilities
        for (int i = 0; i < pipesystem.segmentProbability.Count; i++)
        {
            spareSegmentProbability -= pipesystem.segmentProbability[i];
        }

        //if someone changes and sparesegmentProbability would fall below 0, stop
        for (int i = 0; i < pipesystem.segmentProbability.Count; i++)
        {
            if (pipesystem.segmentProbability[i] != oldSegmentProbability[i])
            {
                if (spareSegmentProbability < 0)
                {
                    pipesystem.segmentProbability[i] -= spareSegmentProbability * (-1);
                    spareSegmentProbability = 0;
                }
                oldSegmentProbability[i] = pipesystem.segmentProbability[i];
            }
        }
    }

    public void DeleteSegment(int position)
    {
        pipesystem.segmentPrefab.RemoveAt(position);
        pipesystem.segmentProbability.RemoveAt(position);
        oldSegmentProbability.RemoveAt(position);
        CalculateSegmentProbability();
    }

    public void Interjacent()
    {
        Event e = Event.current;
        interjacentCount = pipesystem.interjacentPrefab.Count;

        if (interjacentCount > 0)
            CalculateInterjacentProbability();

        //Spare interjacent probability
        GUILayout.Label("Spare interjacent Probabilty: " + spareInterjacentProbability.ToString());

        //Update Interjacent UI
        if (interjacentCount != 0)
        {
            for (int i = 0; i < interjacentCount; i++)
            {
                GUILayout.BeginHorizontal();

                pipesystem.interjacentPrefab[i] = (GameObject)EditorGUILayout.ObjectField(pipesystem.interjacentPrefab[i], typeof(GameObject), false);
                pipesystem.interjacentProbability[i] = EditorGUILayout.IntSlider(pipesystem.interjacentProbability[i], 0, 100);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    DeleteInterjacent(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }
        }

        //Add Interjacent
        Rect newArea = GUILayoutUtility.GetRect(10, 50);
        GUI.Box(newArea, "Add new Interjacent");

        switch (e.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:

                if (!newArea.Contains(e.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (GameObject draggedObject in DragAndDrop.objectReferences)
                    {
                        newGameObject = draggedObject;
                        if (!newGameObject)
                            continue;

                        pipesystem.interjacentPrefab.Add(newGameObject);
                        pipesystem.interjacentProbability.Add(spareInterjacentProbability);
                        oldInterjacentProbability.Add(spareInterjacentProbability);
                    }
                }
                Event.current.Use();
                break;
        }

        //Delete all
        if (GUILayout.Button("Delete All"))
        {
            pipesystem.interjacentPrefab.Clear();
            pipesystem.interjacentProbability.Clear();
            oldInterjacentProbability.Clear();
        }
    }

    public void CalculateInterjacentProbability()
    {
        spareInterjacentProbability = 100;

        //subtract all probabilities
        for (int i = 0; i < pipesystem.interjacentProbability.Count; i++)
        {
            spareInterjacentProbability -= pipesystem.interjacentProbability[i];
        }

        //if someone changes and spareInterjacentProbability would fall below 0, stop
        for (int i = 0; i < pipesystem.interjacentProbability.Count; i++)
        {
            if (pipesystem.interjacentProbability[i] != oldInterjacentProbability[i])
            {
                if (spareInterjacentProbability < 0)
                {
                    pipesystem.interjacentProbability[i] -= spareInterjacentProbability * (-1);
                    spareInterjacentProbability = 0;
                }
                oldInterjacentProbability[i] = pipesystem.interjacentProbability[i];
            }
        }
    }

    public void DeleteInterjacent(int position)
    {
        pipesystem.interjacentPrefab.RemoveAt(position);
        pipesystem.interjacentProbability.RemoveAt(position);
        oldInterjacentProbability.RemoveAt(position);
        CalculateInterjacentProbability();
    }

    public void SavePipestyleAsNew()
    {
        if (spareSegmentProbability > 0)
        {
            Debug.Log("spareSegmentProbability must be 0");
            return;
        }

        //create a new pipestyle instance with the current values
        PipeStyle newStyle = (PipeStyle)CreateInstance("PipesystemStyle");

        newStyle.segmentPrefab = new List<GameObject>();
        newStyle.segmentProbability = new List<int>();
        newStyle.interjacentPrefab = new List<GameObject>();
        newStyle.interjacentProbability = new List<int>();
        newStyle.gizmoColors = new List<Color32>();

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
        GameObject tempGo;
        int tempInt = 0;
        Color32 tempColor = Color.red;
        float tempFloat;
        
        //Material tempMat = new Material(tempShader);

        tempFloat = pipesystem.segmentLength;
        style.segmentLength = tempFloat;

        tempFloat = pipesystem.segmentDiameter;
        style.segmentDiameter = tempFloat;


        //fill segment count if to low
        do
        {
            style.segmentPrefab.Add(emptyPlaceholder);
            style.segmentProbability.Add(tempInt);

        } while (style.segmentPrefab.Count < pipesystem.segmentPrefab.Count);

        //reduce segement count if to high
        while (style.segmentPrefab.Count > pipesystem.segmentPrefab.Count)
        {
            style.segmentPrefab.RemoveAt(style.segmentPrefab.Count - 1);
            style.segmentProbability.RemoveAt(style.segmentProbability.Count - 1);
        }

        //replace segment prefabs
        for (int i = 0; i < pipesystem.segmentPrefab.Count; i++)
        {
            tempGo = pipesystem.segmentPrefab[i];
            style.segmentPrefab[i] = tempGo;

            tempInt = pipesystem.segmentProbability[i];
            style.segmentProbability[i] = tempInt;
        }

        //fill interjacent count
        do
        {
            style.interjacentPrefab.Add(emptyPlaceholder);
            style.interjacentProbability.Add(tempInt);
        } while (style.interjacentPrefab.Count < pipesystem.interjacentPrefab.Count);

        //reduce segment count
        while (style.interjacentPrefab.Count > pipesystem.interjacentPrefab.Count)
        {
            style.interjacentPrefab.RemoveAt(style.interjacentPrefab.Count - 1);
            style.interjacentProbability.RemoveAt(style.interjacentProbability.Count - 1);
        }

        //replace interjacent
        for (int i = 0; i < pipesystem.interjacentPrefab.Count; i++)
        {
            tempGo = pipesystem.interjacentPrefab[i];
            style.interjacentPrefab[i] = tempGo;

            tempInt = pipesystem.interjacentProbability[i];
            style.interjacentProbability[i] = tempInt;
        }

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

        //material
        //tempMat = pipesystem.mainMaterial;
        //style.material = tempMat;
    }

    public void RevertChanges()
    {
        //get the attached pipestyle and change all values to theirs
        int tempInt = 0;
        GameObject tempGo;
        Color32 tempColor;

        pipesystem.segmentLength = usedStyle.segmentLength;
        pipesystem.segmentDiameter = usedStyle.segmentDiameter;

        //fill segment count if to low
        while (pipesystem.segmentPrefab.Count < usedStyle.segmentPrefab.Count)
        {
            pipesystem.segmentPrefab.Add(usedStyle.segmentPrefab[0]);
            pipesystem.segmentProbability.Add(tempInt);
        }

        //reduce segement count if to high
        while (pipesystem.segmentPrefab.Count > usedStyle.segmentPrefab.Count)
        {
            pipesystem.segmentPrefab.RemoveAt(pipesystem.segmentPrefab.Count - 1);
            pipesystem.segmentProbability.RemoveAt(pipesystem.segmentProbability.Count - 1);
        }

        //replace segments
        for (int i = 0; i < pipesystem.segmentProbability.Count; i++)
        {
            tempGo = usedStyle.segmentPrefab[i];
            pipesystem.segmentPrefab[i] = tempGo;

            tempInt = usedStyle.segmentProbability[i];
            pipesystem.segmentProbability[i] = tempInt;
        }

        //fill interjacent count
        while (pipesystem.interjacentPrefab.Count < usedStyle.interjacentPrefab.Count)
        {
            pipesystem.interjacentPrefab.Add(usedStyle.interjacentPrefab[0]);
            pipesystem.interjacentProbability.Add(tempInt);
        }

        //reduce interjacent count
        while (pipesystem.interjacentPrefab.Count > usedStyle.interjacentPrefab.Count)
        {
            pipesystem.interjacentPrefab.RemoveAt(pipesystem.interjacentPrefab.Count - 1);
            pipesystem.interjacentProbability.RemoveAt(pipesystem.interjacentProbability.Count - 1);
        }

        //interjacents
        for (int i = 0; i < pipesystem.interjacentProbability.Count; i++)
        {
            tempGo = usedStyle.interjacentPrefab[i];
            pipesystem.interjacentPrefab[i] = usedStyle.interjacentPrefab[i];

            tempInt = usedStyle.interjacentProbability[i];
            pipesystem.interjacentProbability[i] = usedStyle.interjacentProbability[i];
        }

        //gizmoColors
        for (int i = 0; i < pipesystem.gizmoColors.Count; i++)
        {
            tempColor = usedStyle.gizmoColors[i];
            pipesystem.gizmoColors[i] = tempColor;
        }
    }

    public void DeletePipestyle()
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(pathStyleArray[positionStyleChoser]));

        GetAllPipestyls();
    }

    public void RecalculateAll()
    {
        //recalculate all pipes
    }

    public void GizmoColors()
    {
        GUILayout.BeginVertical();
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

        GUILayout.BeginHorizontal();
        GUILayout.Label("Connection Line");
        pipesystem.gizmoColors[4] = EditorGUILayout.ColorField(pipesystem.gizmoColors[4], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Segment Hover");
        pipesystem.gizmoColors[5] = EditorGUILayout.ColorField(pipesystem.gizmoColors[5], GUILayout.Width(colorfieldWidth));
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
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
