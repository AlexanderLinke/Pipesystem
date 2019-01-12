using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeLine : MonoBehaviour {

    public List<PipeSegment> segments;
    public List<GameObject> interjacents;

    public GameObject SegmentHolder;
    public GameObject InterjacentHolder;

    public Pipesystem correspondingPipesystem;

    public float distance;

    public PipePoint startPipePoint;
    public PipePoint endPipePoint;

    void OnDrawGizmos()
    {
        if (correspondingPipesystem.isLinked)
        {
            Gizmos.color = correspondingPipesystem.gizmoColors[4];
            Gizmos.DrawLine(startPipePoint.transform.position, endPipePoint.transform.position);
        }
    }
}
