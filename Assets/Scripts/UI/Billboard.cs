using System;
using UnityEngine;

namespace UI
{

    public enum BillboardBehavior
    {
        AllAxis,
        ZAxis
    }
    
    [RequireComponent(typeof(SpriteRenderer))]
    public class Billboard : MonoBehaviour
    {
        
        private Camera _camera;
        private BillboardBehavior _behavior;

        protected virtual void Start()
        {
            _camera = Camera.main;
            RotateToCamera();
        }
        
        public void SetBehavior(BillboardBehavior behavior)
        {
            _behavior = behavior;
        }

        private void RotateToCamera()
        {
            if (!_camera) return;
            
            Vector3 forward = _camera.transform.forward;
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

            switch (_behavior)
            {
                case BillboardBehavior.AllAxis:
                    transform.rotation = rotation;
                    break;
                case BillboardBehavior.ZAxis:
                    transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            

        }
    }
}
