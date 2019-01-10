using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PipesystemWindow : EditorWindow {

    //EditorUtility.GetPrefabPrefab(Selection.activeObject)
    //(GameObject)EditorUtility.InstantiatePrefab

    private bool pipeSystemLinked;

    public GameObject prefabPipeSystem;

    private List<PipePoint> selectedPipePoint = new List<PipePoint>();

    private GameObject pipeSystemObject;

    private Pipesystem pipeSystemScript;

    private bool mousePressed;
    private bool shiftPressed;

    private bool forcePipeUpdate;


    [MenuItem("Window/PipeSystem")]
    public static void ShowWindow()
    {
        GetWindow<PipesystemWindow>("PipeSystem");
    }

    void OnGUI()
    {
        if (pipeSystemScript == null || pipeSystemObject == null)
            pipeSystemLinked = false;

        if (pipeSystemLinked)
        {
            if (GUILayout.Button("New"))
                CreatePipePoint();

            if (GUILayout.Button("Delete Selected"))
                DeletePipePoint();

            if (GUILayout.Button("Delete All"))
                DeleteAllPipePoints();

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
        if (pipeSystemLinked)
            CheckSelection();
        UpdatePipes();
    }

    public void OnEnable()
    {
        SceneView.onSceneGUIDelegate += ShortcutUpdate;
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= ShortcutUpdate;
    }

    void ShortcutUpdate(SceneView sceneview)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown)
            mousePressed = true;

        if (e.type == EventType.MouseUp)
            mousePressed = false;

        if (e.isKey && e.character == 'a')
        {
            if (!mousePressed)
            {
                if (pipeSystemLinked)
                    CreatePipePoint();
            }
        }
        if (e.isKey && e.character == 'd')
        {
            if (!mousePressed)
                DeletePipePoint();
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

        pipeSystemScript = selectedObject.GetComponent<Pipesystem>();
        pipeSystemObject = selectedObject;

        if (!(pipeSystemScript == null || pipeSystemObject == null))
        {
            pipeSystemLinked = true;
            pipeSystemScript.isLinked = true;
        }
        else
            Debug.Log("This Object has no Pipesystem attached");
    }

    public void UnlinkPipeSystem()
    {
        pipeSystemScript.isLinked = false;
        pipeSystemScript = null;
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

        Debug.Log(selectedPipePoint.Count);

        if (pipeSystemScript.pipePoints.Count == 0)
        {
            spawnPosition = pipeSystemObject.transform.position;

            newPipePoint = Instantiate(pipeSystemScript.prefabPipePoint, spawnPosition, Quaternion.identity, pipeSystemScript.pipePointHolder.transform) as PipePoint;
            pipeSystemScript.pipePoints.Add(newPipePoint);
            newPipePoint.correspondingPipesystem = pipeSystemScript;
            newPipePoint.oldPosition = spawnPosition;

            Selection.activeObject = newPipePoint;
        }
        else if (selectedPipePoint.Count == 0)
        {
            Debug.Log("No PipePoint selected");
            return;
        }
        else
        {
            GameObject[] newSelected = new GameObject[selectedPipePoint.Count];

            int j = 0;

            foreach (PipePoint pipePoint in selectedPipePoint)
            {
                spawnPosition = pipePoint.transform.position;

                newPipePoint = Instantiate(pipeSystemScript.prefabPipePoint, spawnPosition, Quaternion.identity, pipeSystemScript.pipePointHolder.transform) as PipePoint;
                pipeSystemScript.pipePoints.Add(newPipePoint);
                newPipePoint.correspondingPipesystem = pipeSystemScript;
                newPipePoint.oldPosition = spawnPosition;

                newPipePoint.connectedPipePoints.Add(pipePoint);
                pipePoint.connectedPipePoints.Add(newPipePoint);

                newSelected[j] = newPipePoint.gameObject;
                j++;
            }

            selectedPipePoint.Clear();

            foreach (GameObject pipePoint in newSelected)
                selectedPipePoint.Add(pipePoint.GetComponent<PipePoint>());

            Selection.objects = newSelected;
        }

        //Postactions
        RenamePipePoints();
        CreatePipeLine();
    }

    public void RenamePipePoints()
    {
        if (pipeSystemScript.pipePoints.Count > 0)
        {
            for (int i = 1; i <= pipeSystemScript.pipePoints.Count; i++)
            {
                pipeSystemScript.pipePoints[i - 1].name = "PipePoint " + i;
            }
        }
    }

    public void DeleteAllPipePoints()
    {
        RemoveMissingObjectsFromList();

        foreach (PipePoint pipePoint in pipeSystemScript.pipePoints)
        {
            selectedPipePoint.Add(pipePoint);
        }
        DeletePipePoint();
        Selection.activeObject = pipeSystemObject;
    }

    public void RemoveMissingObjectsFromList()
    {
        for (int i = pipeSystemScript.pipePoints.Count - 1; i >= 0; i--)
        {
            if (pipeSystemScript.pipePoints[i] == null)
                pipeSystemScript.pipePoints.RemoveAt(i);
        }
        RenamePipePoints();
    }

    public void CheckSelection()
    {
        GameObject selectedObject = Selection.activeGameObject;
        PipePoint pipePoint = null;

        if (selectedObject != null)
            pipePoint = selectedObject.GetComponent<PipePoint>();
        else
            selectedPipePoint.Clear();

        if (pipePoint != null)
        {
            if (!selectedPipePoint.Contains(pipePoint))
            {
                selectedPipePoint.Add(pipePoint);
                pipePoint.isSelectedPipePoint = true;
            }
        }
        else
            selectedPipePoint.Clear();

        foreach (PipePoint pipePointi in pipeSystemScript.pipePoints)
        {
            if (!selectedPipePoint.Contains(pipePointi))
                pipePointi.isSelectedPipePoint = false;
        }
    }

    public void DeletePipePoint()
    {
        foreach (PipePoint pipePoint in selectedPipePoint)
        {
            if (pipePoint != null)
            {
                switch (pipePoint.connectedPipePoints.Count)
                {
                    case 0:
                        {
                            DestroyImmediate(pipePoint.gameObject);
                            RemoveMissingObjectsFromList();
                        }
                        break;
                    case 1:
                        {
                            Selection.activeGameObject = pipePoint.connectedPipePoints[0].gameObject;
                            pipePoint.connectedPipePoints[0].drawnPipePoints.Remove(pipePoint);
                            pipePoint.connectedPipePoints[0].connectedPipePoints.Remove(pipePoint);

                            DeletePipeLine(pipePoint.pipeLines[0]);
                            DestroyImmediate(pipePoint.gameObject);
                            RemoveMissingObjectsFromList();
                        }
                        break;
                    case 2:
                        {
                            Selection.activeGameObject = pipePoint.connectedPipePoints[0].gameObject;

                            //Remove references from self on others
                            pipePoint.connectedPipePoints[0].drawnPipePoints.Remove(pipePoint);
                            pipePoint.connectedPipePoints[1].drawnPipePoints.Remove(pipePoint);

                            pipePoint.connectedPipePoints[0].connectedPipePoints.Remove(pipePoint);
                            pipePoint.connectedPipePoints[1].connectedPipePoints.Remove(pipePoint);

                            //connect others
                            pipePoint.connectedPipePoints[0].connectedPipePoints.Add(pipePoint.connectedPipePoints[1]);
                            pipePoint.connectedPipePoints[1].connectedPipePoints.Add(pipePoint.connectedPipePoints[0]);

                            DeletePipeLine(pipePoint.pipeLines[1]);
                            DeletePipeLine(pipePoint.pipeLines[0]);


                            DestroyImmediate(pipePoint.gameObject);
                            RemoveMissingObjectsFromList();
                            CreatePipeLine();
                            forcePipeUpdate = true;
                        }
                        break;
                    default:
                        Debug.Log("To many connections");
                        break;
                }
            }
        }
        selectedPipePoint.Clear();
    }

    public void DeletePipeLine(PipeLine pipeLine)
    {
        foreach (GameObject segment in pipeLine.segments)
            DestroyImmediate(segment);

        foreach (GameObject interjacent in pipeLine.interjacents)
            DestroyImmediate(interjacent);

        DestroyImmediate(pipeLine.SegmentHolder);
        DestroyImmediate(pipeLine.InterjacentHolder);

        pipeLine.segments.Clear();
        pipeLine.interjacents.Clear();

        pipeLine.startPipePoint.pipeLines.Remove(pipeLine);
        pipeLine.endPipePoint.pipeLines.Remove(pipeLine);
        //pipeSystemScript.pipeLines.Remove(pipeLine);
        DestroyImmediate(pipeLine.gameObject);
    }

    public void ManagePipestyle()
    {
        if (GUILayout.Button("Manage Pipestyle"))
        {
            PipeStyleWindow pipeStyle = GetWindow<PipeStyleWindow>("Pipe Style");
            pipeStyle.usedStyle = pipeSystemScript.pipeStyle;
            pipeStyle.pipesystemObject = pipeSystemObject;
        }
    }

    public void CreatePipeLine()
    {
        //check foreach pipePoint if the connected Pipepoints already created the pipes
        foreach (PipePoint pipePoint in pipeSystemScript.pipePoints)
        {
            foreach (PipePoint connectedPipePoint in pipePoint.connectedPipePoints)
            {
                if (!connectedPipePoint.drawnPipePoints.Contains(pipePoint))
                {
                    PipeLine pipeLine;

                    pipeLine = Instantiate(pipeSystemScript.prefabPipeLine, pipeSystemScript.pipeLinelHolder.transform);

                    pipePoint.drawnPipePoints.Add(connectedPipePoint);
                    connectedPipePoint.drawnPipePoints.Add(pipePoint);
                    pipeLine.startPipePoint = pipePoint;
                    pipeLine.endPipePoint = connectedPipePoint;
                    pipeLine.correspondingPipesystem = pipeSystemScript;
                    pipePoint.pipeLines.Add(pipeLine);
                    connectedPipePoint.pipeLines.Add(pipeLine);
                }
            }
        }
    }

    public void UpdatePipes()
    {
        //if one of the selected pipePoints has been moved or forcePipeUpdate has been called, update all connected pipeLines
        foreach (PipePoint pipePoint in selectedPipePoint)
        {
            if (pipePoint.oldPosition != pipePoint.transform.position || forcePipeUpdate)
            {
                forcePipeUpdate = false;

                foreach (PipeLine pipeLine in pipePoint.pipeLines)
                {
                    //setup variables
                    float distance = Vector3.Distance(pipeLine.startPipePoint.transform.position, pipeLine.endPipePoint.transform.position);

                    pipePoint.oldPosition = pipePoint.transform.position;
                    pipeLine.distance = distance;

                    Vector3 newPointPosition = pipePoint.transform.position;
                    Vector3 startPosition = pipeLine.startPipePoint.transform.position;
                    Vector3 endPosition = pipeLine.endPipePoint.transform.position;

                    int segmentCount = (int)(distance / pipeSystemScript.segmentLength);

                    //add new segments
                    while (pipeLine.segments.Count < segmentCount)
                    {
                        int randomSegmentPrefabIndex = 0;

                        //if segmentPrefabs contains more then 1, get one random based on their probability
                        if (pipeSystemScript.segmentProbability.Count > 1)
                        {
                            int randomNumber = Random.Range(0, 100);
                            int index = 0;

                            while (randomNumber >= 0)
                            {
                                randomNumber -= pipeSystemScript.segmentProbability[index];

                                if (randomNumber < 0)
                                {
                                    randomSegmentPrefabIndex = index;
                                }
                                else
                                    index++;
                            }
                        }
                        if (pipePoint == pipeLine.endPipePoint)
                            pipeLine.segments.Add(Instantiate(pipeSystemScript.segmentPrefab[randomSegmentPrefabIndex], pipeLine.SegmentHolder.transform));
                        else
                            pipeLine.segments.Insert(0, Instantiate(pipeSystemScript.segmentPrefab[randomSegmentPrefabIndex], pipeLine.SegmentHolder.transform));
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
                        }
                    }

                    //adjust segments position and rotation
                    for (int i = 1; i <= segmentCount; i++)
                    {
                        Vector3 newSegmentPosition = ((((((segmentCount - i) * startPosition) + (i * endPosition)) / segmentCount) * ((2 * i) - 1)) + startPosition) / (2 * i);
                        pipeLine.segments[i - 1].transform.position = newSegmentPosition;

                        pipeLine.segments[i - 1].transform.rotation = Quaternion.LookRotation(newPointPosition - newSegmentPosition);
                    }


                    //adjust interjacent
                    if (pipeSystemScript.interjacentPrefab.Count > 0)
                    {
                        while (pipeLine.interjacents.Count < segmentCount - 1)
                        {
                            int randomInterjacentPrefabIndex = 0;

                            //if interjacentPrefabs contains more then 1, get one random based on their probability
                            if (pipeSystemScript.interjacentProbability.Count > 1)
                            {
                                int randomNumber = Random.Range(0, 100);
                                int index = 0;

                                while (randomNumber >= 0)
                                {
                                    randomNumber -= pipeSystemScript.interjacentProbability[index];

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
                                pipeLine.interjacents.Add(Instantiate(pipeSystemScript.interjacentPrefab[randomInterjacentPrefabIndex], pipeLine.InterjacentHolder.transform));
                            else
                                pipeLine.interjacents.Insert(0, Instantiate(pipeSystemScript.interjacentPrefab[randomInterjacentPrefabIndex], pipeLine.InterjacentHolder.transform));
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
        }
    }
}
