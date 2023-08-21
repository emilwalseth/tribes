using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private MapGenerator _map;
    [SerializeField] private GameObject _campsitePrefab;
    [SerializeField][Range(0f,1f)] private float _zoomStrength = 1;
    [SerializeField] private float _zoomSpeed = 5;
    [SerializeField] private float _cameraDeceleration = 10;
    [SerializeField] private Vector2 _zoomMaxMin = new(1,20);
    
    private Camera _camera;

    

    // Interaction variables
    private GameObject _selectedObject;
    private Vector3 _startClickPosition;
    private float _timeSinceClick;

    // Movement variables
    private float _zoomTarget;
    private int _lastFingerIndex;
    private Vector3 _lastTouchPosition;
    private bool _canMove;
    private Vector3 _cameraVelocity;



    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Start()
    {
        _zoomTarget = _camera.orthographicSize;
    }

    // Update is called once per frame
    private void Update()
    {
        _timeSinceClick += Time.deltaTime;
        
        CheckClick();
        CheckZoom();
        CheckMovementInput();
        CameraDeceleration();
        CameraZoomInterpolation();
    }
    

    private void CheckClick()
    {
        if (IsMouseOverUI()) return;

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
        SetSelectedObject(hitObject);
            
        // Get interactable
        IInteractable interactable = hitObject.GetComponent<IInteractable>();

        // Interact with interactable interface
        interactable?.OnClicked();

    }

    public void MakeCampsite()
    {
        if (!_map || !_selectedObject || !_campsitePrefab) return;

        if (_selectedObject.GetComponent<TileScript>())
        {
            _map.ReplaceTile(_selectedObject, _campsitePrefab);
        }
    }
    
    private bool IsMouseOverUI()
    {
        bool touchOver = false;
        if (Input.touchCount > 0)
        {
            touchOver = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        }

        bool pointerOver = EventSystem.current.IsPointerOverGameObject(0);

        return touchOver || pointerOver;
    }
    private void CameraZoomInterpolation()
    {
        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _zoomTarget , _zoomSpeed * Time.deltaTime);
    }
    
    private void CameraDeceleration()
    {
        // Lerp the velocity to apply deceleration
        _cameraVelocity = Vector3.Lerp(_cameraVelocity, Vector3.zero, _cameraDeceleration * Time.deltaTime);
        
        // Dont apply the velocity if we are holding the button
        if (Input.touchCount > 0 || Input.GetMouseButton(0)) return;
        
        _camera.transform.position += _cameraVelocity;
    }


    private void SetSelectedObject(GameObject newSelected)
    {
        if (!newSelected) return;
        if (newSelected == _selectedObject) return;

        if (_selectedObject)
        {
            _selectedObject.GetComponent<IInteractable>()?.OnDeselected();
        }
        
        _selectedObject = newSelected;
        
    }


    private void CheckMovementInput()
    {

        Vector3 delta = Vector3.zero;
        
        if (Input.GetMouseButtonDown(0))
        {
            _lastTouchPosition = Input.mousePosition;
            _canMove = !IsMouseOverUI();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.fingerId != _lastFingerIndex && touch.phase == TouchPhase.Began)
            {
                _lastTouchPosition = touch.position;
                _lastFingerIndex = touch.fingerId;
                _canMove = !IsMouseOverUI();
            }
            
            if (touch.phase == TouchPhase.Moved)
            {
                Vector3 currentWorld = _camera.ScreenToWorldPoint(touch.position);
                Vector3 lastWorld =  _camera.ScreenToWorldPoint(_lastTouchPosition);
                delta = currentWorld - lastWorld;
            
                _lastTouchPosition = touch.position;
            }
        }

        else
        {
            _lastFingerIndex = -1;
            if (Input.GetMouseButton(0) && _canMove)
            {
                Vector3 currentWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 lastWorld = _camera.ScreenToWorldPoint(_lastTouchPosition);
                
                delta = currentWorld - lastWorld;
                _lastTouchPosition = Input.mousePosition;
            }
        }

        if (delta != Vector3.zero && _canMove)
        {
            _cameraVelocity = -delta;
            _camera.transform.position -= delta;
        }
        

    }


    private void CheckZoom()
    {
        
        float currentZoom = _camera.orthographicSize;
        

        if (Input.mouseScrollDelta != Vector2.zero)
        {
           
            float changeAmount = currentZoom * _zoomStrength;
            
            if (Input.mouseScrollDelta.y > 0)
            {
                _zoomTarget = Mathf.Clamp(_camera.orthographicSize - changeAmount, _zoomMaxMin.x, _zoomMaxMin.y);
            }
            else
            {
                _zoomTarget = Mathf.Clamp(_camera.orthographicSize + changeAmount, _zoomMaxMin.x, _zoomMaxMin.y);
            }
        }

        // We have mobile zoom! Check the distance between the touches and set the zoom based on these.
        if (Input.touchCount >= 2)
        {
            Vector3 touch1Current = Input.GetTouch(0).position;
            Vector3 touch2Current = Input.GetTouch(1).position;
            
            Vector3 touch1Prev = Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition;
            Vector3 touch2Prev = Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition;


            float previousDistance = Vector3.Magnitude(touch1Prev - touch2Prev);
            float currentDistance = Vector3.Magnitude(touch1Current - touch2Current);
            
            float difference = currentDistance - previousDistance;

            _zoomTarget = Mathf.Clamp(_camera.orthographicSize - (difference * (_zoomStrength * 0.2f)), _zoomMaxMin.x, _zoomMaxMin.y);

        }

    }
    
}
