using System;
using UnityEngine;

namespace Data.Resources
{
    
    [Serializable]
    public class ResourceKeyValuePair
    {
        
        [SerializeField] private ResourceData _resourceData;
        [SerializeField] private int _amount;
        
        public ResourceData Resource { get => _resourceData; set => _resourceData = value; }
        public int Amount { get => _amount; set => _amount = value; }
    }
    
    
    [Serializable]
    public enum ResourceType
    {
        Wood,
        Stone,
        Metal
    }
    
    [CreateAssetMenu(fileName = "ResourceData", menuName = "ScriptableObjects/ResourceData", order = 1)]
    public class ResourceData : ScriptableObject
    {
        
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private Sprite _resourceIcon;
        
        public ResourceType ResourceType => _resourceType;
        public Sprite ResourceIcon => _resourceIcon;
    }
}