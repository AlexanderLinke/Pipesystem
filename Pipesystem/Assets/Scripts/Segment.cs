using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment : MonoBehaviour {

    public float diameter;
    public float length;
    public int segmentNumber;
    public int indexInLine;

    public bool isHoverd;

    public Pipesystem pipesystem;

    void OnDrawGizmos()
    {
        if(isHoverd && pipesystem.isLinked)
        {
            Gizmos.color = pipesystem.gizmoColors[5];
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(diameter, diameter, length));
        }
    }

}
