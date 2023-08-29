using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PathRenderer : MonoBehaviour
    {

        [SerializeField] private bool _debug = false;
        [SerializeField] private List<Vector3> _debugPoints = new List<Vector3>
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 0, 5),
            new Vector3(13, 0, 8),
        };
        [SerializeField] private float _lineThickness = 0.1f;
        [SerializeField] [Range(0.2f,10f)] private float _lineLength = 1;
        [SerializeField] private float _lineOffset = 0.5f;
    
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
    

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnValidate()
        {
            if (!_debug) return;
        
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            MakePath(_debugPoints);
        }

        public void MakePath(List<Vector3> pathPoints)
        {

            if (pathPoints.Count == 0) return;

            Mesh mesh = new Mesh();

            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();
        
            for (int i = 0; i < pathPoints.Count; i++)
            {
                // Check if we can create a line from this point to the next
                int nextIndex = i + 1;
                if (nextIndex >= pathPoints.Count) continue;
            
                Vector3 thisPoint = pathPoints[i];
                Vector3 nextPoint = pathPoints[nextIndex];

                // Make a line from this point to the next
                Vector3 direction = (nextPoint - thisPoint).normalized;
                Vector3 pointOffset = direction * _lineOffset;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
                float length = Vector3.Distance(thisPoint, nextPoint);                
                
                // Make two points at the start
                
                Vector3 vert0 = (nextPoint - pointOffset) + perpendicular * _lineThickness;
                Vector3 vert1 = (nextPoint - pointOffset) - perpendicular * _lineThickness;
                
                // Make two points at the end

                Vector3 vert2 = (thisPoint + pointOffset) + perpendicular * _lineThickness;
                Vector3 vert3 = (thisPoint + pointOffset) - perpendicular * _lineThickness;
                
                int vertCount = vertices.Count;
                
                // Add the verts
                vertices.Add(vert0);
                vertices.Add(vert1);
                vertices.Add(vert2);
                vertices.Add(vert3);
                
                
                // Make the triangles for these corners
                
                //       2-------3
                //       |       |
                //       |       |
                //       |       |
                //       0-------1
               
                float uvLength = length / _lineLength;

                uvs.Add(new Vector2(uvLength,0));
                uvs.Add(new Vector2(uvLength,1));
                uvs.Add(new Vector2(0,1));
                uvs.Add(new Vector2(0,0));

               
                triangles.Add(vertCount + 1);
                triangles.Add(vertCount + 3);
                triangles.Add(vertCount + 0);
                
                triangles.Add(vertCount + 3);
                triangles.Add(vertCount + 2);
                triangles.Add(vertCount + 0);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            _meshFilter.mesh = mesh;
        }
    }
}
