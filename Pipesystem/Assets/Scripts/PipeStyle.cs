using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pipestyle", menuName = "Pipestyle")]
public class PipeStyle : ScriptableObject
{

    public float segmentLength;

    public List<GameObject> segmentPrefab;
    public List<int> segmentProbability;

    public List<GameObject> interjacentPrefab;
    public List<int> interjacentProbability;

    public Material material;

    public List<Color32> gizmoColors;
}
