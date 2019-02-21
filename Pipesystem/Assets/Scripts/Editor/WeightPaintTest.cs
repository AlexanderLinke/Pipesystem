using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WeigthPaintTest : EditorWindow
{
    private SkinnedMeshRenderer meshRenderer;
    private Mesh mesh;
    private List<GameObject> points;
    public GameObject prefab;

    [MenuItem("Window/BendMesh")]
    public static void ShowWindow()
    {
        GetWindow<WeigthPaintTest>("Mesh Bender");
    }
    void OnGUI()
    {
        GetPoints();
        GetMesh();

        if(mesh != null)
            DeformMesh();
    }
    void GetPoints()
    {
        if (points != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
               points[i] = (GameObject)EditorGUILayout.ObjectField(points[i],typeof(GameObject),false);
            }
        }
        else
        {
            points = new List<GameObject>();
        }

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
                        points.Add(draggedObject);
                    }
                }
                Event.current.Use();
                break;
        }


    }

    void GetMesh()
    {

        Event e = Event.current;
        Rect newArea = GUILayoutUtility.GetRect(10, 50);
        GUI.Box(newArea, "Mesh");

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
                        prefab = draggedObject;
                    }
                }
                Event.current.Use();
                break;
        }

        if (GUILayout.Button("GetMesh"))
        {
            GameObject go;
            if (prefab!=null)
            {
                if(meshRenderer!=null)
                {
                    DestroyImmediate(meshRenderer.gameObject);
                    foreach(GameObject bob in points)
                    {
                        DestroyImmediate(bob.transform.GetChild(0).gameObject);
                    }
                }

                go = Instantiate(prefab);
                go.transform.position = Vector3.zero;

                meshRenderer = go.AddComponent<SkinnedMeshRenderer>();
                meshRenderer.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
                mesh = meshRenderer.sharedMesh;
                mesh.boneWeights = null;
                mesh.bindposes = null;
            }
        }
    }

    void DeformMesh()
    {
        if (GUILayout.Button("DeformMesh"))
        {
            BoneWeight[] weights = new BoneWeight[mesh.vertexCount];
            Transform[] bones = new Transform[points.Count];
            Matrix4x4[] bindPoses = new Matrix4x4[points.Count];
            //bones

            for (int i = 0; i < points.Count; i++)
            {
                bones[i] = new GameObject().transform;
                
                bones[i].localRotation = Quaternion.identity;
                bones[i].localPosition = points[i].transform.position;
                bindPoses[i] = bones[i].worldToLocalMatrix * points[2].transform.localToWorldMatrix;
            }

            //weights
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                float lowestDistance = 0;
                int number = 0;

                for (int j = 0; j < points.Count; j++)
                {
                    float newDistance = Vector3.Distance(mesh.vertices[i], points[j].transform.position);
                    if(newDistance < lowestDistance || j==0)
                    {
                        lowestDistance = newDistance;
                        number = j;
                    }
                }
                weights[i].boneIndex0 = number;
                weights[i].weight0 = 1;
            }
            
            Debug.Log(bindPoses[0]);

            meshRenderer.bones = bones;
            mesh.bindposes = bindPoses;
            mesh.boneWeights = weights;
        }
    }
}
