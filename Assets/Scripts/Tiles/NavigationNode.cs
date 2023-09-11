using UnityEngine;

namespace Tiles
{
    public class NavigationNode : MonoBehaviour
    {
        
        // Navigation Variables

        [SerializeField] private int _nodeWeight = 0;
        [SerializeField] private bool _isWalkable = true;

        
        public NavigationNode Parent { get; set; }
        public bool IsWalkable { get => _isWalkable; set => _isWalkable = value; }
        public int NodeWeight{ get => _nodeWeight; set => _nodeWeight = value; }
        
        public int GCost { get; set; }
        public int HCost { get; set; }
    
        public int FCost => GCost + HCost;
    
    }
}
