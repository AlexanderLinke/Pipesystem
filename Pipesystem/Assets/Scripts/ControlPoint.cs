using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour {

    public List<ControlPoint> connectedControlPoints;
    public List<ConnectionLine> connectionLines;
    public int connectionCount;

    public Pipesystem correspondingPipesystem;
    public int indexInPipesystem;

    public Vector3 oldPosition;

    public bool isSelectedControlPoint;

    public SnapPoint snapPoint;

    //ControlPointModel
    public GameObject modelHolder;
    public List<GameObject> models;

    public Vector3[] positionsBendModel;
    public Vector3[] rotationsBendModel;

    public int indexStartBendModel;
    public int indexEndBendModel;

    public List<GameObject> modelConnectionPoints;
    public Vector3 modelCenter;

    public GameObject startCurveInfluencePoint;
    public GameObject endCurveInfluencePoint;

    public List<GameObject> curveSwitschGizmos;

    //Gizmos
    public float gizmoSize;

    void OnDrawGizmos()
    {
        if (correspondingPipesystem!=null && correspondingPipesystem.isLinked)
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
