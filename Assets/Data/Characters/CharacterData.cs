using UnityEngine;

namespace Characters
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private Sprite _characterIcon;
        [SerializeField] private string _characterName;
        [SerializeField] private Mesh _characterMesh;
        [SerializeField] private float _harvestSpeed = 0.5f;
        [SerializeField] private float _health = 5;
        [SerializeField] private float _damage = 1;
        [SerializeField] private float _walkSpeed = 1;
        [SerializeField] private int _walkRadius = 1;
        [SerializeField] private int _attackRadius = 1;
        [SerializeField] private float _attackSpeed = 1;
        
        public Sprite CharacterIcon => _characterIcon;
        public string CharacterName => _characterName;
        public Mesh CharacterMesh => _characterMesh;
        public float Health => _health;
        public float Damage => _damage;
        public float WalkSpeed => _walkSpeed; 
        public float HarvestSpeed => _harvestSpeed;
        public int WalkRadius => _walkRadius;
        public int AttackRadius => _attackRadius;
        public float AttackSpeed => _attackSpeed;
        
        
    }
}
