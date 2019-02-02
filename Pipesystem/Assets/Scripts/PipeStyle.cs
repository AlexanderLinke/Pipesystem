using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeStyle : ScriptableObject
{
    public float segmentLength;
    public float segmentDiameter;

    public List<GameObject> segmentPrefab;
    public List<int> segmentProbability;

    public List<GameObject> interjacentPrefab;
    public List<int> interjacentProbability;

    public Material material;

    public List<Color32> gizmoColors;
}
