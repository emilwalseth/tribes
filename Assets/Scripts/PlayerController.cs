using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private MapGenerator _map;
    [SerializeField] private GameObject _campsitePrefab;
    
    private Camera _camera;


    private GameObject _selectedObject;
    

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    
    // Update is called once per frame
    void Update()
    {
        CheckClick();
    }
    
    
    void CheckClick()
    {
        if (IsMouseOverUI()) return;

        
        if (!Input.GetMouseButtonUp(0)) return;
        
        
        Vector2 mousePos = Input.mousePosition;
            
        // Make Ray from mousePosition
        Ray ray = _camera.ScreenPointToRay(mousePos);
            
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

    private bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void MakeCampsite()
    {
        if (!_map || !_selectedObject || !_campsitePrefab) return;
        
        _map.ReplaceTile(_selectedObject, _campsitePrefab);

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

    
}
