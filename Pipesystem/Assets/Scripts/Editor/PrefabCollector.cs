using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabCollector : Editor {

    private List<GameObject> collectionPrefabs;
    private List<int> collectionPrefabProbability;

    private List<GameObject> pipesystemPrefabs;
    private List<int> pipesystemPrefabProbability;
    private List<int> oldPrefabProbability;

    public int sparePrefabProbability;
    public int deleteAt = -1;

    public GameObject emptyPlaceholder;

    public void Setup(List<GameObject> pipesystemPrefabs, List<int> pipesystemPrefabProbability, List<GameObject> collectionPrefabs, List<int> collectionPrefabProbability)
    {
        this.pipesystemPrefabs = pipesystemPrefabs;
        this.pipesystemPrefabProbability = pipesystemPrefabProbability;

        this.collectionPrefabs = collectionPrefabs;
        this.collectionPrefabProbability = collectionPrefabProbability;
    }

    public void PrefabList()
    {
        if (pipesystemPrefabs.Count != 0)
        {
            CalculatePrefabProbability();
            for (int i = 0; i < pipesystemPrefabs.Count; i++)
            {
                GUILayout.BeginHorizontal();

                pipesystemPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(pipesystemPrefabs[i], typeof(GameObject), false);
                pipesystemPrefabProbability[i] = EditorGUILayout.IntSlider(pipesystemPrefabProbability[i], 0, 100);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    RemovePrefab(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    public void DragNewArea()
    {
        Event e = Event.current;
        Rect newArea = GUILayoutUtility.GetRect(10, 50);
        GUI.Box(newArea, "Add new");

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
                        emptyPlaceholder = draggedObject;
                        if (!emptyPlaceholder)
                            continue;

                        pipesystemPrefabs.Add(emptyPlaceholder);
                        pipesystemPrefabProbability.Add(sparePrefabProbability);
                        oldPrefabProbability.Add(sparePrefabProbability);
                    }
                }
                Event.current.Use();
                break;
        }
    }

    public void RemovePrefab(int position)
    {
        pipesystemPrefabs.RemoveAt(position);
        pipesystemPrefabProbability.RemoveAt(position);
        oldPrefabProbability.RemoveAt(position);
        CalculatePrefabProbability();
        deleteAt = position;
    }

    public void CalculatePrefabProbability()
    {
        sparePrefabProbability = 100;

        //subtract all probabilities
        for (int i = 0; i < pipesystemPrefabProbability.Count; i++)
            sparePrefabProbability -= pipesystemPrefabProbability[i];

        //if someone changes and sparesegmentProbability would fall below 0, stop
        for (int i = 0; i < pipesystemPrefabProbability.Count; i++)
        {
            if (pipesystemPrefabProbability[i] != oldPrefabProbability[i])
            {
                if (sparePrefabProbability < 0)
                {
                    pipesystemPrefabProbability[i] -= sparePrefabProbability * (-1);
                    sparePrefabProbability = 0;
                }
                oldPrefabProbability[i] = pipesystemPrefabProbability[i];
            }
        }
    }

    public void SetupOldProbability()
    {
        oldPrefabProbability = new List<int>();
        for (int i = 0; i < pipesystemPrefabProbability.Count; i++)
        {
            int temp = pipesystemPrefabProbability[i];
            oldPrefabProbability.Add(temp);
        }
    }

    public void RevertChanges()
    {
        int tempInt = 0;
        GameObject tempGo;
        //fill segment count if to low
        while (pipesystemPrefabs.Count < collectionPrefabs.Count)
        {
            pipesystemPrefabs.Add(collectionPrefabs[0]);
            pipesystemPrefabProbability.Add(tempInt);
        }

        //reduce segement count if to high
        while (pipesystemPrefabs.Count > collectionPrefabs.Count)
        {
            pipesystemPrefabs.RemoveAt(pipesystemPrefabs.Count - 1);
            pipesystemPrefabProbability.RemoveAt(pipesystemPrefabProbability.Count - 1);
        }

        //replace segments
        for (int i = 0; i < pipesystemPrefabProbability.Count; i++)
        {
            tempGo = collectionPrefabs[i];
            pipesystemPrefabs[i] = tempGo;

            tempInt = collectionPrefabProbability[i];
            pipesystemPrefabProbability[i] = tempInt;
        }
    }

    public void ApplyChanges()
    {
        int tempInt = 0;
        GameObject tempGo;

        //fill prefab count if to low
        do
        {
            collectionPrefabs.Add(emptyPlaceholder);
            collectionPrefabProbability.Add(tempInt);

        } while (collectionPrefabs.Count < pipesystemPrefabs.Count);

        //reduce prefab count if to high
        while (collectionPrefabs.Count > pipesystemPrefabs.Count)
        {
            collectionPrefabs.RemoveAt(collectionPrefabs.Count - 1);
            collectionPrefabProbability.RemoveAt(collectionPrefabProbability.Count - 1);
        }

        //replace prefabs
        for (int i = 0; i < pipesystemPrefabs.Count; i++)
        {
            tempGo = pipesystemPrefabs[i];
            collectionPrefabs[i] = tempGo;

            tempInt = pipesystemPrefabProbability[i];
            collectionPrefabProbability[i] = tempInt;
        }
    }
}
