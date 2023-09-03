using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    
    
    public class PathRenderer : MonoBehaviour
    {

        [SerializeField] private bool _debug = false;
        [SerializeField] private GameObject _flagPrefab;
        [SerializeField] private List<Vector3> _debugPoints = new()
        {
            new Vector3(0, 0, 0),
            new Vector3(10, 0, 0),
            new Vector3(10, 0, 5),
            new Vector3(13, 0, 8),
        };
        [SerializeField] private float _lineThickness = 0.1f;
        [SerializeField] [Range(0.2f,10f)] private float _lineLength = 1;
        [SerializeField] private float _lineOffset = 0.5f;
        private GameObject _flag;
    
        private MeshFilter _meshFilter;
        private Unit OwningUnit { get; set; }
        
        private List<Vector3> _pathPoints = new();

        private void OnValidate()
        {
            if (!_debug) return;
            _pathPoints = _debugPoints;
            RebuildPath();
        }


        public void InitiatePath(Unit unit)
        {
            if (!unit) return;
            OwningUnit = unit;
            _meshFilter = GetComponent<MeshFilter>();

            List<Vector3> path = OwningUnit.CurrentNavigationPath;
            if (path.Count == 0) return;
            
            OwningUnit.CurrentPathRenderer = this;
            MakeFlag(path[^1]);
            UpdatePath();

        }
        
        private void MakeFlag(Vector3 position)
        {
            if (_flag)
            {
                _flag.transform.position = position;
                return;
            }
            _flag = Instantiate(_flagPrefab, position, Quaternion.identity);
            _flag.transform.parent = transform;
        }

        public void UpdatePath()
        {
            // Every Time our character updates its target tile, this will rebuild the path
            ConvertCharacterPath();
            RebuildPath();
        }

        private void UpdateEndpoint()
        {
            // This sets the last point of the pathPoints list to the current position of the character
            _pathPoints[^1] = OwningUnit.transform.position;
            RebuildPath();
        }

        private void ConvertCharacterPath()
        {
            List<Vector3> unitPath = OwningUnit.CurrentNavigationPath;
            if (unitPath.Count == 0)
            {
                Destroy(transform.gameObject);
                return;
            }
            
            Vector3 characterPosition = OwningUnit.transform.position;
            List<Vector3> pathCopy = unitPath.Select(t => t).ToList();
            pathCopy.Reverse();
            pathCopy.Add(characterPosition);
            _pathPoints = pathCopy;
        }

        private void Update()
        {
            UpdateEndpoint();
        }


        private void RebuildPath()
        {
            if (_pathPoints.Count == 0) return;

            Mesh mesh = new Mesh();

            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<int> triangles = new();

            float uvLength = 0;

            for (int i = 0; i < _pathPoints.Count; i++)
            {

                //if (!ValidatePoint(i, pathPoints[i])) continue;
                
                // Check if we can create a line from this point to the next
                int nextIndex = i + 1;
                if (nextIndex >= _pathPoints.Count) continue;
            
                Vector3 thisPoint = _pathPoints[i];
                Vector3 nextPoint = _pathPoints[nextIndex];

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


                
                // For if we want the UVs to be continuous (makes small weird spots)
                // Also replace 0 in x of uv to prev
                //float prev = uvLength;
                //uvLength += length / _lineLength;
                
                
                uvLength = length / _lineLength;

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
            
            //_previousPathPoints = pathPoints;
            
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            _meshFilter.mesh = mesh;
        }
    }
}
