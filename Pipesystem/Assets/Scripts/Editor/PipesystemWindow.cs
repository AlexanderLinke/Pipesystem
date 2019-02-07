using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipesystemWindow : EditorWindow {

    //pipesystem
    private bool pipeSystemLinked;
    private GameObject pipeSystemObject;
    private Pipesystem pipesystem;

    //prefabs
    public GameObject prefabPipeSystem;
    public ControlPoint prefabControlPoint;
    public ConnectionLine prefabConnectionLine;
    public Segment prefabSegment;

    //selection
    private List<ControlPoint> selectedControlPoints = new List<ControlPoint>();
    private bool mousePressed;
    private bool shiftPressed;
    private bool segmentSelection;
    private Segment currentHoverd;
    private bool otherSelection;
    private SnapPoint selectedSnapPoint;

    private bool forcePipeUpdate;

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
            PipesystemManager.activePipesystem = pipesystem;
            //new and insert
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
                CreateControlPoint();

            if (GUILayout.Button("Insert"))
                InsertControlPoint();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Merge"))
                MergeControlPoints();

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

            //Selection
            GUILayout.Label("Selection");

            GUILayout.BeginHorizontal();

            segmentSelection = GUILayout.Toggle(segmentSelection, "Segments");
            pipesystem.selectControlPointsOnly= GUILayout.Toggle(pipesystem.selectControlPointsOnly, "Control Points only");

            GUILayout.EndHorizontal();


            if (GUILayout.Button("Unlink Pipesystem"))
            {
                UnlinkPipeSystem();
                return;
            }

            ManagePipestyle();
        }
        else
        {
            if (GUILayout.Button("New Pipesystem"))
                CreatePipesystem();

            if (GUILayout.Button("Link PipeSystem"))
                LinkPipeSystem();
        }

    }

    void Update()
    {
        UpdatePipes();
    }

    public void OnEnable()
    {
        SceneView.onSceneGUIDelegate += ShortcutUpdate;
        SceneView.onSceneGUIDelegate += CheckSelection;
        SceneView.onSceneGUIDelegate += ChangeSegment;

        PipesystemManager.activePipesystem = pipesystem;
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= ShortcutUpdate;
        SceneView.onSceneGUIDelegate -= CheckSelection;
        SceneView.onSceneGUIDelegate -= ChangeSegment;
        PipesystemManager.activePipesystem = null;

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

    public void CheckSelection(SceneView sceneView)
    {
        if (pipesystem == null)
            return;

        //SnapPoint
        foreach(Object obj in Selection.objects)
        {
            GameObject go = (GameObject)obj;
            if (go != null)
            {
                SnapPoint snapPoint = go.GetComponent<SnapPoint>();
                if (snapPoint != null)
                    selectedSnapPoint = snapPoint;
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

            foreach(ControlPoint controlPoint in pipesystem.controlPoints)
                controlPoint.isSelectedControlPoint = false;

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

                if (controlPoint != null)
                {
                    selectedControlPoints.Add(controlPoint);
                    controlPoint.isSelectedControlPoint = true;
                }
            }

            Object[] newSelection = new Object[selectedControlPoints.Count];
            for (int i = 0; i < selectedControlPoints.Count; i++)
            {
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
        GameObject newPipeSystem;
        newPipeSystem = Instantiate(prefabPipeSystem);
        newPipeSystem.name = "Pipesystem";
        LinkPipeSystem();
        CreateControlPoint();
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
        PipesystemManager.activePipesystem = pipesystem;
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
                forcePipeUpdate = true;
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
        PipesystemManager.activePipesystem = null;

        foreach (ControlPoint controlPoint in selectedControlPoints)
            controlPoint.isSelectedControlPoint = false;

        selectedControlPoints.Clear();
    }

    public void CreateControlPoint()
    {
        Vector3 spawnPosition = Vector3.zero;
        ControlPoint newControlPoint = null;

        //if there is not already a controlPoint spawn one
        if (pipesystem.controlPoints.Count == 0)
        {
            spawnPosition = pipeSystemObject.transform.position;
            newControlPoint = CreateControlPointBasic(spawnPosition);
            newControlPoint.indexInPipesystem = 1;
            RenameControlPoints();
            Selection.activeObject = newControlPoint;
        }
        else if (selectedControlPoints.Count == 0)
        {

            Debug.Log("No controlPoint selected");
            return;
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

        return newControlPoint;
    }

    public void ConnectControlPoints()
    {
        //if two controlPoints are selecetd and not already connected
        if(selectedControlPoints.Count == 2 && !selectedControlPoints[0].connectedControlPoints.Contains(selectedControlPoints[1]))
            CreateConnectionLine(selectedControlPoints[0], selectedControlPoints[1]);

        if(selectedControlPoints.Count==1 && selectedSnapPoint!=null)
        {
            ControlPoint newControlPoint = CreateControlPointBasic(selectedSnapPoint.transform.position);
            CreateConnectionLine(selectedControlPoints[0], newControlPoint);
            selectedSnapPoint.isConnected = true;
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
                    forcePipeUpdate = true;
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

    public void RenameControlPoints()
    {
        for (int i = 1; i <= pipesystem.controlPoints.Count; i++)
        {
            pipesystem.controlPoints[i - 1].name = "ControlPoint " + i;
            pipesystem.controlPoints[i - 1].indexInPipesystem = i;
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

        pipesystem.controlPoints.Clear();
        selectedControlPoints.Clear();
        Selection.activeObject = pipeSystemObject;
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
                            forcePipeUpdate = true;
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

        connectionLine.startControlPoint.connectionLines.Remove(connectionLine);
        connectionLine.endControlPoint.connectionLines.Remove(connectionLine);

        //destroy
        DestroyImmediate(connectionLine.gameObject);
    }

    public void ManagePipestyle()
    {
        if (GUILayout.Button("Manage Pipestyle"))
        {
            PipeStyleWindow pipeStyle = GetWindow<PipeStyleWindow>("Pipe Style");
            pipeStyle.usedStyle = pipesystem.pipeStyle;
            pipeStyle.pipesystemObject = pipeSystemObject;
        }
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
        newConnectionLine.name = "ConnectionLine " + controlPoint_1.indexInPipesystem + " - " + controlPoint_2.indexInPipesystem;

        //setup controllPoint referenzes
        controlPoint_1.connectedControlPoints.Add(controlPoint_2);
        controlPoint_2.connectedControlPoints.Add(controlPoint_1);
        controlPoint_1.connectionLines.Add(newConnectionLine);
        controlPoint_2.connectionLines.Add(newConnectionLine);

        forcePipeUpdate = true;
    }

    public void UpdatePipes()
    {
        //if one of the selected controlPoints has been moved or forcePipeUpdate has been called, update all their connectionLines
        foreach (ControlPoint controlPoint in selectedControlPoints)
        {
            if (controlPoint.oldPosition != controlPoint.transform.position || forcePipeUpdate)
            {
                foreach (ConnectionLine connectionLine in controlPoint.connectionLines)
                {
                    //setup variables
                    float distance = Vector3.Distance(connectionLine.startControlPoint.transform.position, connectionLine.endControlPoint.transform.position);

                    controlPoint.oldPosition = controlPoint.transform.position;
                    connectionLine.distance = distance;

                    Vector3 newPointPosition = controlPoint.transform.position;
                    Vector3 startPosition = connectionLine.startControlPoint.transform.position;
                    Vector3 endPosition = connectionLine.endControlPoint.transform.position;

                    int segmentCount = (int)(distance / pipesystem.segmentLength);

                    //add new segments
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

                        newSegment.pipesystem = pipesystem;
                        newSegment.length = pipesystem.segmentLength;
                        newSegment.diameter = pipesystem.segmentDiameter;
                        newSegment.segmentNumber = randomSegmentPrefabIndex;

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
                    for (int i = 1; i <= segmentCount; i++)
                    {
                        Vector3 newSegmentPosition = ((((((segmentCount - i) * startPosition) + (i * endPosition)) / segmentCount) * ((2 * i) - 1)) + startPosition) / (2 * i);
                        connectionLine.segments[i - 1].transform.position = newSegmentPosition;

                        connectionLine.segments[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newSegmentPosition);
                    }


                    //adjust interjacent
                    if (pipesystem.interjacentPrefab.Count > 0)
                    {
                        while (connectionLine.interjacents.Count < segmentCount - 1)
                        {
                            int randomInterjacentPrefabIndex = 0;

                            //if interjacentPrefabs contains more then 1, get one random based on their probability
                            if (pipesystem.interjacentProbability.Count > 1)
                            {
                                int randomNumber = Random.Range(0, 100);
                                int index = 0;

                                while (randomNumber >= 0 && index<pipesystem.interjacentProbability.Count)
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
                        while (connectionLine.interjacents.Count >= segmentCount && connectionLine.interjacents.Count > 0)
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
                        for (int i = 1; i < segmentCount; i++)
                        {
                            Vector3 newInterjacentPosition = (((segmentCount - i) * startPosition) + (i * endPosition)) / segmentCount;
                            connectionLine.interjacents[i - 1].transform.position = newInterjacentPosition;

                            connectionLine.interjacents[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newInterjacentPosition);
                        }
                    }
                }
            }
            forcePipeUpdate = false;
        }
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

                currentHoverd.segmentNumber = currentSegment;
            }
            HandleUtility.AddDefaultControl(controlId);
        }
    }
}
