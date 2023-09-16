using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private Mesh _characterMesh;
        [SerializeField] private float _harvestSpeed = 0.5f;
        [SerializeField] private int _walkRadius = 1;
        [SerializeField] private int _attackRadius = 1;
        [SerializeField] private float _attackSpeed = 1;
        
        public Mesh CharacterMesh => _characterMesh;
        public float HarvestSpeed => _harvestSpeed;
        public int WalkRadius => _walkRadius;
        public int AttackRadius => _attackRadius;
        public float AttackSpeed => _attackSpeed;
        
        
    }
}
