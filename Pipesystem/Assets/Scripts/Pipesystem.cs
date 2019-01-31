using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipesystem : MonoBehaviour
{
    public List<PipePoint> pipePoints;
    public PipeStyle pipeStyle;

    public GameObject pipePointHolder;
    public GameObject pipeLineHolder;

    public float segmentLength;
    public float segmentDiameter;

    public bool isLinked;
    public bool selectControlPipePointsOnly;

    //Prefabs with style
    public List<GameObject> segmentPrefab;
    public List<int> segmentProbability;

    public List<GameObject> interjacentPrefab;
    public List<int> interjacentProbability;

    //Gizmo Colors
    public List<Color32> gizmoColors;

    public Material mainMaterial;

}
