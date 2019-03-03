using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutomaticWeightPainting : Editor
{

    public static GameObject CalculateWeights(Pipesystem pipesystem, GameObject originalPrefab)
    {
        Transform[] bones;
        GameObject prefab;

        
        prefab = (GameObject)PrefabUtility.InstantiatePrefab(originalPrefab);


        SkinnedMeshRenderer meshRenderer;
        Mesh mesh;

        //get mesh
        meshRenderer = prefab.GetComponent<SkinnedMeshRenderer>();
        if(meshRenderer==null)
            meshRenderer = prefab.AddComponent<SkinnedMeshRenderer>();

        meshRenderer.sharedMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        mesh = meshRenderer.sharedMesh;
        mesh.boneWeights = null;
        mesh.bindposes = null;

        //weigth paint
        Matrix4x4[] bindPoses = new Matrix4x4[pipesystem.controlPointBendPoints];
        BoneWeight[] weights = new BoneWeight[mesh.vertexCount];

        bool creatNewTransforms = false;

        //bones
        if (meshRenderer.bones == null)
        {
            creatNewTransforms = true;
            bones = new Transform[pipesystem.controlPointBendPoints];

        }
        else
            bones = meshRenderer.bones;


        for (int i = 0; i < pipesystem.controlPointBendPoints; i++)
        {
            if(creatNewTransforms)
            {
                bones[i] = new GameObject().transform;
                bones[i].name = i.ToString();
                bones[i].localRotation = Quaternion.identity;

                float zPosition = -pipesystem.distanceSegmentsControlPoint + ((pipesystem.distanceSegmentsControlPoint * 2) / (pipesystem.controlPointBendPoints - 1) * i);
                bones[i].localPosition = new Vector3(0, 0, zPosition);
            }

            bindPoses[i] = bones[i].worldToLocalMatrix * prefab.transform.localToWorldMatrix;
            bones[i].transform.parent = prefab.transform;
        }

            

        //weights
        for (int i = 0; i < mesh.vertexCount; i++)
        {

            float distanceToPoint = 0;
            float distanceToClosestPoint = bones[0].transform.position.z * -2;
            float distanceBetweenPoints = bones[1].transform.position.z - bones[0].transform.position.z;
            int closestPoint = 0;
            int secondClosestPoint = 1;
            float weigthOfClosestPoint;

            for (int j = 0; j < pipesystem.controlPointBendPoints; j++)
            {
                distanceToPoint = mesh.vertices[i].z - bones[j].transform.position.z;

                if (distanceToPoint < 0)
                    distanceToPoint = distanceToPoint * -1;

                if (distanceToPoint <= distanceToClosestPoint)
                {
                    secondClosestPoint = closestPoint;
                    closestPoint = j;
                    distanceToClosestPoint = distanceToPoint;
                }
            }

            weigthOfClosestPoint = (distanceBetweenPoints - (bones[closestPoint].transform.position.z - mesh.vertices[i].z)) / distanceBetweenPoints;

            weights[i].boneIndex0 = closestPoint;
            weights[i].weight0 = weigthOfClosestPoint;

            weights[i].boneIndex1 = secondClosestPoint;
            weights[i].weight1 = 1 - weigthOfClosestPoint;

        }

        meshRenderer.bones = bones;
        mesh.bindposes = bindPoses;
        mesh.boneWeights = weights;

        return prefab;
    }
}
