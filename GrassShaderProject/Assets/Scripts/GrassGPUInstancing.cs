using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System;

public class GrassGPUInstancing : MonoBehaviour {

    private Matrix4x4[] matrixArray = new Matrix4x4[1000];
    private List<Matrix4x4[]> ListOfMatrixArrays = new List<Matrix4x4[]>();

    public Mesh grassMesh;
    public Material grassMaterial;
    private Matrix4x4 matrix = new Matrix4x4();
    private MaterialPropertyBlock mpb;
    private Vector4[] collisionBending = new Vector4[1000];
    private List<Vector4[]> ListOfCOllisionBendings = new List<Vector4[]>();

    public Collider[] colliders = new Collider[20];
    public Collider[] colliders2 = new Collider[5];

    public Vector3 overlapBoxPosition;
    public Vector3 overlapBoxExtents;

    public Vector3[] grassPositions;

    public LayerMask grassCutterLayer;
    public LayerMask grassCollisionLayer;

    // Particle System stuff
    public ParticleSystem particleSys = null;
    private Vector3[] cutPositions = new Vector3[1000];
    private int cutAmount = 0;
    private ParticleSystem.Particle[] particleArray;



    void Start()
    {
        particleArray = new ParticleSystem.Particle[particleSys.main.maxParticles];
        GetComponent<MeshCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        mpb = new MaterialPropertyBlock();

        ListOfMatrixArrays.Clear();
        ListOfCOllisionBendings.Clear();
        for (int i = 0; i < ((grassPositions.Length) / 1000) + 1; i++)
        {
            ListOfMatrixArrays.Add(new Matrix4x4[1000]);
            ListOfCOllisionBendings.Add(new Vector4[1000]);
        }

        int thousands = 0;
        int subindex = 0;
        for (int i = 0; i < grassPositions.Length; i++)
        {
            subindex = i % 1000;
            if (i != 0 && subindex == 0)
            {
                thousands += 1;
            }


            matrix.SetTRS(grassPositions[i], Quaternion.identity, Vector3.one);


            ListOfMatrixArrays[thousands][subindex] = matrix;


            float xbend = 0f;
            float zbend = 0f;

            ListOfCOllisionBendings[thousands][subindex].x = xbend;
            ListOfCOllisionBendings[thousands][subindex].z = zbend;
        }
    }

    public void updateHitBox(Vector3 pos, Vector3 size)
    {
        overlapBoxPosition = pos;
        overlapBoxExtents = size;  
    }



    void Update() {
        int colliderCount = Physics.OverlapBoxNonAlloc(overlapBoxPosition, overlapBoxExtents, colliders, Quaternion.identity, grassCollisionLayer);
        int colliderCount2 = Physics.OverlapBoxNonAlloc(overlapBoxPosition, overlapBoxExtents, colliders2, Quaternion.identity, grassCutterLayer);

        int thousands = 0;
        cutAmount = 0;

        for (int i = 0; i < grassPositions.Length; i++)
        {
            int subindex = i % 1000;
            if (i != 0 && subindex == 0)
            {
                thousands += 1;
            }

            for (int c = 0; c < colliderCount; c++)
            {
                float xdist = ListOfMatrixArrays[thousands][subindex].GetColumn(3).x - colliders[c].transform.position.x;
                float zdist = ListOfMatrixArrays[thousands][subindex].GetColumn(3).z - colliders[c].transform.position.z;
                Vector2 abc;
                abc.x = xdist;
                abc.y = zdist;
                Vector2 bend = abc.normalized * Mathf.Clamp(1f / abc.sqrMagnitude, 0, 1);

                Vector4 lerpTarget = Vector4.zero;
                Vector4 lerped = Vector4.zero;



                



                if ((xdist * xdist + zdist * zdist) < 20f)
                {
                    // Lay down
                    Vector2 currentDirection;

                    currentDirection.x = ListOfCOllisionBendings[thousands][subindex].x;
                    currentDirection.y = ListOfCOllisionBendings[thousands][subindex].z;


                    if (currentDirection.sqrMagnitude <= 0.4f)
                    {
                        lerpTarget.x = bend.x;
                        lerpTarget.z = bend.y;


                        float lerpSpeed = Time.deltaTime * 10f;
                        lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                        // Apply changes
                        ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                        ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                    }

                    if (currentDirection.sqrMagnitude > 0.4f && bend.sqrMagnitude > currentDirection.sqrMagnitude)
                    {
                        Vector2 newVec = currentDirection.normalized * bend.magnitude;
                        lerpTarget.x = newVec.x;
                        lerpTarget.z = newVec.y;

                        float lerpSpeed = Time.deltaTime * 10f;
                        lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                        // Apply changes
                        ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                        ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                    }
                }
                else
                {
                    // Raise
                    float lerpSpeed = Time.deltaTime * .12f;
                    lerped = Vector4.Lerp(ListOfCOllisionBendings[thousands][subindex], lerpTarget, lerpSpeed);
                    // Apply changes
                    ListOfCOllisionBendings[thousands][subindex].x = lerped.x;
                    ListOfCOllisionBendings[thousands][subindex].z = lerped.z;
                }

            }


            // fix initial value (Vector4 starts at 0,0,0,0, we want y to be 1)
            if (ListOfCOllisionBendings[thousands][subindex].y == 0)
            {
                ListOfCOllisionBendings[thousands][subindex].y = 1f;
            }




            // Check for Cutting
            for (int cut = 0; cut < colliderCount2; cut++)
            {
                float xdist2 = ListOfMatrixArrays[thousands][subindex].GetColumn(3).x - colliders2[cut].transform.position.x;
                float zdist2 = ListOfMatrixArrays[thousands][subindex].GetColumn(3).z - colliders2[cut].transform.position.z;
                Vector2 abc2;
                abc2.x = xdist2;
                abc2.y = zdist2;

                if (abc2.sqrMagnitude < 9 && ListOfCOllisionBendings[thousands][subindex].y > 0.4f)
                {
                    // Cut the Grass:
                    ListOfCOllisionBendings[thousands][subindex].y = .2f;
                    cutPositions[cutAmount++] = ListOfMatrixArrays[thousands][subindex].GetColumn(3);
                }           
            }
            // regrow Grass 
            if (ListOfCOllisionBendings[thousands][subindex].y < 1f)
            {
                ListOfCOllisionBendings[thousands][subindex].y += .01f * Time.deltaTime;
            }

        }

        // Because we have multiple grass blades per mesh, we increase the single_grass particles per Grass cut:
        cutAmount *= 6;
        // Particle Effect:
        if (cutAmount > 0)
        {
            int old_amount = particleSys.particleCount;
            particleSys.Emit(cutAmount);
            int newAmount = particleSys.GetParticles(particleArray);

            for (int i = old_amount; i < old_amount + cutAmount; i++)
            {
                particleArray[i].position = cutPositions[(i-old_amount) / 6];
            }
            particleSys.SetParticles(particleArray, newAmount);

            // disable the cut-colliders:
            for (int cut = 0; cut < colliderCount2; cut++)
            {
                // colliders2[cut].transform.parent.gameObject.SetActive(false);
            }
        }



        for (int i = 0; i < ListOfMatrixArrays.Count; i++)
        {
            mpb.SetVectorArray("_CollisionBending", ListOfCOllisionBendings[i]);
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, ListOfMatrixArrays[i], ListOfMatrixArrays[i].Length, mpb, ShadowCastingMode.On, false, 0, null);
        }

    }

}
