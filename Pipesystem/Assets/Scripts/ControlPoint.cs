using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour {

    public List<ControlPoint> connectedControlPoints;
    public List<ConnectionLine> connectionLines;

    public bool isSelectedControlPoint;

    public Pipesystem correspondingPipesystem;

    public Vector3 oldPosition;

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
                Debug.Log("bob");
                Gizmos.color = correspondingPipesystem.gizmoColors[2];
                Gizmos.DrawWireSphere(transform.position, gizmoSize);

                Gizmos.color = correspondingPipesystem.gizmoColors[3];
                Gizmos.DrawSphere(transform.position, gizmoSize);
            }
        }
    }
}
