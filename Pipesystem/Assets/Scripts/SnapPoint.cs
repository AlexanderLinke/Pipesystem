﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SnapPoint : MonoBehaviour {

    public bool isConnected;

    public List<PipeStyle> connectablePipestyles;

    public float gizmoSize = 1;

    private Color32 hiddenColor = new Color32(0, 0, 0, 0);

    void OnDrawGizmos()
    {
        if(!isConnected)
        {
            if(PipesystemManager.activePipesystem!=null && connectablePipestyles.Contains(PipesystemManager.activePipesystem.pipeStyle))
            {
                Gizmos.DrawSphere(transform.position, gizmoSize);
                Gizmos.DrawWireSphere(transform.position, gizmoSize);
            }
            else
            {
                //creates a sneaky collider for selection
                Gizmos.color = hiddenColor;
                Gizmos.DrawSphere(transform.position, gizmoSize);
                Gizmos.DrawWireSphere(transform.position, gizmoSize);
            }
        }
    }
}
