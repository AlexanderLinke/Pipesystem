using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour {

    public List<ControlPoint> connectedControlPoints;
    public List<ConnectionLine> connectionLines;

    public int connectionCount;
    public Vector3 oldPosition;

    public bool isSelectedControlPoint;

    public SnapPoint snapPoint;

    public GameObject model;

    public Pipesystem correspondingPipesystem;
    public int indexInPipesystem;

    public float gizmoSize;

    void OnDrawGizmos()
    {
        if (correspondingPipesystem.isLinked)
        {
            if (!isSelectedControlPoint)
            {
                Gizmos.color = correspondingPipesystem.gizmoColors[0];
                Gizmos.DrawWireSphere(transform.position, gizmoSize);

                Gizmos.color = correspondingPipesystem.gizmoColors[1];
                Gizmos.DrawSphere(transform.position, gizmoSize);
            }
            else
            {
                Gizmos.color = correspondingPipesystem.gizmoColors[2];
                Gizmos.DrawWireSphere(transform.position, gizmoSize);

                Gizmos.color = correspondingPipesystem.gizmoColors[3];
                Gizmos.DrawSphere(transform.position, gizmoSize);
            }
        }
    }
}
