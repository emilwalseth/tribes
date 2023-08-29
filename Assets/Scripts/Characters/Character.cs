using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using World;

namespace Characters
{
    public class Character : MonoBehaviour, IInteractable
    {

        [SerializeField] private float _movementSpeed = 1f;
    
        private Animator _animator;
        private PathRenderer _currentPathRenderer;

        public GameObject CurrentTile { get; set; }

        private List<GameObject> _navigationPath = new List<GameObject>();
        

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
            MakePathRenderer();
        }

        private void MakePathRenderer()
        {
            DestroyRenderPath();
            _currentPathRenderer = GameManager.Instance.MakePathRenderer(GetRenderPathPoints());
        }

        private List<Vector3> GetRenderPathPoints()
        {
            List<Vector3> pathPoints = _navigationPath.Select(t => t.transform.position).ToList();
            pathPoints.Reverse();
            pathPoints.Add(transform.position);

            return pathPoints;
        }

        private void UpdatePathRenderer()
        {
            if (_currentPathRenderer)
            {
                _currentPathRenderer.MakePath(GetRenderPathPoints());
            }
            else
            {
                MakePathRenderer();
            }
        }

        private void Update()
        {
            MoveAlongPath();
        }
        
        private void DestroyRenderPath()
        {
            if (!_currentPathRenderer) return;
            Destroy(_currentPathRenderer.gameObject);
            _currentPathRenderer = null;
        }

        private void MoveAlongPath()
        {
            if (_navigationPath.Count == 0)
            {
                _animator.SetBool(IsMoving, false);
                DestroyRenderPath();
                return;
            };
            _animator.SetBool(IsMoving, true);

            Vector3 currentPos = transform.position;
            Vector3 currentTarget = _navigationPath[0].transform.position;

            float distance = Vector3.Distance(currentPos, currentTarget);

            // Move towards current target
            transform.position = Vector3.MoveTowards(currentPos, currentTarget, _movementSpeed * Time.deltaTime);
            RotateTowardsPosition(currentTarget);
            UpdatePathRenderer();
            
            // We are close enough to the point to go to next point.
            if (distance < 0.1f)
            {
                CurrentTile = _navigationPath[0];
                _navigationPath.RemoveAt(0);
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
