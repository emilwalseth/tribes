using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private Mesh _characterMesh;
        [SerializeField] private float _woodChopSpeed = 0.5f;
        [SerializeField] private int _walkRadius = 1;
        
        public Mesh CharacterMesh => _characterMesh;
        public float WoodChopSpeed => _woodChopSpeed;
        public int WalkRadius => _walkRadius;
        
        
    }
}
