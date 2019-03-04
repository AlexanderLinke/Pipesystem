using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipesystem : MonoBehaviour
{
    public List<ControlPoint> controlPoints;
    public PipeStyle pipeStyle;
    public bool isLinked;
    public bool selectControlPointsOnly;
    public bool recalculateAll;

    //ObjectHolder
    public GameObject controlPointHolder;
    public GameObject connectionLineHolder;
    public GameObject snapPointsHolder;
    public GameObject controlPointModelHolder;

    //segment
    public float segmentLength;
    public float segmentDiameter;
    public float distanceSegmentsControlPoint;
    public int segmentBendPoints;

    //interjacent
    public bool interjacentAtConnectionPoint;
    public bool interjacentAtEndPoint;

    //ControlPoint
    public int controlPointBendPoints;
    public float controlPointCurveStrength;

    //Prefabs with style
    public List<GameObject> segmentPrefab;
    public List<int> segmentProbability;

    public List<GameObject> bendSegmentPrefab;
    public List<GameObject> modifiedBendSegmentPrefab;
    public List<int> bendSegmentProbability;

    public List<GameObject> interjacentPrefab;
    public List<int> interjacentProbability;

    public List<GameObject> endPointPrefab;
    public List<int> endPointProbability;

    public List<GameObject> snappablePrefab;
    public List<int> snappableProbability;

    public List<GameObject> controlPointPrefab;
    public List<GameObject> modifiedControlPointPrefab;
    public List<int> controlPointProbability;

    //Gizmos
    public List<Color32> gizmoColors;
    public float gizmoSizeControlPoints;
    public float gizmoSizeSnapPoints;

    public Material mainMaterial;

}
