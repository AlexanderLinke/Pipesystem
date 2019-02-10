using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeStyle : ScriptableObject
{
    //segment
    public float segmentLength;
    public float segmentDiameter;
    public float distanceSegmentsControlPoint;

    //prefabs
    public List<GameObject> segmentPrefab;
    public List<int> segmentProbability;

    public List<GameObject> interjacentPrefab;
    public List<int> interjacentProbability;

    public List<GameObject> endPointPrefab;
    public List<int> endPointProbability;

    public List<GameObject> snappablePrefab;
    public List<int> snappableProbability;

    public Material material;

    //Gizmo
    public List<Color32> gizmoColors;

    public float gizmoSizeControlPoints;
    public float gizmoSizeSnapPoint;
}
