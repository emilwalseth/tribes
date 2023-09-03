using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Billboard : MonoBehaviour
    {
        
        private Camera _camera;

        protected virtual void Start()
        {
            _camera = Camera.main;
            RotateToCamera();
        }

        private void RotateToCamera()
        {
            if (!_camera) return;
            transform.LookAt(_camera.transform.position, Vector3.up);
        }
    }
}
