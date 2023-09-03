using System;
using UnityEngine;

namespace Tiles
{
    public enum ResourceType
    {
        Wood,
    }
    
    [Serializable]
    public struct Resource
    {
        
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private Sprite _resourceIcon;
        [SerializeField] private int _amount;
        
        public ResourceType ResourceType => _resourceType;
        public Sprite ResourceIcon => _resourceIcon;
        public int Amount => _amount;
    }
}