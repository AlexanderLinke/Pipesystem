﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipesystemWindow : EditorWindow {

    //pipesystem
    private bool pipeSystemLinked;
    private GameObject pipeSystemObject;
    private Pipesystem pipesystem;
    private PipesystemManager manager;

    //prefabs
    public GameObject prefabPipeSystem;
    public ControlPoint prefabControlPoint;
    public ConnectionLine prefabConnectionLine;
    public Segment prefabSegment;
    public GameObject emptyGameObject;

    //selection
    private List<ControlPoint> selectedControlPoints = new List<ControlPoint>();
    private InfluencePoint selectedInfluencePoint;
    private bool mousePressed;
    private bool shiftPressed;
    private bool segmentSelection;
    private Segment currentHoverd;
    private bool otherSelection;
    private SnapPoint selectedSnapPoint;

    private bool moveAlongLine;

    //private bool forcePipeUpdate;
    public List<ControlPoint> controlPointsToUpdate = new List<ControlPoint>();

    [MenuItem("Window/PipeSystem")]
    public static void ShowWindow()
    {
        GetWindow<PipesystemWindow>("PipeSystem");
    }

    void OnGUI()
    {
        if (pipesystem == null || pipeSystemObject == null)
            pipeSystemLinked = false;

        if (pipeSystemLinked)
        {

            GUILayout.Label("Control Points", EditorStyles.boldLabel);
            //new and insert
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
                CreateControlPoint();

            if (GUILayout.Button("Insert"))
                InsertControlPoint();

            GUILayout.EndHorizontal();

            //Merge and Bevel
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Merge"))
                MergeControlPoints();

            if (GUILayout.Button("Bevel"))
                BevelControlPoints();

            GUILayout.EndHorizontal();

            //connect and deconnect
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Connect"))
                ConnectControlPoints();

            if (GUILayout.Button("Disconnect"))
                DeleteConnectionBetweenControlPoints();

            GUILayout.EndHorizontal();

            //delete
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete Selected"))
                DeleteControlPoint();

            if (GUILayout.Button("Delete All"))
                DeleteAll();

            GUILayout.EndHorizontal();

            //Move along Line
            GUILayout.BeginHorizontal();

            //GUILayout.Label("Move along Line");
            moveAlongLine = GUILayout.Toggle(moveAlongLine, "Move Along Line");
            if (moveAlongLine)
                MoveAlongLine();

            if (GUILayout.Button("Curve/Uncurve"))
                CurveConnectionLine();

            GUILayout.EndHorizontal();

            //Snappoint
            GUILayout.Space(10);
            GUILayout.Label("Snappoint", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
                CreateSnapPointHolder();

            if (GUILayout.Button("Delete"))
                DeleteSnapPointHolder();

            GUILayout.EndHorizontal();

            //Selection
            GUILayout.Space(10);
            GUILayout.Label("Selection",EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            segmentSelection = GUILayout.Toggle(segmentSelection, "Segments");
            pipesystem.selectControlPointsOnly= GUILayout.Toggle(pipesystem.selectControlPointsOnly, "Control Points only");

            GUILayout.EndHorizontal();

            //Pipesystem
            GUILayout.Space(10);
            GUILayout.Label("Pipesystem", EditorStyles.boldLabel);
            if (GUILayout.Button("Unlink Pipesystem"))
            {
                UnlinkPipeSystem();
                return;
            }
            if (GUILayout.Button("Manage Pipestyle"))
                ManagePipestyle();
        }
        else
        {
            if(manager.activePipesystem != null)
            {
                manager.activePipesystem.isLinked = false;
                manager.activePipesystem = null;
            }

            if (GUILayout.Button("New Pipesystem"))
                CreatePipesystem();

            if (GUILayout.Button("Link PipeSystem"))
                LinkPipeSystem();
        }

    }

    void Update()
    {
        if(pipesystem != null)
        {
            Debugger();
            UpdatePipes();

            if (pipesystem.recalculateAll == true)
                RecalculateAll();

            //keeps snapPoints in place
            if (selectedSnapPoint!=null &&selectedSnapPoint.isAnchord && selectedSnapPoint.position != selectedSnapPoint.transform.localPosition)
                selectedSnapPoint.transform.localPosition = selectedSnapPoint.position;
        }
    }

    public void OnEnable()
    {
        SceneView.onSceneGUIDelegate += ShortcutUpdate;
        SceneView.onSceneGUIDelegate += CheckSelection;
        SceneView.onSceneGUIDelegate += ChangeSegment;

        if (manager == null)
            manager = FindObjectOfType<PipesystemManager>();

        foreach (Pipesystem pipesystem in manager.pipesystems)
        {
            if (pipesystem == null)
            {
                manager.pipesystems.Remove(pipesystem);
                return;
            }

            if(manager.activePipesystem==null)
                pipesystem.isLinked = false;

            if (pipesystem.controlPointPrefab.Count > 0)
                RecalculateControlPointPrefabs(pipesystem);
        }
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= ShortcutUpdate;
        SceneView.onSceneGUIDelegate -= CheckSelection;
        SceneView.onSceneGUIDelegate -= ChangeSegment;
    }

    void ShortcutUpdate(SceneView sceneview)
    {
        Event e = Event.current;

        //int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
        if (e.type == EventType.MouseDown)
        {
            mousePressed = true;
            //Debug.Log("pressed");
        }

        if (e.isKey && e.character == 'e')
        {
            //if (!mousePressed)
            //{
            //    if (pipeSystemLinked)
            //        CreatePipePoint();
            //}
            Debug.Log("e");
        }
        //HandleUtility.AddDefaultControl(controlId);

        if (e.type == EventType.MouseUp)
            mousePressed = false;

       
        

        if (e.isKey && e.character == 'd')
        {
            if (!mousePressed)
                DeleteControlPoint();
        }
    }

    void RecalculateControlPointPrefabs(Pipesystem pipesystem)
    {
        pipesystem.modifiedControlPointPrefab.Clear();
        for (int i = 0; i < pipesystem.controlPointPrefab.Count; i++)
        {
            string datapath;
            datapath = AssetDatabase.GetAssetPath(pipesystem.controlPointPrefab[i]);
            datapath = datapath.Replace(".prefab", "Modified" + ".prefab");
            Object modifiedPrefab = AssetDatabase.LoadAssetAtPath(datapath, typeof(Object));

            GameObject newModifiedPrefab = AutomaticWeightPainting.CalculateWeights(pipesystem, (GameObject)modifiedPrefab);

            PrefabUtility.SaveAsPrefabAsset(newModifiedPrefab, datapath);
            pipesystem.modifiedControlPointPrefab.Add((GameObject)modifiedPrefab);

            DestroyImmediate(newModifiedPrefab);
        }
    }

    public void CheckSelection(SceneView sceneView)
    {
        if (pipesystem == null)
            return;

        //SnapPoint
        if(selectedSnapPoint != null)
        {
            selectedSnapPoint.isSelected = false;
            selectedSnapPoint = null;
        }

        foreach(Object obj in Selection.objects)
        {

            if (obj.GetType()==(typeof(GameObject)))
            {
                GameObject go = (GameObject)obj;
                SnapPoint snapPoint = go.GetComponent<SnapPoint>();


                if (snapPoint != null && snapPoint.connectedControlPoint == null)
                {
                    if (selectedSnapPoint != null)
                        selectedSnapPoint.isSelected = false;
                    selectedSnapPoint = snapPoint;
                    snapPoint.isSelected = true;
                }
            }
        }

        //Select all 
        if (!pipesystem.selectControlPointsOnly)
        {
            selectedControlPoints.Clear();
            
            foreach (ControlPoint controlPoint in pipesystem.controlPoints)
            {
               
                foreach(Object go in Selection.objects)
                {
                    if (controlPoint.gameObject == go)
                    {
                        controlPoint.isSelectedControlPoint = true;
                        selectedControlPoints.Add(controlPoint);
                    }

                    GameObject influencePointObject = (GameObject)go;
                    if (influencePointObject != null)
                    {
                        InfluencePoint influencePoint = influencePointObject.GetComponent<InfluencePoint>();
                        if(influencePoint!=null)
                        {
                            selectedInfluencePoint = influencePoint;
                        }
                    }
                }
                if (!selectedControlPoints.Contains(controlPoint))
                    controlPoint.isSelectedControlPoint = false;
            }
        }
        
        //select ControlPoints only
        if (pipesystem.selectControlPointsOnly)
        {
            Object[] objects;
            objects = Selection.objects;
            selectedInfluencePoint = null;

            foreach(ControlPoint controlPoint in pipesystem.controlPoints)
                controlPoint.isSelectedControlPoint = false;

            foreach (ControlPoint controlPoint in pipesystem.controlPoints)
                foreach (ConnectionLine connectionLine in controlPoint.connectionLines)
                {
                    //if (connectionLine.startInfluencePoint != null)
                    //{
                    //    connectionLine.startInfluencePoint.GetComponent<InfluencePoint>().isSelected = false;
                    //    connectionLine.endInfluencePoint.GetComponent<InfluencePoint>().isSelected = false;
                    //}
                }

            selectedControlPoints.Clear();

            foreach (Object totalSelected in objects)
            {
                GameObject gameObject = (GameObject)totalSelected;

                ControlPoint controlPoint = gameObject.GetComponent<ControlPoint>();

                Pipesystem system = gameObject.GetComponent<Pipesystem>();
                if (system!=null)
                {
                    Selection.activeGameObject = pipeSystemObject;
                    return;
                }

                InfluencePoint influencePoint = gameObject.GetComponent<InfluencePoint>();
                if (influencePoint != null)
                {
                    influencePoint.isSelected = true;
                    Object[] selection = new Object[1];
                    selection[0] = influencePoint.gameObject;
                    Selection.objects = selection;
                    selectedInfluencePoint = influencePoint;
                    
                    return;
                }

                if (controlPoint != null)
                {
                    selectedControlPoints.Add(controlPoint);
                    controlPoint.isSelectedControlPoint = true;
                }
            }

            //set the selection
            Object[] newSelection;
            if (selectedSnapPoint!=null)
            {
                newSelection = new Object[selectedControlPoints.Count + 1];

                for (int i = 0; i < selectedControlPoints.Count; i++)
                    newSelection[i] = selectedControlPoints[i].gameObject;

                newSelection[selectedControlPoints.Count] = selectedSnapPoint.gameObject;
            }
            else
            {
                newSelection = new Object[selectedControlPoints.Count];

                for (int i = 0; i < selectedControlPoints.Count; i++)
                    newSelection[i] = selectedControlPoints[i].gameObject;
            }

            Selection.objects = newSelection;
        }

        //Segment selection
        if(segmentSelection)
        {
            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit = new RaycastHit();

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Segment segment = hit.collider.GetComponentInParent<Segment>();

                if (segment != null && segment != currentHoverd)
                {
                    segment.isHoverd = true;

                    if(currentHoverd != null)
                    {
                        currentHoverd.isHoverd = false;
                    }

                    currentHoverd = segment;
                }
            }
            else
            {
                if (currentHoverd != null)
                    currentHoverd.isHoverd = false;
                currentHoverd = null;
            }
        }
    }

    public void CreatePipesystem()
    {
        GameObject newPipesystem;
        newPipesystem = Instantiate(prefabPipeSystem);
        newPipesystem.name = "Pipesystem";
        Selection.activeGameObject = newPipesystem;
        LinkPipeSystem();
        pipesystem.controlPoints = new List<ControlPoint>();
        CreateControlPoint();
        manager.pipesystems.Add(newPipesystem.GetComponent<Pipesystem>());
        manager.activePipesystem = newPipesystem.GetComponent<Pipesystem>();
    }

    public void LinkPipeSystem()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            Debug.Log("Please select a Pipesystem");
            return;
        }

        pipesystem = selectedObject.GetComponent<Pipesystem>();
        pipeSystemObject = selectedObject;

        manager.activePipesystem = pipesystem;

        if (!(pipesystem == null || pipeSystemObject == null))
        {
            pipeSystemLinked = true;
            pipesystem.isLinked = true;
            
            //bugfix where after restart the controlPoints list was empty although there were controlPoints 
            int controlPointCount = pipesystem.controlPointHolder.transform.childCount;

            if( controlPointCount != 0 && pipesystem.controlPoints == null || pipesystem.controlPoints.Count != controlPointCount)
            {
                pipesystem.controlPoints.Clear();
                for(int i = 0; i < controlPointCount; i++)
                {
                    pipesystem.controlPoints.Add(pipesystem.controlPointHolder.transform.GetChild(i).gameObject.GetComponent<ControlPoint>());
                }

                RemoveMissingObjectsFromList();
                RenameControlPoints();
            }
        }
        else
            Debug.Log("This Object has no Pipesystem attached");
    }

    public void UnlinkPipeSystem()
    {
        pipesystem.isLinked = false;
        pipesystem = null;
        pipeSystemObject = null;
        pipeSystemLinked = false;
        manager.activePipesystem = null;

        foreach (ControlPoint controlPoint in selectedControlPoints)
            controlPoint.isSelectedControlPoint = false;

        selectedControlPoints.Clear();
    }

    public void CreateControlPoint()
    {
        Vector3 spawnPosition = Vector3.zero;
        ControlPoint newControlPoint = null;

        //if there is not already a controlPoint spawn one
        if (pipesystem.controlPoints.Count == 0 && selectedSnapPoint == null)
        {
            spawnPosition = pipeSystemObject.transform.position;
            newControlPoint = CreateControlPointBasic(spawnPosition);
            newControlPoint.indexInPipesystem = 1;
            RenameControlPoints();
            Selection.activeObject = newControlPoint;
        }
        else if (selectedControlPoints.Count == 0)
        {
            if(selectedSnapPoint != null)
            {
                ControlPoint newSnapPoint= CreateSnapPointBasic();
                newControlPoint = CreateControlPointBasic(selectedSnapPoint.transform.position);
                CreateConnectionLine(newSnapPoint, newControlPoint);
                Selection.activeObject = newControlPoint;
            }
            else
                Debug.Log("No controlPoint selected");
        }
        else
        {
            //create a new controlPoint foreach selected controlPoint 
            GameObject[] newSelected = new GameObject[selectedControlPoints.Count];

            int indexOfNewSelected = 0;

            foreach (ControlPoint controlPoint in selectedControlPoints)
            {
                spawnPosition = controlPoint.transform.position;
                newControlPoint = CreateControlPointBasic(spawnPosition);

                CreateConnectionLine(controlPoint, newControlPoint);

                newSelected[indexOfNewSelected] = newControlPoint.gameObject;
                indexOfNewSelected++;
            }
            //select the new controlPoints
            selectedControlPoints.Clear();

            foreach (GameObject go in newSelected)
                selectedControlPoints.Add(go.GetComponent<ControlPoint>());

            Selection.objects = newSelected;
        }
    }

    ControlPoint CreateControlPointBasic(Vector3 spawnPosition)
    {
        ControlPoint newControlPoint;

        newControlPoint = Instantiate(prefabControlPoint, spawnPosition, Quaternion.identity, pipesystem.controlPointHolder.transform) as ControlPoint;
        pipesystem.controlPoints.Add(newControlPoint);
        newControlPoint.correspondingPipesystem = pipesystem;
        newControlPoint.oldPosition = spawnPosition;
        newControlPoint.gizmoSize = pipesystem.gizmoSizeControlPoints;
        newControlPoint.models = new List<GameObject>();
        newControlPoint.modelConnectionPoints = new List<GameObject>();

        GameObject newControlPointModelHolder = Instantiate(emptyGameObject, pipesystem.controlPointModelHolder.transform);
        newControlPoint.modelHolder = newControlPointModelHolder;

        

        return newControlPoint;
    }

    ControlPoint CreateSnapPointBasic()
    {
        ControlPoint newControlPoint;

        newControlPoint = CreateControlPointBasic(selectedSnapPoint.transform.position);
        newControlPoint.snapPoint = selectedSnapPoint;
        selectedSnapPoint.connectedControlPoint = newControlPoint;

        return newControlPoint;
    }

    public void ConnectControlPoints()
    {
        //if two controlPoints are selecetd and not already connected
        if(selectedControlPoints.Count == 2 && !selectedControlPoints[0].connectedControlPoints.Contains(selectedControlPoints[1]))
            CreateConnectionLine(selectedControlPoints[0], selectedControlPoints[1]);

        //if open snapPoint and 1 controlPoint are selected
        if(selectedControlPoints.Count==1 && selectedSnapPoint!=null)
        {
            ControlPoint newControlPoint = CreateSnapPointBasic();
            CreateConnectionLine(selectedControlPoints[0], newControlPoint);
        }
    }

    public void DeleteConnectionBetweenControlPoints()
    {
        //if two controlPoints are selecetd and connected, delete the connection
        if (selectedControlPoints.Count == 2 && selectedControlPoints[0].connectedControlPoints.Contains(selectedControlPoints[1]))
        {
            foreach(ConnectionLine connectionLine in selectedControlPoints[0].connectionLines)
            {
                if(selectedControlPoints[1].connectionLines.Contains(connectionLine))
                {
                    DeleteConnectionLine(connectionLine);
                    UpdateControlPointModel(selectedControlPoints[0]);
                    UpdateControlPointModel(selectedControlPoints[1]);
                    break;
                }
            }
        }
    }

    public void InsertControlPoint()
    {
        if(selectedControlPoints.Count==2 && selectedControlPoints[0].connectedControlPoints.Contains(selectedControlPoints[1]))
        {
            //create the new controlPoint
            Vector3 spawnPosition = (selectedControlPoints[0].transform.position + selectedControlPoints[1].transform.position)/2;
            ControlPoint newControlPoint = CreateControlPointBasic(spawnPosition);

            //delete the old pipeline
            foreach(ConnectionLine connectionLine in selectedControlPoints[0].connectionLines)
            {
                if(selectedControlPoints[1].connectionLines.Contains(connectionLine))
                {
                    DeleteConnectionLine(connectionLine);
                    break;
                }
            }

            //Create new pipeLines
            CreateConnectionLine( selectedControlPoints[0], newControlPoint);
            CreateConnectionLine( newControlPoint, selectedControlPoints[1]);

            //Selection
            selectedControlPoints.Clear();
            selectedControlPoints.Add(newControlPoint);
            Selection.activeGameObject = newControlPoint.gameObject;
        }
    }

    public void MergeControlPoints()
    {
        //Deletes the two selected controlpoints and creates a new one between them with all their connections
        if(selectedControlPoints.Count == 2)
        {
            foreach (ControlPoint controlPoint in selectedControlPoints)
                if (controlPoint.snapPoint != null)
                    return;

            Vector3 newPosition = (selectedControlPoints[0].transform.position + selectedControlPoints[1].transform.position)/2;
            ControlPoint newControlPoint;

            //Get all connections
            List<ControlPoint> newConnecteControlPoints = new List<ControlPoint>();
            foreach(ControlPoint controlPoint in selectedControlPoints)
            {
                foreach(ControlPoint connectedControlPoint in controlPoint.connectedControlPoints)
                {
                    if(!newConnecteControlPoints.Contains(connectedControlPoint) && !selectedControlPoints.Contains(connectedControlPoint))
                        newConnecteControlPoints.Add(connectedControlPoint);
                }
            }

            //Delete old connections and contolPoints
            foreach (ControlPoint controlPoint in selectedControlPoints)
            {
                while(controlPoint.connectionLines.Count > 0)
                        DeleteConnectionLine(controlPoint.connectionLines[controlPoint.connectionLines.Count-1]);
            }
            DeleteControlPoint();

            //create new controlPoint and connections
            newControlPoint = CreateControlPointBasic(newPosition);

            for(int i = 0; i < newConnecteControlPoints.Count; i++)
            {
                CreateConnectionLine(newConnecteControlPoints[i], newControlPoint);
            }

            Selection.activeGameObject = newControlPoint.gameObject;
        }
    }

    public void BevelControlPoints()
    {
        //implement
    }

    public void RenameControlPoints()
    {
        for (int i = 1; i <= pipesystem.controlPoints.Count; i++)
        {
            pipesystem.controlPoints[i - 1].name = "ControlPoint " + i;
            pipesystem.controlPoints[i - 1].indexInPipesystem = i;
            pipesystem.controlPoints[i - 1].modelHolder.name = "ControlPointModel " + i;
        }

        foreach (ControlPoint controlPoint in pipesystem.controlPoints)
        {
            foreach (ConnectionLine connectionLine in controlPoint.connectionLines)
            {
                foreach (ControlPoint connectedPipePoint in controlPoint.connectedControlPoints)
                {
                    if (connectedPipePoint.connectionLines.Contains(connectionLine))
                    {
                        connectionLine.name = "ConnectionLine " + connectedPipePoint.indexInPipesystem + " - " + controlPoint.indexInPipesystem;
                    }
                }
            }
        }
    }

    public void DeleteAll()
    {
        //harddeletes everything
        while (pipesystem.controlPointHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.controlPointHolder.transform.GetChild(0).gameObject);

        while (pipesystem.connectionLineHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.connectionLineHolder.transform.GetChild(0).gameObject);

        while (pipesystem.snapPointsHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.snapPointsHolder.transform.GetChild(0).gameObject);

        while (pipesystem.controlPointModelHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.controlPointModelHolder.transform.GetChild(0).gameObject);

        pipesystem.controlPoints.Clear();
        selectedControlPoints.Clear();
        controlPointsToUpdate.Clear();
        Selection.activeObject = pipeSystemObject;
    }

    void MoveAlongLine()
    {

    }

    void CurveConnectionLine()
    {
        if (selectedControlPoints.Count == 2 && selectedControlPoints[0].connectedControlPoints.Contains(selectedControlPoints[1]))
        {
            foreach (ConnectionLine connectionLine in selectedControlPoints[0].connectionLines)
            {
                if (selectedControlPoints[1].connectionLines.Contains(connectionLine))
                {
                    if (connectionLine.isCurved == true)
                    {
                        connectionLine.isCurved = false;
                        DestroyImmediate(connectionLine.startInfluencePoint.gameObject);
                        DestroyImmediate(connectionLine.endInfluencePoint.gameObject);
                    }
                    else
                    {
                        connectionLine.isCurved = true;
                        connectionLine.startInfluencePoint = Instantiate(emptyGameObject, connectionLine.startControlPoint.transform);
                        connectionLine.startInfluencePoint.name = "StartInfluencePoint";
                        connectionLine.startInfluencePoint.transform.position = connectionLine.startPosition.transform.position;
                        InfluencePoint startInfluencePoint = connectionLine.startInfluencePoint.AddComponent<InfluencePoint>();
                        startInfluencePoint.correspondingPipesystem = pipesystem;

                        connectionLine.endInfluencePoint = Instantiate(emptyGameObject, connectionLine.endControlPoint.transform);
                        connectionLine.endInfluencePoint.name = "EndInfluencePoint";
                        connectionLine.endInfluencePoint.transform.position = connectionLine.endPosition.transform.position;
                        InfluencePoint endInfluencePoint = connectionLine.endInfluencePoint.AddComponent<InfluencePoint>();
                        endInfluencePoint.correspondingPipesystem = pipesystem;
                    }

                    foreach (Segment segment in connectionLine.segments)
                    {
                        DestroyImmediate(segment.model.gameObject);
                        DestroyImmediate(segment.gameObject);

                    }
                    connectionLine.segments.Clear();

                }
            }
        }
        controlPointsToUpdate.Add(selectedControlPoints[0]);
    }

    public void CreateSnapPointHolder()
    {
        if(selectedControlPoints.Count==1)
        {
            SnapPoint snapPoint;

            if(selectedControlPoints[0].snapPoint != null)
            {
                Debug.Log("Already connected to a SnapPoint");
                return;
            }

            GameObject newSnapPointHolderPrefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.snappablePrefab[0]);
            newSnapPointHolderPrefab.transform.parent = pipesystem.snapPointsHolder.transform;
            snapPoint = newSnapPointHolderPrefab.GetComponent<SnapPointHolder>().snapPoints[0];
            newSnapPointHolderPrefab.transform.position = selectedControlPoints[0].transform.position;

            Vector3 direction = (snapPoint.transform.position - newSnapPointHolderPrefab.transform.position).normalized;
            Debug.Log(direction);
            Vector3 goal = (selectedControlPoints[0].connectedControlPoints[0].transform.position-newSnapPointHolderPrefab.transform.position).normalized;
            Debug.Log("goal: " + goal);
            newSnapPointHolderPrefab.transform.rotation = Quaternion.LookRotation(goal);
            Vector3 r1 = newSnapPointHolderPrefab.transform.rotation.eulerAngles.normalized;
            Debug.Log(r1);
            //newSnapPointHolderPrefab.transform.rotation
        }
    }

    public void DeleteSnapPointHolder()
    {
        //implement
    }

    public void RemoveMissingObjectsFromList()
    {
        for (int i = pipesystem.controlPoints.Count - 1; i >= 0; i--)
        {
            if (pipesystem.controlPoints[i] == null)
                pipesystem.controlPoints.RemoveAt(i);
        }
        RenameControlPoints();
    }

    public void DeleteControlPoint()
    {
        List<ControlPoint> controlPointsToDelete = new List<ControlPoint>();

        GameObject[] newSelectedControlPoint = new GameObject[1];

        foreach (ControlPoint controlPoint in selectedControlPoints)
        {
            if (controlPoint != null)
            {
                switch (controlPoint.connectedControlPoints.Count)
                {
                    case 0:
                        {
                            controlPointsToDelete.Add(controlPoint);
                            Selection.activeGameObject = pipeSystemObject;
                        }
                        break;
                    case 1:
                        {
                            newSelectedControlPoint[0] = controlPoint.connectedControlPoints[0].gameObject;
                            DeleteConnectionLine(controlPoint.connectionLines[0]);
                            controlPointsToDelete.Add(controlPoint);
                            
                        }
                        break;
                    case 2:
                        {
                            //if the selected controlPoint sits between two other controlPoints, delete it and connect the others
                            ControlPoint newStartControlPoint = controlPoint.connectedControlPoints[0];
                            ControlPoint newEndControlPoint = controlPoint.connectedControlPoints[1];

                            DeleteConnectionLine(controlPoint.connectionLines[1]);
                            DeleteConnectionLine(controlPoint.connectionLines[0]);

                            CreateConnectionLine(newStartControlPoint, newEndControlPoint);
                            controlPointsToDelete.Add(controlPoint);

                            newSelectedControlPoint[0] = newStartControlPoint.gameObject;
                            controlPointsToUpdate.Add(newStartControlPoint);
                        }
                        break;
                    default:
                        {
                            // if there are 3 or more connected controlPoints delete all connections
                            while(controlPoint.connectionLines.Count > 0)
                                DeleteConnectionLine(controlPoint.connectionLines[controlPoint.connectionLines.Count - 1]);

                            controlPointsToDelete.Add(controlPoint);
                        }
                        break;
                }
            }
        }

        while (controlPointsToDelete.Count > 0)
        {
            while(controlPointsToDelete[controlPointsToDelete.Count - 1].models.Count > 0)
            {
                DestroyImmediate(controlPointsToDelete[controlPointsToDelete.Count - 1].models[0].gameObject);
                controlPointsToDelete[controlPointsToDelete.Count - 1].models.RemoveAt(controlPointsToDelete[controlPointsToDelete.Count - 1].models.Count - 1);
            }
            controlPointsToDelete[controlPointsToDelete.Count - 1].models.Clear();

            DestroyImmediate(controlPointsToDelete[controlPointsToDelete.Count - 1].gameObject);
            controlPointsToDelete.RemoveAt(controlPointsToDelete.Count - 1);
        }

        selectedControlPoints.Clear();
        if (newSelectedControlPoint[0]!=null)
        {
            foreach(GameObject controlPoint in newSelectedControlPoint)
                selectedControlPoints.Add(controlPoint.GetComponent<ControlPoint>());

            Selection.objects = newSelectedControlPoint;

            
        }
        RemoveMissingObjectsFromList();

        foreach(ControlPoint controlPoint in pipesystem.controlPoints)
            controlPointsToUpdate.Add(controlPoint);
    }

    public void DeleteConnectionLine(ConnectionLine connectionLine)
    {
        //Clear objects from Lists
        foreach (Segment segment in connectionLine.segments)
            DestroyImmediate(segment);

        foreach (GameObject interjacent in connectionLine.interjacents)
            DestroyImmediate(interjacent);

        DestroyImmediate(connectionLine.SegmentHolder);
        DestroyImmediate(connectionLine.InterjacentHolder);

        connectionLine.segments.Clear();
        connectionLine.interjacents.Clear();

        //Remove connections
        connectionLine.startControlPoint.connectedControlPoints.Remove(connectionLine.endControlPoint);
        connectionLine.endControlPoint.connectedControlPoints.Remove(connectionLine.startControlPoint);

        connectionLine.startControlPoint.modelConnectionPoints.Remove(connectionLine.startPosition);
        connectionLine.endControlPoint.modelConnectionPoints.Remove(connectionLine.endPosition);

        connectionLine.startControlPoint.connectionLines.Remove(connectionLine);
        connectionLine.endControlPoint.connectionLines.Remove(connectionLine);

        //destroy
        DestroyImmediate(connectionLine.gameObject);
    }

    public void ManagePipestyle()
    {
        PipeStyleWindow pipeStyle = GetWindow<PipeStyleWindow>("Pipe Style");
        pipeStyle.usedStyle = pipesystem.pipeStyle;
        pipeStyle.pipesystem = pipesystem;
    }

    public void CreateConnectionLine(ControlPoint controlPoint_1, ControlPoint controlPoint_2)
    {
        ConnectionLine newConnectionLine;

        RenameControlPoints();

        //create and setup new connectionLine
        newConnectionLine = Instantiate(prefabConnectionLine, pipesystem.connectionLineHolder.transform);
        newConnectionLine.correspondingPipesystem = pipesystem;
        newConnectionLine.startControlPoint = controlPoint_1;
        newConnectionLine.endControlPoint = controlPoint_2;
        newConnectionLine.startPosition = Instantiate(emptyGameObject, newConnectionLine.endControlPoint.transform.position, newConnectionLine.startControlPoint.transform.rotation, newConnectionLine.transform);
        newConnectionLine.startPosition.name = "StartPosition";
        newConnectionLine.segmentEndPosition = Instantiate(emptyGameObject, newConnectionLine.startControlPoint.transform.position, newConnectionLine.startControlPoint.transform.rotation, newConnectionLine.transform);
        newConnectionLine.segmentEndPosition.name = "SegmentEndPosition";
        newConnectionLine.endPosition = Instantiate(emptyGameObject, newConnectionLine.startControlPoint.transform.position, newConnectionLine.startControlPoint.transform.rotation, newConnectionLine.transform);
        newConnectionLine.endPosition.name = "EndPosition";


        newConnectionLine.name = "ConnectionLine " + controlPoint_1.indexInPipesystem + " - " + controlPoint_2.indexInPipesystem;

        //setup controllPoint referenzes
        controlPoint_1.connectedControlPoints.Add(controlPoint_2);
        controlPoint_2.connectedControlPoints.Add(controlPoint_1);
        controlPoint_1.connectionLines.Add(newConnectionLine);
        controlPoint_2.connectionLines.Add(newConnectionLine);
        controlPoint_1.modelConnectionPoints.Add(newConnectionLine.startPosition);
        controlPoint_2.modelConnectionPoints.Add(newConnectionLine.endPosition);

        controlPointsToUpdate.Add(controlPoint_1);
    }

    Vector3 GetPointOnCurve(Vector3[] points, float percentageValue)
    {
        float steps = (float)1 / points.Length;
        
        int index = 1;
        while(percentageValue > index * steps)
            index++;

        float rest = index * steps - percentageValue;

        if(index >= points.Length)
        {
            return Vector3.zero;
        }
        float dis = Vector3.Distance(points[index-1], points[index]);
        float precentBetweenTwo = (1 - rest / dis);
        //Debug.Log(precentBetweenTwo);

        Vector3 point = (1 - precentBetweenTwo) * points[index-1] + points[index] * precentBetweenTwo;

        return point;
    }

    Vector3 GetRotationOnCurve(Vector3[]points, float percentageValue)
    {
        Vector3 rotation = Vector3.zero;

        return rotation;
    }

    public void UpdatePipes()
    {
        foreach (ControlPoint controlPoint in pipesystem.controlPoints)
        {
            if(controlPoint.snapPoint!=null && controlPoint.snapPoint.transform.position != controlPoint.transform.position)
                    controlPoint.transform.position = controlPoint.snapPoint.transform.position;

            if (controlPoint.oldPosition != controlPoint.transform.position)
            {
                controlPointsToUpdate.Add(controlPoint);
                controlPoint.oldPosition = controlPoint.transform.position;
                
            }
        }
        if(selectedInfluencePoint!=null)
        {
            Debug.Log("shit");
            controlPointsToUpdate.Add(selectedInfluencePoint.transform.parent.GetComponent<ControlPoint>());
        }

        foreach (ControlPoint controlPoint in controlPointsToUpdate)
        {
            foreach (ConnectionLine connectionLine in controlPoint.connectionLines)
            {
                //setup distance
                bool shortendSegment = false;
                float distance = 0;
                float restDistance = 0;
                int segmentCount = 0;

                Vector3 newPointPosition = controlPoint.transform.position;

                Vector3 startPosition;
                Vector3 segmentEndPosition;
                Vector3 endPosition;

                int iterationCount = 1000;
                Vector3[] points = new Vector3[iterationCount];
                float controlPointOffset =0;

                if (!connectionLine.isCurved)
                {
                    //linearLine
                    distance = Vector3.Distance(connectionLine.startControlPoint.transform.position, connectionLine.endControlPoint.transform.position);

                    restDistance = distance;

                    if (connectionLine.startControlPoint.snapPoint == null)
                        restDistance -= pipesystem.distanceSegmentsControlPoint;
                    if (connectionLine.endControlPoint.snapPoint == null)
                        restDistance -= pipesystem.distanceSegmentsControlPoint;


                    if(restDistance>pipesystem.segmentLength)
                         segmentCount = (int)(restDistance / pipesystem.segmentLength);

                    if (restDistance > 0)
                    {
                        UpdateShortendSegment();
                        shortendSegment = true;
                    }

                    while (restDistance > pipesystem.segmentLength)
                    {
                        restDistance -= pipesystem.segmentLength;
                        if(pipesystem.segmentLength <= 0)
                        {
                            Debug.Log("Please increase your Segment Length");
                            return;
                        }
                    }
                    //setup startPosition
                    GameObject startPoint = connectionLine.startPosition;
                    if (connectionLine.endControlPoint.transform.position != Vector3.zero)
                        startPoint.transform.rotation = Quaternion.LookRotation(connectionLine.endControlPoint.transform.position);
                    startPoint.transform.position = connectionLine.startControlPoint.transform.position;
                    startPoint.transform.position = Vector3.MoveTowards(startPoint.transform.position, connectionLine.endControlPoint.transform.position, pipesystem.distanceSegmentsControlPoint);
                    startPosition = startPoint.transform.position;

                    //setup segmentEndPosition
                    GameObject segmentEndPoint = connectionLine.segmentEndPosition;
                    if (connectionLine.startControlPoint.transform.position != Vector3.zero)
                        segmentEndPoint.transform.rotation = Quaternion.LookRotation(connectionLine.startControlPoint.transform.position);

                    segmentEndPoint.transform.position = connectionLine.endControlPoint.transform.position;
                    segmentEndPoint.transform.position = Vector3.MoveTowards(segmentEndPoint.transform.position, connectionLine.startControlPoint.transform.position, pipesystem.distanceSegmentsControlPoint + restDistance);
                    segmentEndPosition = segmentEndPoint.transform.position;

                    //setup endPosition
                    GameObject endPoint = connectionLine.endPosition;
                    if (connectionLine.startControlPoint.transform.position != Vector3.zero)
                        endPoint.transform.rotation = Quaternion.LookRotation(connectionLine.startControlPoint.transform.position);

                    endPoint.transform.position = connectionLine.endControlPoint.transform.position;
                    endPoint.transform.position = Vector3.MoveTowards(endPoint.transform.position, connectionLine.startControlPoint.transform.position, pipesystem.distanceSegmentsControlPoint);
                    endPosition = endPoint.transform.position;

                    controlPoint.oldPosition = controlPoint.transform.position;
                    connectionLine.distance = distance;



                    //Update ControlPointModel
                    UpdateControlPointModel(controlPoint);

                    if (connectionLine.startControlPoint == controlPoint)
                        UpdateControlPointModel(connectionLine.endControlPoint);
                    else
                        UpdateControlPointModel(connectionLine.startControlPoint);


                    // add new segments
                    while (connectionLine.segments.Count < segmentCount)
                    {
                        int randomSegmentPrefabIndex = 0;

                        //if segmentPrefabs contains more then 1, get one random based on their probability
                        if (pipesystem.segmentProbability.Count > 1)
                        {
                            int randomNumber = Random.Range(0, 100);
                            int index = 0;

                            while (randomNumber >= 0 && index < pipesystem.segmentProbability.Count)
                            {
                                randomNumber -= pipesystem.segmentProbability[index];

                                if (randomNumber < 0)
                                {
                                    randomSegmentPrefabIndex = index;
                                }
                                else
                                    index++;
                            }
                        }
                        Segment newSegment = Instantiate(prefabSegment, connectionLine.SegmentHolder.transform);

                        GameObject newSegmentPrefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.segmentPrefab[randomSegmentPrefabIndex]);
                        newSegmentPrefab.transform.parent = newSegment.gameObject.transform;
                        newSegmentPrefab.transform.localPosition = Vector3.zero;

                        newSegment.pipesystem = pipesystem;
                        newSegment.length = pipesystem.segmentLength;
                        newSegment.diameter = pipesystem.segmentDiameter;
                        newSegment.segmentNumber = randomSegmentPrefabIndex;
                        newSegment.model = newSegmentPrefab;

                        if (controlPoint == connectionLine.endControlPoint)
                        {
                            connectionLine.segments.Add(newSegment);
                            newSegment.indexInLine = connectionLine.segments.Count;
                        }
                        else
                        {
                            connectionLine.segments.Insert(0, newSegment);
                            foreach (Segment segment in connectionLine.segments)
                                segment.indexInLine++;
                        }
                    }

                    //remove segments
                    while (connectionLine.segments.Count > segmentCount && connectionLine.segments.Count > 0)
                    {
                        if (controlPoint == connectionLine.endControlPoint)
                        {
                            DestroyImmediate(connectionLine.segments[connectionLine.segments.Count - 1].gameObject);
                            connectionLine.segments.RemoveAt(connectionLine.segments.Count - 1);
                        }
                        else
                        {
                            DestroyImmediate(connectionLine.segments[0].gameObject);
                            connectionLine.segments.RemoveAt(0);
                            foreach (Segment segment in connectionLine.segments)
                                segment.indexInLine--;
                        }
                    }

                    //rename segments
                    foreach (Segment segment in connectionLine.segments)
                        segment.name = "Segment " + segment.indexInLine;

                    //adjust segments position and rotation
                    if (!connectionLine.isCurved)
                    {
                        for (int i = 1; i <= segmentCount; i++)
                        {
                            Vector3 newSegmentPosition = ((((((segmentCount - i) * startPosition) + (i * segmentEndPosition)) / segmentCount) * ((2 * i) - 1)) + startPosition) / (2 * i);
                            connectionLine.segments[i - 1].transform.position = newSegmentPosition;

                            connectionLine.segments[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newSegmentPosition);
                        }
                    }
                }
                else
                {
                    //curvedLine
                    float percentageValue = 0;
                    //int iterationCount = 1000;
                    //Vector3[] points = new Vector3[iterationCount];

                    Vector3 startPoint = connectionLine.startControlPoint.transform.position;
                    Vector3 endPoint = connectionLine.endControlPoint.transform.position;

                    Vector3 influencePoint1 = connectionLine.startInfluencePoint.transform.position;
                    Vector3 influencePoint2 = connectionLine.endInfluencePoint.transform.position;

                    Vector3[] curvePoints = new Vector3[iterationCount];
                    Vector3[] curveRotations = new Vector3[iterationCount];

                    //create the bezier Curve
                    for (int i = 0; i < iterationCount; i++)
                    {
                        float t = (float)i / (iterationCount - 1);

                        float zmt = 1 - t;
                        float zmt2 = zmt * zmt;
                        float t2 = t * t;
                        curvePoints[i] = startPoint * (zmt2 * zmt) + influencePoint1 * (3 * zmt2 * t) + influencePoint2 * (3 * zmt * t2) + endPoint * (t2 * t);

                        curveRotations[i] = startPoint * (-zmt2) + influencePoint1 * (3 * zmt2 - 2 * zmt) + influencePoint2 * (-3 * t2 + 2 * t) + endPoint * (t2);
                        curveRotations[i] = curveRotations[i].normalized;
                    }

                    for (int i = 0; i < curvePoints.Length-1; i++)
                    {
                        distance = distance + Vector3.Distance(curvePoints[i], curvePoints[i + 1]);
                    }

                    //set up the percentage correct curve
                    
                    float stepSize = distance / iterationCount;
                    float distanceCurvePoints = 0;
                    float rest = 0;
                    int point = -1;
                    float dis = 0;

                    points[0] = curvePoints[0];
                    for (int i = 1; i < iterationCount; i++)
                    {
                        while (distanceCurvePoints < stepSize)
                        {
                            point++;
                            distanceCurvePoints = distanceCurvePoints + Vector3.Distance(curvePoints[point], curvePoints[point + 1]);
                        }

                        rest = distanceCurvePoints - stepSize;
                        dis = Vector3.Distance(curvePoints[point], curvePoints[point+1]);
                        points[i] = (1 - (1- rest /dis )) * curvePoints[point] + curvePoints[point+1] * (1-rest/dis);
                        distanceCurvePoints = rest;
                    }

                    //get the segmentcount
                    restDistance = distance;

                    if (connectionLine.startControlPoint.snapPoint == null)
                        restDistance -= pipesystem.distanceSegmentsControlPoint;
                    if (connectionLine.endControlPoint.snapPoint == null)
                        restDistance -= pipesystem.distanceSegmentsControlPoint;


                    if (restDistance > pipesystem.segmentLength)
                        segmentCount = (int)(restDistance / pipesystem.segmentLength);

                    if (restDistance > 0)
                    {
                        UpdateShortendSegment();
                        shortendSegment = true;
                    }

                    while (restDistance > pipesystem.segmentLength)
                    {
                        restDistance -= pipesystem.segmentLength;
                        if (pipesystem.segmentLength <= 0)
                        {
                            Debug.Log("Please increase your Segment Length");
                            return;
                        }
                    }

                    //start position of connectionline
                    percentageValue = pipesystem.distanceSegmentsControlPoint / distance;
                    startPosition = GetPointOnCurve(points, percentageValue);
                    connectionLine.startPosition.transform.position = startPosition;
               
                    //end position of ConnectionLine
                    percentageValue = (distance-pipesystem.distanceSegmentsControlPoint) / distance;
                    endPosition = GetPointOnCurve(points, percentageValue);
                    connectionLine.endPosition.transform.position = endPosition;

                    //segmentend position of connectionLine
                    percentageValue = (segmentCount*pipesystem.segmentLength + pipesystem.distanceSegmentsControlPoint / distance) / distance;
                    segmentEndPosition = GetPointOnCurve(points, percentageValue);
                    connectionLine.segmentEndPosition.transform.position = segmentEndPosition;

                    controlPoint.oldPosition = controlPoint.transform.position;
                    connectionLine.distance = distance;


                    //Update ControlPointModel
                    UpdateControlPointModel(controlPoint);

                    if (connectionLine.startControlPoint == controlPoint)
                        UpdateControlPointModel(connectionLine.endControlPoint);
                    else
                        UpdateControlPointModel(connectionLine.startControlPoint);

                    //add new segments
                    while (connectionLine.segments.Count < segmentCount)
                    {
                        int randomBendSegmentPrefabIndex = 0;

                        //if segmentPrefabs contains more then 1, get one random based on their probability
                        if (pipesystem.segmentProbability.Count > 1)
                        {
                            int randomNumber = Random.Range(0, 100);
                            int index = 0;

                            while (randomNumber >= 0 && index < pipesystem.bendSegmentProbability.Count)
                            {
                                randomNumber -= pipesystem.bendSegmentProbability[index];

                                if (randomNumber < 0)
                                {
                                    randomBendSegmentPrefabIndex = index;
                                }
                                else
                                    index++;
                            }
                        }
                        Segment newSegment = Instantiate(prefabSegment, connectionLine.SegmentHolder.transform);

                        GameObject newSegmentPrefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.modifiedBendSegmentPrefab[randomBendSegmentPrefabIndex]);
                        newSegmentPrefab.transform.parent = newSegment.gameObject.transform;
                        newSegmentPrefab.transform.localPosition = Vector3.zero;

                        newSegment.pipesystem = pipesystem;
                        newSegment.length = pipesystem.segmentLength;
                        newSegment.diameter = pipesystem.segmentDiameter;
                        newSegment.segmentNumber = randomBendSegmentPrefabIndex;
                        newSegment.model = newSegmentPrefab;
                        newSegment.bendPointsPositions = new Vector3[pipesystem.segmentBendPoints];
                        newSegment.bendPointRotations = new Vector3[pipesystem.segmentBendPoints];

                        if (controlPoint == connectionLine.endControlPoint)
                        {
                            connectionLine.segments.Add(newSegment);
                            newSegment.indexInLine = connectionLine.segments.Count;
                        }
                        else
                        {
                            connectionLine.segments.Insert(0, newSegment);
                            foreach (Segment segment in connectionLine.segments)
                                segment.indexInLine++;
                        }
                    }

                    //remove segments
                    while (connectionLine.segments.Count > segmentCount && connectionLine.segments.Count > 0)
                    {
                        if (controlPoint == connectionLine.endControlPoint)
                        {
                            DestroyImmediate(connectionLine.segments[connectionLine.segments.Count - 1].gameObject);
                            connectionLine.segments.RemoveAt(connectionLine.segments.Count - 1);
                        }
                        else
                        {
                            DestroyImmediate(connectionLine.segments[0].gameObject);
                            connectionLine.segments.RemoveAt(0);
                            foreach (Segment segment in connectionLine.segments)
                                segment.indexInLine--;
                        }
                    }

                    //rename segments
                    foreach (Segment segment in connectionLine.segments)
                        segment.name = "Segment " + segment.indexInLine;

                    //adjust position and rotation
                    controlPointOffset = pipesystem.distanceSegmentsControlPoint / distance;

                    Vector3 position;

                    for (int i = 0; i < segmentCount; i++)
                    {
                        connectionLine.segments[i].transform.position = GetPointOnCurve(points, controlPointOffset + i * (pipesystem.segmentLength / distance) + (pipesystem.segmentBendPoints/2) * ((pipesystem.segmentLength / pipesystem.segmentBendPoints) / distance));

                        for (int j = 0; j < pipesystem.segmentBendPoints; j++)
                        {
                            position = GetPointOnCurve(points, controlPointOffset + i * (pipesystem.segmentLength / distance) + (j * pipesystem.segmentBendPoints/ (pipesystem.segmentBendPoints-1)) * ((pipesystem.segmentLength / pipesystem.segmentBendPoints) / distance));
                            connectionLine.segments[i].bendPointsPositions[j] = position;
                            connectionLine.segments[i].model.transform.GetChild(j).transform.position = connectionLine.segments[i].bendPointsPositions[j];


                            //rotations
                            float t = controlPointOffset + i * (pipesystem.segmentLength / distance) + (j * pipesystem.segmentBendPoints / (pipesystem.segmentBendPoints - 1)) * ((pipesystem.segmentLength / pipesystem.segmentBendPoints) / distance);
                            float zmt = 1 - t;
                            float zmt2 = zmt * zmt;
                            float t2 = t * t;

                            connectionLine.segments[i].bendPointRotations[j] = startPoint * (-zmt2) + influencePoint1 * (3 * zmt2 - 2 * zmt) + influencePoint2 * (-3 * t2 + 2 * t) + endPoint * (t2);
                            connectionLine.segments[i].bendPointRotations[j] = connectionLine.segments[i].bendPointRotations[j].normalized;

                            Quaternion rotation = Quaternion.LookRotation(connectionLine.segments[i].bendPointRotations[j]);

                            connectionLine.segments[i].model.transform.GetChild(j).transform.rotation = rotation;
                        }
                    }
                }

                //adjust interjacent
                if (pipesystem.interjacentPrefab.Count > 0)
                {
                    //get interjacentcount
                    int interjacentCount;

                    if (distance > pipesystem.distanceSegmentsControlPoint * 2)
                    {
                        interjacentCount = segmentCount - 1;

                        if (shortendSegment)
                            interjacentCount++;

                        if (pipesystem.interjacentAtConnectionPoint)
                        {
                            if (connectionLine.startControlPoint.connectedControlPoints.Count > 1)
                                interjacentCount++;

                            if (connectionLine.endControlPoint.connectedControlPoints.Count > 1)
                                interjacentCount++;
                        }

                        if (pipesystem.interjacentAtEndPoint)
                        {
                            if (connectionLine.startControlPoint.connectedControlPoints.Count == 1 && connectionLine.startControlPoint.snapPoint == null)
                                interjacentCount++;

                            if (connectionLine.endControlPoint.connectedControlPoints.Count == 1 && connectionLine.endControlPoint.snapPoint == null)
                                interjacentCount++;
                        }
                    }
                    else if (distance == pipesystem.distanceSegmentsControlPoint * 2)
                        interjacentCount = 1;
                    else
                        interjacentCount = 0;


                    //if interjacentPrefabs contains more then 1, get one random based on their probability
                    while (connectionLine.interjacents.Count <= interjacentCount)
                    {
                        int randomInterjacentPrefabIndex = 0;

                        if (pipesystem.interjacentProbability.Count > 1)
                        {
                            int randomNumber = Random.Range(0, 100);
                            int index = 0;

                            while (randomNumber >= 0 && index < pipesystem.interjacentProbability.Count)
                            {
                                randomNumber -= pipesystem.interjacentProbability[index];

                                if (randomNumber < 0)
                                {
                                    randomInterjacentPrefabIndex = index;
                                }
                                else
                                    index++;
                            }
                        }
                        GameObject newInterjacentPrefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.interjacentPrefab[randomInterjacentPrefabIndex]);
                        newInterjacentPrefab.transform.parent = connectionLine.InterjacentHolder.transform;

                        if (controlPoint == connectionLine.endControlPoint)
                            connectionLine.interjacents.Add(newInterjacentPrefab);
                        else
                            connectionLine.interjacents.Insert(0, newInterjacentPrefab);
                    }

                    //Remove Interjacents
                    while (connectionLine.interjacents.Count > interjacentCount && connectionLine.interjacents.Count > 0)
                    {
                        if (controlPoint == connectionLine.endControlPoint)
                        {
                            DestroyImmediate(connectionLine.interjacents[connectionLine.interjacents.Count - 1].gameObject);
                            connectionLine.interjacents.RemoveAt(connectionLine.interjacents.Count - 1);
                        }
                        else
                        {
                            DestroyImmediate(connectionLine.interjacents[0].gameObject);
                            connectionLine.interjacents.RemoveAt(0);
                        }
                    }

                    //adjust interjacent position and rotation
                    for (int i = 0; i < interjacentCount; i++)
                    {
                        int number = i;
                        Vector3 newInterjacentPosition = Vector3.zero;

                        //endscheck
                        if (segmentCount > 0)
                        {
                            if (!connectionLine.isCurved)
                                newInterjacentPosition = (((segmentCount - number) * startPosition) + (number * segmentEndPosition)) / segmentCount;
                            else
                            {
                                float percentageValueOnCurve = controlPointOffset + i * (pipesystem.segmentLength / distance);

                                if (percentageValueOnCurve > 1)
                                    percentageValueOnCurve = 0.9f;
                                newInterjacentPosition = GetPointOnCurve(points, percentageValueOnCurve);
                            }
                        }
                        else
                            newInterjacentPosition = connectionLine.startPosition.transform.position;


                        if (i == interjacentCount - 1)
                            newInterjacentPosition = connectionLine.endPosition.transform.position;

                        connectionLine.interjacents[i].transform.position = newInterjacentPosition;

                        if(!connectionLine.isCurved)
                            connectionLine.interjacents[i].transform.rotation = Quaternion.LookRotation(newPointPosition - newInterjacentPosition);
                        else
                        {
                            Vector3 startPoint = connectionLine.startControlPoint.transform.position;
                            Vector3 endPoint = connectionLine.endControlPoint.transform.position;

                            Vector3 influencePoint1 = connectionLine.startInfluencePoint.transform.position;
                            Vector3 influencePoint2 = connectionLine.endInfluencePoint.transform.position;

                            float t = controlPointOffset + i * (pipesystem.segmentLength / distance);
                            float zmt = 1 - t;
                            float zmt2 = zmt * zmt;
                            float t2 = t * t;

                            Vector3 rotation = startPoint * (-zmt2) + influencePoint1 * (3 * zmt2 - 2 * zmt) + influencePoint2 * (-3 * t2 + 2 * t) + endPoint * (t2);
                            rotation = rotation.normalized;
                            connectionLine.interjacents[i].transform.rotation = Quaternion.LookRotation(rotation);
                        }

                    }
                }
            }
        }
        controlPointsToUpdate.Clear();
    }

    void UpdateControlPointModel(ControlPoint controlPoint)
    {
        GameObject controlPointModel;

        //delete old if number has changed
        if (controlPoint.connectionCount != controlPoint.connectionLines.Count)
        {
            while (controlPoint.models.Count > 0)
            {
                DestroyImmediate(controlPoint.models[controlPoint.models.Count-1].gameObject);
                controlPoint.models.RemoveAt(controlPoint.models.Count - 1);
            }
            controlPoint.models.Clear();
            controlPoint.connectionCount = controlPoint.connectionLines.Count;
        }

        if (controlPoint.connectionLines.Count == 1)
        {
            if (controlPoint.connectionLines[0].distance > pipesystem.distanceSegmentsControlPoint * 2)
            {
                Vector3 newPosition;
                if (controlPoint.connectionLines[0].startControlPoint == controlPoint)
                    newPosition = controlPoint.connectionLines[0].startPosition.transform.position;
                else
                    newPosition = controlPoint.connectionLines[0].endPosition.transform.position;

                //instantiate new 
                if (controlPoint.models.Count == 0)
                {
                    int randomEndPointPrefabIndex = 0;

                    if (pipesystem.endPointProbability.Count > 1)
                    {
                        int randomNumber = Random.Range(0, 100);
                        int index = 0;

                        while (randomNumber >= 0 && index < pipesystem.endPointProbability.Count)
                        {
                            randomNumber -= pipesystem.endPointProbability[index];

                            if (randomNumber <= 0)
                            {
                                randomEndPointPrefabIndex = index;
                            }
                            else
                                index++;
                        }
                    }
                    controlPointModel = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.endPointPrefab[randomEndPointPrefabIndex]);
                    controlPointModel.transform.parent = controlPoint.modelHolder.transform;
                    controlPoint.connectionCount = 1;
                    controlPoint.models.Add(controlPointModel);
                }

                //adjust transform
                controlPoint.models[0].transform.position = newPosition;
                controlPoint.models[0].transform.rotation = Quaternion.LookRotation(controlPoint.transform.position - newPosition);
            }
        }
        else if (controlPoint.connectionLines.Count > 1)
        {
            //bendModel

            //instantiate new bendModel
            if (controlPoint.models.Count == 0)
            {
                int randomControlPointPrefabIndex = 0;

                if (pipesystem.controlPointProbability.Count > 1)
                {
                    int randomNumber = Random.Range(0, 100);
                    int index = 0;

                    while (randomNumber >= 0 && index < pipesystem.controlPointProbability.Count)
                    {
                        randomNumber -= pipesystem.controlPointProbability[index];

                        if (randomNumber <= 0)
                        {
                            randomControlPointPrefabIndex = index;
                        }
                        else
                            index++;
                    }
                }
                controlPointModel = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.modifiedControlPointPrefab[randomControlPointPrefabIndex]);
                controlPointModel.transform.parent = controlPoint.modelHolder.transform;
                controlPointModel.transform.position = controlPoint.transform.position;
                controlPoint.models.Add(controlPointModel);

                if(controlPoint.startCurveInfluencePoint==null)
                {
                    controlPoint.startCurveInfluencePoint = Instantiate(emptyGameObject, controlPoint.modelHolder.transform);
                    controlPoint.startCurveInfluencePoint.gameObject.name = "InfluencePoint 1";

                    controlPoint.endCurveInfluencePoint = Instantiate(emptyGameObject, controlPoint.modelHolder.transform);
                    controlPoint.endCurveInfluencePoint.gameObject.name = "InfluencePoint 2";
                }

                controlPoint.positionsBendModel= new Vector3[pipesystem.controlPointBendPoints];
                controlPoint.rotationsBendModel= new Vector3[pipesystem.controlPointBendPoints];
            }

            // adjust bend position and rotation
            if(controlPoint.indexStartBendModel == controlPoint.indexEndBendModel)
            {
                controlPoint.indexStartBendModel = 0;
                controlPoint.indexEndBendModel = 1;
            }

            // setup variables
            Vector3 startPosition = controlPoint.modelConnectionPoints[controlPoint.indexStartBendModel].transform.position;
            Vector3 endPosition   = controlPoint.modelConnectionPoints[controlPoint.indexEndBendModel].transform.position;

            controlPoint.startCurveInfluencePoint.transform.position = (1 - pipesystem.controlPointCurveStrength) * startPosition + pipesystem.controlPointCurveStrength * controlPoint.transform.position;
            Vector3 influencePoint1 = controlPoint.startCurveInfluencePoint.transform.position;

            controlPoint.endCurveInfluencePoint.transform.position = (1 - pipesystem.controlPointCurveStrength) * endPosition + pipesystem.controlPointCurveStrength * controlPoint.transform.position;
            Vector3 influencePoint2 = controlPoint.endCurveInfluencePoint.transform.position;

            controlPoint.models[0].transform.position = controlPoint.transform.position;

            // calculate
            for (int i = 0; i < pipesystem.controlPointBendPoints; i++)
            {
                float t = (float)i/ (pipesystem.controlPointBendPoints-1);

                float zmt = 1-t;
                float zmt2 = zmt * zmt;
                float t2 = t * t;
                

                controlPoint.positionsBendModel[i] = startPosition * (zmt2 * zmt) + influencePoint1 * (3 * zmt2 * t) + influencePoint2 * (3 * zmt * t2) + endPosition * (t2 * t);

                controlPoint.rotationsBendModel[i] = startPosition * (-zmt2) + influencePoint1 * (3 * zmt2 - 2 * zmt) + influencePoint2 * (-3 * t2 + 2 * t) + endPosition * (t2);
                controlPoint.rotationsBendModel[i] = controlPoint.rotationsBendModel[i].normalized;
            }

            // set
            for (int i = 0; i < controlPoint.positionsBendModel.Length; i++)
            {
                controlPoint.models[0].transform.GetChild(i).transform.position = controlPoint.positionsBendModel[i];

                Quaternion rotation = Quaternion.LookRotation(controlPoint.rotationsBendModel[i]);
                
                controlPoint.models[0].transform.GetChild(i).transform.rotation = rotation;
            }

            //if there are more then two connections
            if(controlPoint.connectionLines.Count > 2)
            {
                //get modelConter
                float t = 0.5f;
                float zmt = 1 - t;
                float zmt2 = zmt * zmt;
                float t2 = t * t;
                controlPoint.modelCenter = startPosition * (zmt2 * zmt) + influencePoint1 * (3 * zmt2 * t) + influencePoint2 * (3 * zmt * t2) + endPosition * (t2 * t);

                //gizmos
                //int conn = controlPoint.modelConnectionPoints.Count;

                //float gizmoCount = (conn * conn - conn) / 2 - 1;

                //int a = controlPoint.indexStartBendModel;
                //int b = controlPoint.indexEndBendModel;

                //while (controlPoint.curveSwitschGizmos.Count < gizmoCount)
                //{

                //}
                
                //additional models

            }
        }
    }

    public void UpdateShortendSegment()
    {

    }

    public void ChangeSegment(SceneView sceneView)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        if (currentHoverd != null)
        {
            Event e = Event.current;
            int currentSegment = currentHoverd.segmentNumber;
            //if (e.type == EventType.ScrollWheel)
            //    Debug.Log("wheel");


            if (e.type == EventType.MouseDown && e.button == 0)
            {
                currentSegment++;
                if (currentSegment >= pipesystem.segmentPrefab.Count)
                    currentSegment = 0;

                DestroyImmediate(currentHoverd.gameObject.transform.GetChild(0).gameObject);

                GameObject newSegmentPrefab = (GameObject)PrefabUtility.InstantiatePrefab(pipesystem.segmentPrefab[currentSegment]);
                newSegmentPrefab.transform.parent = currentHoverd.gameObject.transform;
                newSegmentPrefab.transform.position = currentHoverd.gameObject.transform.position;
                newSegmentPrefab.transform.rotation = currentHoverd.gameObject.transform.rotation;
                currentHoverd.model = newSegmentPrefab;

                currentHoverd.segmentNumber = currentSegment;
            }
            HandleUtility.AddDefaultControl(controlId);
        }
    }

    public void RecalculateAll()
    {
        foreach(ControlPoint controlPoint in pipesystem.controlPoints)
        {
            //connectionLine
            foreach (ConnectionLine connectionLine in controlPoint.connectionLines)
            {
                for (int i = 0; i < connectionLine.segments.Count; i++)
                {
                    if(connectionLine.segments[i].transform.childCount>0)
                        DestroyImmediate(connectionLine.segments[i].transform.GetChild(0).gameObject);
                    
                }

                for (int i = 0; i < connectionLine.interjacents.Count; i++)
                {
                    DestroyImmediate(connectionLine.interjacents[i].gameObject);
                }
                connectionLine.segments.Clear();
                connectionLine.interjacents.Clear();
            }
            //controlPoint
            while (controlPoint.models.Count > 0)
            {
                DestroyImmediate(controlPoint.models[0].gameObject);
                controlPoint.models.RemoveAt(0);
            }

            controlPointsToUpdate.Add(controlPoint);
        }
        pipesystem.recalculateAll = false;
    }

    public void Debugger()
    {
        
    }
}
