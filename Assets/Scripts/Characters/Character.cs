using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Characters
{
    public class Character : MonoBehaviour, IInteractable
    {

        [SerializeField] private float _movementSpeed = 1f;
    
        private Animator _animator;
        private Vector3 _lastFramePos;

        public GameObject CurrentTile { get; set; }

        private List<GameObject> _navigationPath = new List<GameObject>();

        
        // Actions
        public UnityAction onUpdatePath;

        // Animation values
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");


        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void SetNewNavigationPath(List<GameObject> newNavigationPath)
        {
            if (newNavigationPath == null) return;

            _navigationPath = newNavigationPath;
        }
        
        
        // Start is called before the first frame update
        void Start()
        {
            _lastFramePos = transform.position;
        }

        private void Update()
        {
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            if (_navigationPath.Count == 0)
            {
                _animator.SetBool(IsMoving, false);
                return;
            };
            _animator.SetBool(IsMoving, true);

            Vector3 currentPos = transform.position;
            Vector3 currentTarget = _navigationPath[0].transform.position;

            float distance = Vector3.Distance(currentPos, currentTarget);

            // Move towards current target
            transform.position = Vector3.MoveTowards(currentPos, currentTarget, _movementSpeed * Time.deltaTime);
            RotateTowardsPosition(currentTarget);
            
            // We are close enough to the point to go to next point.
            if (distance < 0.1f)
            {
                CurrentTile = _navigationPath[0];
                _navigationPath.RemoveAt(0);
                onUpdatePath?.Invoke();
            }
        }
        public void OnClicked()
        {
            print("Clicked");
            _animator.Play("OnClicked",0,0);
        }

        public List<GameObject> GetCurrentPath()
        {
            return _navigationPath;
        }

        private void RotateTowardsPosition(Vector3 target)
        {
            Vector3 direction = (target - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime * 10f);
        }

        public void OnDeselected()
        {
        
        }
    }
}
