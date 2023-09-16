using System;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public enum ToolType
    {
        None,
        Axe,
        Sword,
        Pick
    }
    
    
    [RequireComponent(typeof(MeshFilter))]
    public class CharacterTool : MonoBehaviour
    {
        
        [SerializeField] private List<Mesh> _toolMeshes = new();
        [SerializeField] private ToolType _toolType = ToolType.None;
        
        private MeshFilter _meshFilter;
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            SetTool(_toolType);
        }

        public void SetTool(ToolType toolType)
        {
            if (toolType == ToolType.None)
            {
                _meshFilter.mesh = null;
                return;
            }
            
            // Subtract one because of None
            int index = (int) toolType - 1;
            _meshFilter.mesh = _toolMeshes[index];
        }
        
    }
}
