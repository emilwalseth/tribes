using System;
using System.Collections;
using System.Collections.Generic;
using Characters;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PathRenderer : MonoBehaviour
{
    

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Character _targetCharacter;

    
    
    public void SetCharacter(Character targetCharacter)
    {
        _targetCharacter = targetCharacter;
        //_targetCharacter.onUpdatePath += UpdatePath;
        //UpdatePath();
    }

    private void Update()
    {

    }

    private void OnValidate()
    {
        
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        print("Making");
        MakePath();
    }

    [SerializeField] public Vector3[] _points;  // The points the line should pass through
    [SerializeField] public float _lineThickness = 0.1f;
    [SerializeField] private float _lineLength = 20;


    private void MakePath()
    {
        if (_points.Length == 0) return;

        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<int> triangles = new();

        Vector3 previousDirection = Vector3.zero;



        for (int i = 0; i < _points.Length; i++)
        {
            // Check if we can create a line from this point to the next
            int nextIndex = i + 1;
            if (nextIndex < _points.Length)
            {
                Vector3 thisPoint = _points[i];
                Vector3 nextPoint = _points[nextIndex];

                // Make a line from this point to the next
                Vector3 direction = (nextPoint - thisPoint).normalized;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
                
                
                
                // Make two points angled towards the direction

                Vector3 vert0 = thisPoint + perpendicular * _lineThickness;
                Vector3 vert1 = thisPoint - perpendicular * _lineThickness;

                // Make the next two points
                Vector3 vert2 = nextPoint + perpendicular * _lineThickness;
                Vector3 vert3 = nextPoint - perpendicular * _lineThickness;
    
                int vertCount = vertices.Count;
                
                // Change the previous verts to match the new ones
                int prev0Index = vertCount - 2;
                int prev1Index = vertCount - 1;

                // Get the distance we need to move the verts along
                float adjustAmountMultiplier = Vector3.Dot(direction, previousDirection) - 1;
                float adjustAmount = adjustAmountMultiplier * (_lineThickness);

                // Adjusts
                Vector3 currentAdjust = -direction * adjustAmount;
                Vector3 previousAdjust = previousDirection * adjustAmount;

                // Determine if the verts are on the inside or outside corner, if we are on a corner
                if (adjustAmount != 0 && (prev0Index >= 0 || prev1Index >= 0))
                {

                    Vector3 previousPos0 = vertices[prev0Index];
                    Vector3 previousPos1 = vertices[prev1Index];

                    Vector3 edge0 = vert0 - vert1;
                    Vector3 edge1 = previousPos1 - previousPos0;

                    Vector3 crossProduct = Vector3.Cross(edge0.normalized, edge1.normalized);

                    // If it is positive, the larger of the two is on the inside
                    if (crossProduct.y > 0)
                    {
                        vert0 -= currentAdjust;
                        vert1 += currentAdjust;
                        
                        previousPos0 -= previousAdjust;
                        previousPos1 += previousAdjust;

                        vert0 = Vector3.Lerp(vert0, previousPos0, 0.5f);
                        vert1 = Vector3.Lerp(vert1, previousPos1, 0.5f);

                        vertices[prev0Index] = vert0;
                        vertices[prev1Index] = vert1;
                        
                        uvs[prev0Index] = VertToUV(vert0, previousDirection);
                        uvs[prev1Index] = VertToUV(vert1, previousDirection);
                    }
                    // If not, the larger of the two are on the inside
                    else
                    {
                        vert0 += currentAdjust;
                        vert1 -= currentAdjust;
                        
                        previousPos0 += previousAdjust;
                        previousPos1 -= previousAdjust;

                        vert0 = Vector3.Lerp(vert0, previousPos0, 0.5f);
                        vert1 = Vector3.Lerp(vert1, previousPos1, 0.5f);
                        
                        vertices[prev0Index] = vert0;
                        vertices[prev1Index] = vert1;

                        uvs[prev0Index] = VertToUV(vert0, previousDirection);
                        uvs[prev1Index] = VertToUV(vert1, previousDirection);

                    }
                    
                    // Add the verts to the list
                    vertices.Add(vert0);
                    vertices.Add(vert1);
                    uvs.Add(VertToUV(vert0, direction));
                    uvs.Add(VertToUV(vert1, direction));

                }
                else
                {
                    // Add the verts to the list
                    vertices.Add(vert0);
                    vertices.Add(vert1);
                    uvs.Add(VertToUV(vert0, direction));
                    uvs.Add(VertToUV(vert1, direction));
                    
                }
                
                previousDirection = direction;

                // Add the rest of the verts currentVerts
                vertices.Add(vert2);
                vertices.Add(vert3);

                uvs.Add(VertToUV(vert2, direction));
                uvs.Add(VertToUV(vert3, direction));
                
                
                // Make the triangles for these corners
                
               //       2-------3
               //       |       |
               //       |       |
               //       |       |
               //       0-------1
                
                triangles.Add(vertCount + 0);
                triangles.Add(vertCount + 3);
                triangles.Add(vertCount + 1);
                
                triangles.Add(vertCount + 0);
                triangles.Add(vertCount + 2);
                triangles.Add(vertCount + 3);
                
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        _meshFilter.mesh = mesh;
    }

    private Vector2 VertToUV(Vector3 vert, Vector3 direction)
    {
        Vector2 uv = new();

        float dot = Vector3.Dot(direction, new Vector3(1, 0, 0));
        print(dot);
        Quaternion rotation = Quaternion.Euler(0,360 * dot, 0);
        Vector3 pos = rotation * vert;

        uv.x = pos.x * _lineLength;
        uv.y = pos.z;
        
        return uv;
    }
}
