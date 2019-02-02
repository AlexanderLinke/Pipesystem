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

    void OnDrawGizmos()
    {
        if (correspondingPipesystem.isLinked)
        {
            if (!isSelectedControlPoint)
            {
                Gizmos.color = correspondingPipesystem.gizmoColors[0];
                Gizmos.DrawWireSphere(transform.position, 1);

                Gizmos.color = correspondingPipesystem.gizmoColors[1];
                Gizmos.DrawSphere(transform.position, 1);
            }
            else
            {
                Gizmos.color = correspondingPipesystem.gizmoColors[2];
                Gizmos.DrawWireSphere(transform.position, 1);

                Gizmos.color = correspondingPipesystem.gizmoColors[3];
                Gizmos.DrawSphere(transform.position, 1);
            }
        }
    }
}
