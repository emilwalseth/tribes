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
        [SerializeField] private TrailRenderer _trail;
        
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            SetTool(_toolType);
        }

        public void SetHidden(bool newHidden)
        {
            _meshRenderer.enabled = !newHidden;
            _trail.emitting = !newHidden;
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
