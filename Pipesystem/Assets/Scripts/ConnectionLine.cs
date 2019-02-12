using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionLine : MonoBehaviour {

    public List<Segment> segments;
    public List<GameObject> interjacents;

    public GameObject SegmentHolder;
    public GameObject InterjacentHolder;

    public Pipesystem correspondingPipesystem;

    public float distance;

    public ControlPoint startControlPoint;
    public ControlPoint endControlPoint;

    public GameObject startPosition;
    public GameObject endPosition;

    void OnDrawGizmos()
    {
        if (correspondingPipesystem.isLinked)
        {
            Gizmos.color = correspondingPipesystem.gizmoColors[4];
            Gizmos.DrawLine(startControlPoint.transform.position, endControlPoint.transform.position);
        }
    }
}
