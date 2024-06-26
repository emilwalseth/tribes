using System;
using Characters;
using Interfaces;
using Managers;
using Tiles;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {

        [Header("General")]
        [SerializeField] private Camera _camera;
        [SerializeField] private Camera _otherCamera;

        // Interaction variables
        private Vector3 _startClickPosition;
        private float _timeSinceClick;

        [Space(10)]
    
        // Movement variables
        [Header("Movement")]
        [SerializeField] private float _cameraDeceleration = 10;
        private bool _isMoving;
        private Vector3 _lastTouchPosition;
        private bool _canMove;
        private Vector2 _cameraVelocity;
        private Vector2 _boundsMinMaxX;
        private Vector2 _boundsMinMaxZ;
    
        [Space(10)]
        // Zooming variables
        [Header("Zooming")]
        [SerializeField][Range(0f,1f)] private float _zoomStrength = 1;
        [SerializeField] private Vector2 _zoomMaxMin = new(1,20);
        private bool _isZooming;
        private float _targetZoom;
        private Vector3 _zoomTargetCenter;
    
        private void Start()
        {
            
            // Set spawn position to the spawned hero
            transform.position = MapManager.Instance.GetMapCenter();
            AnimationManager.Instance.SetFadeValue(1);
            EventManager.Instance.onHeroSpawned += hero =>
            {
                AnimationManager.Instance.DoFadeCamera(0.4f, 1, 0);
                AnimationManager.Instance.DoMoveToAnimation(gameObject, hero.transform.position, 1f);
            };

            _targetZoom = _camera.orthographicSize;
        
            InitMapBounds();
        
            Application.targetFrameRate = 60;

            if (Application.isMobilePlatform)
            {
                QualitySettings.vSyncCount = 0;
            }

        }
        
        private void SetCameraOrthographicSize(float size)
        {
            _camera.orthographicSize = size;
            _otherCamera.orthographicSize = size;
        }

        // Update is called once per frame
        private void Update()
        {
            _timeSinceClick += Time.deltaTime;
        
            // None of these will be called if we are over a UI
            if (IsMouseOverUI()) return;

            // Check Click
            CheckClick();
        
            // Move Camera
            CheckMovement();

            // Zoom Camera
            CheckZoom();
        
            // Zoom Bouncing
            ZoomBounce();
        
            // Clamp Pos
            ClampMovement();
        }

        private void Move(Vector2 delta)
        {
        
            Vector3 vel = new Vector3(delta.x, 0, delta.y * 2);
            transform.transform.position += vel;
        }

        private void InitMapBounds()
        {
        
            Vector3 mapMax = MapManager.Instance.GetMapMax();
            Vector3 mapMin = MapManager.Instance.GetMapMin();

            const float extraPadding = 2;
        
            _boundsMinMaxX.x = mapMin.x - extraPadding;
            _boundsMinMaxX.y = mapMax.x + extraPadding;

            _boundsMinMaxZ.x = mapMin.z - extraPadding;
            _boundsMinMaxZ.y = mapMax.z + extraPadding;
        
        }

        private void ClampMovement()
        {
            Vector3 currentPos = transform.position;
            float edgeDistance = _camera.orthographicSize/2;
        
            float minX = _boundsMinMaxX.x;
            float maxX = _boundsMinMaxX.y;
            float minZ = _boundsMinMaxZ.x;
            float maxZ = _boundsMinMaxZ.y;

            float xPos = Mathf.Clamp(currentPos.x, minX, maxX);
            float zPos = Mathf.Clamp(currentPos.z, minZ, maxZ);


            // If we are not currently moving the camera, lerp back towards center if the camera is on the edge
            if (!_isMoving)
            {
                if (xPos < minX + edgeDistance)
                {
                    _cameraVelocity = Vector3.zero;
                    xPos = Mathf.Lerp(xPos, minX + edgeDistance, 6f * Time.deltaTime);
                    xPos = Mathf.Round(xPos * 1000.0f) * 0.001f;
                }
                if (xPos > maxX - edgeDistance)
                {
                    _cameraVelocity = Vector3.zero;
                    xPos = Mathf.Lerp(xPos, maxX - edgeDistance, 6f * Time.deltaTime);
                    xPos = Mathf.Round(xPos * 1000.0f) * 0.001f;
                }
                if (zPos < minZ + edgeDistance)
                {
                    _cameraVelocity = Vector3.zero;
                    zPos = Mathf.Lerp(zPos, minZ + edgeDistance, 6f * Time.deltaTime);
                    zPos = Mathf.Round(zPos * 1000.0f) * 0.001f;
                }
                if (zPos > maxZ - edgeDistance)
                {
                    _cameraVelocity = Vector3.zero;
                    zPos = Mathf.Lerp(zPos, maxZ - edgeDistance, 6f * Time.deltaTime);
                    zPos = Mathf.Round(zPos * 1000.0f) * 0.001f;
                }
 
            }
        
            // Lerp camera back to center
            transform.position = new Vector3(xPos, currentPos.y, zPos);
        
        }


        private void CheckMovement()
        {

            if (Application.isMobilePlatform)
            {
                HandleTouchMovement();
            }
            else
            {
                HandlePCMovement();
            }
        }
    
    
        private void HandlePCMovement()
        {
            // PC start movement input
            if (Input.GetMouseButtonDown(0))
            {
                _lastTouchPosition = Input.mousePosition;
                _canMove = !IsMouseOverUI();
            }

            // PC is moving pointer
            if ( Input.GetMouseButton(0) && _canMove)
            {
                _isMoving = true;
            
                // Makes the move delta based on the last and current position
                Vector3 currentWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 lastWorld =  _camera.ScreenToWorldPoint(_lastTouchPosition);
                Vector3 diff = currentWorld - lastWorld;
                Vector2 delta = new Vector2(diff.x, diff.z);
                _lastTouchPosition = Input.mousePosition;
            
                SetVelocity(-delta);
                Move(-delta);
            }

            else
            {
                _isMoving = false;
                Move(_cameraVelocity);
                CameraDeceleration();
            }
        }

        private void HandleTouchMovement()
        {
            if (Input.touchCount >= 1)
            {
                Touch touch0 = Input.touches[0];
                Vector2 centerTouchPos = Vector2.zero;
                Vector2 centerTouchDelta = Vector2.zero;

                // Gather the center of all touches, in case more than one finger is on the screen.
                // This helps with panning while zooming
                foreach (Touch touch in Input.touches)
                {
                    centerTouchPos += touch.position;
                    centerTouchDelta += touch.deltaPosition;
                }
            
                centerTouchPos /= Input.touchCount;
                centerTouchDelta /= Input.touchCount;
            
            
            
                // If this is the first frame of panning, check if we can move
                if (!_isMoving) _canMove = !IsMouseOverUI();

                _isMoving = true;
            
            
                if (touch0.phase == TouchPhase.Moved && _canMove)
                {
                
                    // Make the move delta based on the current and the last touch positions
                    Vector3 touchPosPrev =  centerTouchPos - centerTouchDelta;
                    Vector3 currentWorld = _camera.ScreenToWorldPoint(centerTouchPos);
                    Vector3 lastWorld =  _camera.ScreenToWorldPoint(touchPosPrev);
                    Vector3 diff = currentWorld - lastWorld;
                    Vector2 delta = new Vector2(diff.x, diff.z);
                    _lastTouchPosition = centerTouchPos;
                
                    SetVelocity(-delta);
                    Move(-delta);
                }
                else if (touch0.phase == TouchPhase.Stationary)
                {
                    SetVelocity(Vector2.zero);
                }
            }
            else
            {
                _isMoving = false;
                Move(_cameraVelocity);
                CameraDeceleration();
            }
        }
    
        private void CheckZoom()
        {

            if (Application.isMobilePlatform)
            {
                HandleTouchZoom();
            }
            else
            {
                HandlePCZoom();
            }
        }


        private void HandleTouchZoom()
        {
        
            float currentZoom = _camera.orthographicSize;
        
        
            if (Input.touchCount == 2)
            {
            
                Touch touch0 = Input.touches[0];
                Touch touch1 = Input.touches[1];

                Vector3 touch0Current = touch0.position;
                Vector3 touch1Current = touch1.position;
            
                Vector3 touch0Prev =  touch0.position - touch0.deltaPosition;
                Vector3 touch1Prev =  touch1.position - touch1.deltaPosition;
            
                // This is first frame of zooming
                if (!_isZooming)
                {
                
                }
            
                _isZooming = true;

                // Get difference between touch distances last frame and current frame
                float previousDistance = Vector3.Magnitude(touch0Prev - touch1Prev);
                float currentDistance = Vector3.Magnitude(touch0Current - touch1Current);
                float difference = currentDistance - previousDistance;

                float zoomAmount = currentZoom * _zoomStrength;

                _targetZoom = Mathf.Clamp(currentZoom - difference * (zoomAmount * 0.3f * Time.deltaTime), _zoomMaxMin.x, _zoomMaxMin.y);
            
                // Panning the camera while zooming
                // Get position of center before zoom
                Vector3 currentCenter = (touch0Current + touch1Current) * 0.5f;
                Vector3 centerWorldBefore = _camera.ScreenToWorldPoint(currentCenter);
            
                // Do the zoom
                SetCameraOrthographicSize(_targetZoom);
                // Get world point after
                Vector3 centerWorldAfter = _camera.ScreenToWorldPoint(currentCenter);
            
                // Get the delta between them and pan the camera accordingly
                Vector3 delta = centerWorldBefore - centerWorldAfter;
            
                Move(delta);
            }
            else
            {
                _isZooming = false;
            }

        }


        private void HandlePCZoom()
        {

            float currentZoom = _camera.orthographicSize;
        
        
            if (Input.mouseScrollDelta != Vector2.zero)
            {
                _isZooming = true;
            
                // Calculate the amount we need to zoom in or out based on the current zoom value.
            
                bool zoomOut = Input.mouseScrollDelta.y > 0;
                float zoomAmount = currentZoom * _zoomStrength;
            
                zoomAmount = zoomOut ? -zoomAmount : zoomAmount;
                _targetZoom = currentZoom + zoomAmount;
                _targetZoom = Mathf.Clamp(_targetZoom, _zoomMaxMin.x, _zoomMaxMin.y);
            }
            else
            {
                _isZooming = false;
            }

            if (Math.Abs(currentZoom - _targetZoom) > 0.5f)
            {
                // Apply pan to zoom location
                Vector3 centerBefore = _camera.ScreenToWorldPoint(Input.mousePosition);
            
                // Do the zoom
                // Apply zoom lerping for smooth zooming
                SetCameraOrthographicSize(Mathf.Lerp(currentZoom, _targetZoom, 15 * Time.deltaTime));
            
                Vector3 centerAfter = _camera.ScreenToWorldPoint(Input.mousePosition);
            
                // Move the camera so it keeps the hovered location in focus
                Vector3 delta = centerBefore - centerAfter;
                Move(delta);
            }
        
        
        
        }
    

        private void CheckClick()
        {

            // Check when the user start clicking
            if (Input.GetMouseButtonDown(0))
            {
                _timeSinceClick = 0;
                _startClickPosition = Input.mousePosition;
            }
        
            // Check if user clicked
            if (!Input.GetMouseButtonUp(0)) return;
        
            // If we have clicked for long, it is not a click, but a hold.
            if (_timeSinceClick > 0.25f) return;

            // If we have moved, it is not a click, but a drag
            const float similarityThreshold = 10;
            float distance = Vector3.Distance(_startClickPosition, Input.mousePosition);

            // Also, if the user has dragged, dont click
            if (distance > similarityThreshold) return;
        
            // Make Ray from mousePosition
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            
            // Cast the ray and check hit
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            // Get object from hit
            GameObject hitObject = hit.transform.gameObject;

            // Select the new object
            if(hitObject.TryGetComponent(out IInteractable interactable))
            {
                interactable.OnClicked();
            }
            


        }

        private void CameraDeceleration()
        {

            if (_cameraVelocity.magnitude < Time.deltaTime * 0.2f)
            {
                _cameraVelocity = Vector3.zero;
            }
        
            _cameraVelocity = Vector3.Lerp(_cameraVelocity, Vector3.zero, _cameraDeceleration * Time.deltaTime);
        }

    
        private void ZoomBounce()
        {
            if (_isZooming) return;
        
            const float maxBounce = 4f;
            const float minBounce = 1f;
        
            float size = _camera.orthographicSize;
            
            // If the camera is zoomed all the way in or out, move the camera a bit back to give some feedback to the user
            if (size < _zoomMaxMin.x + minBounce)
            {
                size = Mathf.Lerp(size, _zoomMaxMin.x + minBounce, 3f * Time.deltaTime);
                size = Mathf.Round(size * 1000.0f) * 0.001f;
                SetCameraOrthographicSize(size);
                _targetZoom = size;
            }
            if (size > _zoomMaxMin.y - maxBounce)
            {
 
                size = Mathf.Lerp(size, _zoomMaxMin.y - maxBounce, 3f * Time.deltaTime);
                size = Mathf.Round(size * 1000.0f) * 0.001f;
                SetCameraOrthographicSize(size);
                _targetZoom = size;
            }
        }
    
        private void SetVelocity(Vector2 velocity)
        {

            // Clamps the velocity so it never goes too fast
            _cameraVelocity = Vector3.ClampMagnitude(velocity, 50 * Time.deltaTime);
        }
    
    
    
        private static bool IsMouseOverUI()
        {
            return Input.touchCount > 0 ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
