using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrassShaper : MonoBehaviour {

    
    public Mesh mesh;
    public bool updateGrass = false;
    public LayerMask grassShaperLayer;
    public LayerMask grassPlacementLayer;
    public GrassGPUInstancing targetGrassSpawner;
    private List<Vector3> newSpots;
    public float raycastHeight = 5f;



    void Update () {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        if (updateGrass)
        {
            updateBoundingBox();
            updateGrassPoints();
            updateGrass = false;
        }
    }

    void updateBoundingBox()
    {
        mesh.RecalculateBounds();
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + mesh.bounds.center, mesh.bounds.size);
    }


    void updateGrassPoints ()
    {
        Vector3 startSpot = transform.position + mesh.bounds.min;
        Vector3 currentSpot = transform.position + mesh.bounds.min;
        Vector3 endSpot = transform.position + mesh.bounds.max;
        int counter = 0;
        RaycastHit hit;


        Debug.DrawLine(startSpot, startSpot + Vector3.up * 7, Color.blue);
        Debug.DrawLine(endSpot, endSpot + Vector3.up * 7, Color.yellow);

        int hits = 0;
        newSpots = new List<Vector3>();

        while (counter < 100000 && (currentSpot.x < endSpot.x || currentSpot.z < endSpot.z))
        {
            counter++;

            if (Physics.Raycast(currentSpot + Vector3.up * raycastHeight, Vector3.down, 30f, grassShaperLayer))
            {
                if (Physics.Raycast(currentSpot + Vector3.up * raycastHeight, Vector3.down, out hit, 30f, grassPlacementLayer))
                {
                    hits += 1;
                    Debug.DrawLine(hit.point, hit.point + Vector3.up * 3);
                    newSpots.Add(hit.point);
                }
            }

            currentSpot += Vector3.forward * 0.8f;
            if(currentSpot.z > endSpot.z && currentSpot.x < endSpot.x)
            {
                currentSpot.z = startSpot.z;
                currentSpot += Vector3.right * 0.8f;
            }
        }
        Debug.Log("counter : " + counter + " hits: " + hits);

                                       
        targetGrassSpawner.grassPositions = new Vector3[hits];
        Debug.Log("assigning new array size:" + targetGrassSpawner.grassPositions.Length);
        for (int i = 0; i < hits; i++)
        {
            targetGrassSpawner.grassPositions[i] = newSpots[i];
        }

        targetGrassSpawner.updateHitBox(transform.position + mesh.bounds.center, mesh.bounds.extents);
    }
}

