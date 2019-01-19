using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipesystemWindow : EditorWindow {

    //EditorUtility.GetPrefabPrefab(Selection.activeObject)
    //(GameObject)EditorUtility.InstantiatePrefab

    private bool pipeSystemLinked;

    public GameObject prefabPipeSystem;
    public PipePoint prefabPipePoint;
    public PipeLine prefabPipeLine;
    public PipeSegment prefabSegment;


    private List<PipePoint> selectedPipePoint = new List<PipePoint>();

    private GameObject pipeSystemObject;

    private Pipesystem pipesystem;

    //selection
    private bool mousePressed;
    private bool shiftPressed;

    private bool segmentSelection;
    private PipeSegment currentHoverd;
    private bool otherSelection;

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
            //new and insert
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
                CreatePipePoint();

            if (GUILayout.Button("Insert"))
                InsertPipePoint();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Merge"))
                MergePipePoints();

            //connect and deconnect
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Connect"))
                ConnectPipePoints();

            if (GUILayout.Button("Delete Connection"))
                DeleteConnectionBetweenPipePoints();

            GUILayout.EndHorizontal();

            //delete
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Delete Selected"))
                DeletePipePoint();

            if (GUILayout.Button("Delete All"))
                DeleteAllPipePoints();

            GUILayout.EndHorizontal();

            //Selection
            GUILayout.Label("Selection");

            GUILayout.BeginHorizontal();

            segmentSelection = GUILayout.Toggle(segmentSelection, "Segments");
            pipesystem.selectControlPipePointsOnly= GUILayout.Toggle(pipesystem.selectControlPipePointsOnly, "Control Points only");

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
                DeletePipePoint();
        }
    }

    public void CheckSelection(SceneView sceneView)
    {
        if (pipesystem == null)
            return;
        //Select all 
        if (!pipesystem.selectControlPipePointsOnly)
        {
            selectedPipePoint.Clear();
            foreach (PipePoint pipePoint in pipesystem.pipePoints)
            {
                foreach(Object go in Selection.objects)
                {
                    if (pipePoint.gameObject == go)
                    {
                        pipePoint.isSelectedPipePoint = true;
                        selectedPipePoint.Add(pipePoint);
                    } 
                }
                if (!selectedPipePoint.Contains(pipePoint))
                    pipePoint.isSelectedPipePoint = false;
            }
        }
        
        //select ControllPoints only
        if (pipesystem.selectControlPipePointsOnly)
        {
            Object[] objects;
            objects = Selection.objects;

            foreach(PipePoint pipePoint in pipesystem.pipePoints)
                pipePoint.isSelectedPipePoint = false;

            selectedPipePoint.Clear();

            foreach (Object totalSelected in objects)
            {
                GameObject gameObject = (GameObject)totalSelected;

                PipePoint pipePoint = gameObject.GetComponent<PipePoint>();

                Pipesystem system = gameObject.GetComponent<Pipesystem>();
                if (system!=null)
                {
                    Selection.activeGameObject = pipeSystemObject;
                    return;
                }

                if (pipePoint != null)
                {
                    selectedPipePoint.Add(pipePoint);
                    pipePoint.isSelectedPipePoint = true;
                }
            }

            Object[] newSelection = new Object[selectedPipePoint.Count];
            for (int i = 0; i < selectedPipePoint.Count; i++)
            {
                newSelection[i] = selectedPipePoint[i].gameObject;
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
                PipeSegment segment = hit.collider.GetComponentInParent<PipeSegment>();

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
        Selection.activeGameObject = newPipeSystem;
        LinkPipeSystem();
        CreatePipePoint();
    }

    public void LinkPipeSystem()
    {
        GameObject selectedObject;
        selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.Log("Please select a Pipesystem");
            return;
        }

        pipesystem = selectedObject.GetComponent<Pipesystem>();
        pipeSystemObject = selectedObject;

        if (!(pipesystem == null || pipeSystemObject == null))
        {
            pipeSystemLinked = true;
            pipesystem.isLinked = true;

            //bugfix where after restart the pipepoints list was empty although there were pipepoints 
            int pipePointCount = pipesystem.pipePointHolder.transform.childCount;

            if( pipePointCount != 0 && pipesystem.pipePoints == null || pipesystem.pipePoints.Count != pipePointCount)
            {
                pipesystem.pipePoints.Clear();
                for(int i = 0; i < pipePointCount; i++)
                {
                    pipesystem.pipePoints.Add(pipesystem.pipePointHolder.transform.GetChild(i).gameObject.GetComponent<PipePoint>());
                }

                RemoveMissingObjectsFromList();
                RenamePipePoints();
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

        foreach (PipePoint pipePoint in selectedPipePoint)
            pipePoint.isSelectedPipePoint = false;

        selectedPipePoint.Clear();
    }

    public void CreatePipePoint()
    {
        Vector3 spawnPosition = Vector3.zero;
        PipePoint newPipePoint;

        //if there is not already a pipepoint spawn one
        if (pipesystem.pipePoints.Count == 0)
        {
            spawnPosition = pipeSystemObject.transform.position;

            newPipePoint = Instantiate(prefabPipePoint, spawnPosition, Quaternion.identity, pipesystem.pipePointHolder.transform) as PipePoint;
            pipesystem.pipePoints.Add(newPipePoint);
            newPipePoint.correspondingPipesystem = pipesystem;
            newPipePoint.oldPosition = spawnPosition;

            newPipePoint.indexInPipesystem = 1;
            RenamePipePoints();
            Selection.activeObject = newPipePoint;
        }
        else if (selectedPipePoint.Count == 0)
        {

            Debug.Log("No PipePoint selected");
            return;
        }
        else
        {
            //create a new pipepoint foreach selected pipepoint 
            GameObject[] newSelected = new GameObject[selectedPipePoint.Count];

            int indexOfNewSelected = 0;

            foreach (PipePoint pipePoint in selectedPipePoint)
            {
                spawnPosition = pipePoint.transform.position;

                newPipePoint = Instantiate(prefabPipePoint, spawnPosition, Quaternion.identity, pipesystem.pipePointHolder.transform) as PipePoint;
                pipesystem.pipePoints.Add(newPipePoint);
                newPipePoint.correspondingPipesystem = pipesystem;
                newPipePoint.oldPosition = spawnPosition;
                CreatePipeLine(pipePoint, newPipePoint);

                newSelected[indexOfNewSelected] = newPipePoint.gameObject;
                indexOfNewSelected++;
            }
            //select the new pipepoints
            selectedPipePoint.Clear();

            foreach (GameObject go in newSelected)
                selectedPipePoint.Add(go.GetComponent<PipePoint>());

            Selection.objects = newSelected;
        }
    }

    public void ConnectPipePoints()
    {
        //if two pipepoints are selecetd and not already connected
        if(selectedPipePoint.Count == 2 && !selectedPipePoint[0].connectedPipePoints.Contains(selectedPipePoint[1]))
            CreatePipeLine(selectedPipePoint[0], selectedPipePoint[1]);
    }

    public void DeleteConnectionBetweenPipePoints()
    {
        //if two pipepoints are selecetd and connected delete the connection
        if (selectedPipePoint.Count == 2 && selectedPipePoint[0].connectedPipePoints.Contains(selectedPipePoint[1]))
        {
            foreach(PipeLine pipeLine in selectedPipePoint[0].pipeLines)
            {
                if(selectedPipePoint[1].pipeLines.Contains(pipeLine))
                {
                    DeletePipeLine(pipeLine);
                    forcePipeUpdate = true;
                    break;
                }
            }
        }
    }

    public void InsertPipePoint()
    {
        if(selectedPipePoint.Count==2 && selectedPipePoint[0].connectedPipePoints.Contains(selectedPipePoint[1]))
        {
            //create the new PipePoint
            PipePoint newPipePoint;

            Vector3 spawnPosition;

            spawnPosition = (selectedPipePoint[0].transform.position + selectedPipePoint[1].transform.position)/2;

            newPipePoint = Instantiate(prefabPipePoint, spawnPosition, Quaternion.identity, pipesystem.pipePointHolder.transform) as PipePoint;
            pipesystem.pipePoints.Add(newPipePoint);
            newPipePoint.correspondingPipesystem = pipesystem;
            newPipePoint.oldPosition = spawnPosition;

            //delete the old pipeline
            foreach(PipeLine pipeLine in selectedPipePoint[0].pipeLines)
            {
                if(selectedPipePoint[1].pipeLines.Contains(pipeLine))
                {
                    DeletePipeLine(pipeLine);
                    break;
                }
            }

            //Create new pipeLines
            CreatePipeLine( selectedPipePoint[0], newPipePoint);
            CreatePipeLine( newPipePoint, selectedPipePoint[1]);
        }
    }

    public void MergePipePoints()
    {
        //Deletes the two selected contollpoint and creates a new one between them with all their connections
        if(selectedPipePoint.Count == 2)
        {
            Vector3 newPosition = (selectedPipePoint[0].transform.position + selectedPipePoint[1].transform.position)/2;
            PipePoint newPipePoint;

            //Get all connections
            List<PipePoint> newConnecteControllPoints = new List<PipePoint>();
            foreach(PipePoint pipePoint in selectedPipePoint)
            {
                foreach(PipePoint connectedPipePoint in pipePoint.connectedPipePoints)
                {
                    if(!newConnecteControllPoints.Contains(connectedPipePoint) && !selectedPipePoint.Contains(connectedPipePoint))
                        newConnecteControllPoints.Add(connectedPipePoint);
                }
            }

            //Delete old connections and contollPoints
            foreach (PipePoint pipePoint in selectedPipePoint)
            {
                while(pipePoint.pipeLines.Count > 0)
                        DeletePipeLine(pipePoint.pipeLines[pipePoint.pipeLines.Count-1]);
            }
            DeletePipePoint();

            //create new pipePoint and connections
            newPipePoint = Instantiate(prefabPipePoint, newPosition, Quaternion.identity, pipesystem.pipePointHolder.transform) as PipePoint;
            pipesystem.pipePoints.Add(newPipePoint);
            newPipePoint.correspondingPipesystem = pipesystem;
            newPipePoint.oldPosition = newPosition;
            newPipePoint.connectedPipePoints = newConnecteControllPoints;

            foreach (PipePoint controllPoint in newConnecteControllPoints)
                CreatePipeLine(controllPoint, newPipePoint);

            Selection.activeGameObject = newPipePoint.gameObject;
        }
    }

    public void RenamePipePoints()
    {
        for (int i = 1; i <= pipesystem.pipePoints.Count; i++)
        {
            pipesystem.pipePoints[i - 1].name = "PipePoint " + i;
            pipesystem.pipePoints[i - 1].indexInPipesystem = i;
        }

        foreach (PipePoint pipePoint in pipesystem.pipePoints)
        {
            foreach (PipeLine pipeLine in pipePoint.pipeLines)
            {
                foreach (PipePoint connectedPipePoint in pipePoint.connectedPipePoints)
                {
                    if (connectedPipePoint.pipeLines.Contains(pipeLine))
                    {
                        pipeLine.name = "PipeLine " + connectedPipePoint.indexInPipesystem + " - " + pipePoint.indexInPipesystem;
                    }
                }
            }
        }
    }

    public void DeleteAllPipePoints()
    {
        //harddeletes everything
        while (pipesystem.pipePointHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.pipePointHolder.transform.GetChild(0).gameObject);

        while (pipesystem.pipeLineHolder.transform.childCount > 0)
            DestroyImmediate(pipesystem.pipeLineHolder.transform.GetChild(0).gameObject);

        pipesystem.pipePoints.Clear();
        selectedPipePoint.Clear();
        Selection.activeObject = pipeSystemObject;
    }

    public void RemoveMissingObjectsFromList()
    {
        for (int i = pipesystem.pipePoints.Count - 1; i >= 0; i--)
        {
            if (pipesystem.pipePoints[i] == null)
                pipesystem.pipePoints.RemoveAt(i);
        }
        RenamePipePoints();
    }

    public void DeletePipePoint()
    {
        List<PipePoint> controllPointsToDelete = new List<PipePoint>();

        GameObject[] newSelectedControllPoint = new GameObject[1];

        foreach (PipePoint pipePoint in selectedPipePoint)
        {
            if (pipePoint != null)
            {
                switch (pipePoint.connectedPipePoints.Count)
                {
                    case 0:
                        {
                            controllPointsToDelete.Add(pipePoint);
                            Selection.activeGameObject = pipeSystemObject;
                        }
                        break;
                    case 1:
                        {
                            newSelectedControllPoint[0] = pipePoint.connectedPipePoints[0].gameObject;
                            DeletePipeLine(pipePoint.pipeLines[0]);
                            controllPointsToDelete.Add(pipePoint);
                            
                        }
                        break;
                    case 2:
                        {
                            //if the selected controllPoint sits between two other controllPoints, delete it and connect the others
                            PipePoint newStartControllPoint = pipePoint.connectedPipePoints[0];
                            PipePoint newEndControllPoint = pipePoint.connectedPipePoints[1];

                            DeletePipeLine(pipePoint.pipeLines[1]);
                            DeletePipeLine(pipePoint.pipeLines[0]);

                            CreatePipeLine(newStartControllPoint, newEndControllPoint);
                            controllPointsToDelete.Add(pipePoint);

                            newSelectedControllPoint[0] = newStartControllPoint.gameObject;
                            forcePipeUpdate = true;
                        }
                        break;
                    default:
                        {
                            // if there are 3 or more connected controllPoints delete all connections
                            while(pipePoint.pipeLines.Count > 0)
                                DeletePipeLine(pipePoint.pipeLines[pipePoint.pipeLines.Count - 1]);

                            controllPointsToDelete.Add(pipePoint);
                        }
                        break;
                }
            }
        }

        while (controllPointsToDelete.Count > 0)
        {
            DestroyImmediate(controllPointsToDelete[controllPointsToDelete.Count - 1].gameObject);
            controllPointsToDelete.RemoveAt(controllPointsToDelete.Count - 1);
        }

        selectedPipePoint.Clear();
        if (newSelectedControllPoint[0]!=null)
        {
            foreach(GameObject controllPoint in newSelectedControllPoint)
                selectedPipePoint.Add(controllPoint.GetComponent<PipePoint>());

            Selection.objects = newSelectedControllPoint;
        }
        
        RemoveMissingObjectsFromList();
    }

    public void DeletePipeLine(PipeLine pipeLine)
    {
        //Clear objects from Lists
        foreach (PipeSegment segment in pipeLine.segments)
            DestroyImmediate(segment);

        foreach (GameObject interjacent in pipeLine.interjacents)
            DestroyImmediate(interjacent);

        DestroyImmediate(pipeLine.SegmentHolder);
        DestroyImmediate(pipeLine.InterjacentHolder);

        pipeLine.segments.Clear();
        pipeLine.interjacents.Clear();

        //Remove connections
        pipeLine.startPipePoint.connectedPipePoints.Remove(pipeLine.endPipePoint);
        pipeLine.endPipePoint.connectedPipePoints.Remove(pipeLine.startPipePoint);

        pipeLine.startPipePoint.pipeLines.Remove(pipeLine);
        pipeLine.endPipePoint.pipeLines.Remove(pipeLine);

        //destroy
        DestroyImmediate(pipeLine.gameObject);
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

    public void CreatePipeLine(PipePoint controllPoint_1, PipePoint controllPoint_2)
    {
        PipeLine newPipeLine;

        RenamePipePoints();

        //create and setup new connectionLine
        newPipeLine = Instantiate(prefabPipeLine, pipesystem.pipeLineHolder.transform);
        newPipeLine.correspondingPipesystem = pipesystem;
        newPipeLine.startPipePoint = controllPoint_1;
        newPipeLine.endPipePoint = controllPoint_2;
        newPipeLine.name = "PipeLine " + controllPoint_1.indexInPipesystem + " - " + controllPoint_2.indexInPipesystem;

        //setup controllPoint referenzes
        controllPoint_1.connectedPipePoints.Add(controllPoint_2);
        controllPoint_2.connectedPipePoints.Add(controllPoint_1);
        controllPoint_1.pipeLines.Add(newPipeLine);
        controllPoint_2.pipeLines.Add(newPipeLine);

        forcePipeUpdate = true;
    }

    public void UpdatePipes()
    {
        //if one of the selected pipePoints has been moved or forcePipeUpdate has been called, update all connected pipeLines
        foreach (PipePoint pipePoint in selectedPipePoint)
        {
            if (pipePoint.oldPosition != pipePoint.transform.position || forcePipeUpdate)
            {
                foreach (PipeLine pipeLine in pipePoint.pipeLines)
                {
                    //setup variables
                    float distance = Vector3.Distance(pipeLine.startPipePoint.transform.position, pipeLine.endPipePoint.transform.position);

                    pipePoint.oldPosition = pipePoint.transform.position;
                    pipeLine.distance = distance;

                    Vector3 newPointPosition = pipePoint.transform.position;
                    Vector3 startPosition = pipeLine.startPipePoint.transform.position;
                    Vector3 endPosition = pipeLine.endPipePoint.transform.position;

                    int segmentCount = (int)(distance / pipesystem.segmentLength);

                    //add new segments
                    while (pipeLine.segments.Count < segmentCount)
                    {
                        int randomSegmentPrefabIndex = 0;

                        //if segmentPrefabs contains more then 1, get one random based on their probability
                        if (pipesystem.segmentProbability.Count > 1)
                        {
                            int randomNumber = Random.Range(0, 100);
                            int index = 0;

                            while (randomNumber >= 0)
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
                        PipeSegment newSegment = Instantiate(prefabSegment, pipeLine.SegmentHolder.transform);
                        Instantiate(pipesystem.segmentPrefab[randomSegmentPrefabIndex], newSegment.transform);

                        newSegment.pipesystem = pipesystem;
                        newSegment.length = pipesystem.segmentLength;
                        newSegment.diameter = pipesystem.segmentDiameter;
                        newSegment.segmentNumber = randomSegmentPrefabIndex;

                        if (pipePoint == pipeLine.endPipePoint)
                        {
                            pipeLine.segments.Add(newSegment);
                            newSegment.indexInLine = pipeLine.segments.Count;
                        }
                        else
                        {
                            pipeLine.segments.Insert(0, newSegment);
                            foreach (PipeSegment segment in pipeLine.segments)
                                segment.indexInLine++;
                        }
                    }

                    //remove segments
                    while (pipeLine.segments.Count > segmentCount && pipeLine.segments.Count > 0)
                    {
                        if (pipePoint == pipeLine.endPipePoint)
                        {
                            DestroyImmediate(pipeLine.segments[pipeLine.segments.Count - 1].gameObject);
                            pipeLine.segments.RemoveAt(pipeLine.segments.Count - 1);
                        }
                        else
                        {
                            DestroyImmediate(pipeLine.segments[0].gameObject);
                            pipeLine.segments.RemoveAt(0);
                            foreach (PipeSegment segment in pipeLine.segments)
                                segment.indexInLine--;
                        }
                    }

                    //rename segments
                    foreach (PipeSegment segment in pipeLine.segments)
                        segment.name = "Segment " + segment.indexInLine;

                    //adjust segments position and rotation
                    for (int i = 1; i <= segmentCount; i++)
                    {
                        Vector3 newSegmentPosition = ((((((segmentCount - i) * startPosition) + (i * endPosition)) / segmentCount) * ((2 * i) - 1)) + startPosition) / (2 * i);
                        pipeLine.segments[i - 1].transform.position = newSegmentPosition;

                        pipeLine.segments[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newSegmentPosition);
                    }


                    //adjust interjacent
                    if (pipesystem.interjacentPrefab.Count > 0)
                    {
                        while (pipeLine.interjacents.Count < segmentCount - 1)
                        {
                            int randomInterjacentPrefabIndex = 0;

                            //if interjacentPrefabs contains more then 1, get one random based on their probability
                            if (pipesystem.interjacentProbability.Count > 1)
                            {
                                int randomNumber = Random.Range(0, 100);
                                int index = 0;

                                while (randomNumber >= 0)
                                {
                                    randomNumber -= pipesystem.interjacentProbability[index];

                                    if (randomNumber < 0)
                                    {
                                        randomInterjacentPrefabIndex = index;
                                        Debug.Log(randomInterjacentPrefabIndex);
                                    }
                                    else
                                        index++;
                                }
                            }
                            if (pipePoint == pipeLine.endPipePoint)
                                pipeLine.interjacents.Add(Instantiate(pipesystem.interjacentPrefab[randomInterjacentPrefabIndex], pipeLine.InterjacentHolder.transform));
                            else
                                pipeLine.interjacents.Insert(0, Instantiate(pipesystem.interjacentPrefab[randomInterjacentPrefabIndex], pipeLine.InterjacentHolder.transform));
                        }

                        //Remove Interjacents
                        while (pipeLine.interjacents.Count >= segmentCount && pipeLine.interjacents.Count > 0)
                        {
                            if (pipePoint == pipeLine.endPipePoint)
                            {
                                DestroyImmediate(pipeLine.interjacents[pipeLine.interjacents.Count - 1].gameObject);
                                pipeLine.interjacents.RemoveAt(pipeLine.interjacents.Count - 1);
                            }
                            else
                            {
                                DestroyImmediate(pipeLine.interjacents[0].gameObject);
                                pipeLine.interjacents.RemoveAt(0);
                            }
                        }

                        //adjust interjacent position and rotation
                        for (int i = 1; i < segmentCount; i++)
                        {
                            Vector3 newInterjacentPosition = (((segmentCount - i) * startPosition) + (i * endPosition)) / segmentCount;
                            pipeLine.interjacents[i - 1].transform.position = newInterjacentPosition;

                            pipeLine.interjacents[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newInterjacentPosition);
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
                Instantiate(pipesystem.segmentPrefab[currentSegment], currentHoverd.transform);
                currentHoverd.segmentNumber = currentSegment;
            }
            HandleUtility.AddDefaultControl(controlId);
        }
    }
}
