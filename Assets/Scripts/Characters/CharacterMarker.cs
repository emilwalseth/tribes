using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UI;
using UnityEngine;




public enum MarkerType
{
    None,
    Alert,
    Attack
}


public class CharacterMarker : Billboard
{
    
    [SerializeField] private MarkerType _markerType;
    [SerializeField] private List<Sprite> _markerSprites;
    
    private SpriteRenderer _spriteRenderer;
    
    
    protected override void Start()
    {
        base.Start();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        ChangeSprite(MarkerType.None);
        SetBehavior(BillboardBehavior.ZAxis);
    }

    public void SetMarker(MarkerType markerType)
    {
        bool clear = markerType == MarkerType.None;
        AnimationManager.Instance.DoMoveFromToAnimation(transform.gameObject, Vector3.zero, new Vector3(0,1,0), clear, 0.1f, false, clear ? ClearMarker : null);
        
        if(!clear)
            ChangeSprite(markerType);
    }
    
    private void ChangeSprite(MarkerType markerType)
    {
        if (!_spriteRenderer) return;
        
        _markerType = markerType;
        _spriteRenderer.sprite = _markerType switch
        {
            MarkerType.None => null,
            MarkerType.Alert => _markerSprites[0],
            MarkerType.Attack => _markerSprites[1],
            _ => throw new ArgumentOutOfRangeException(nameof(markerType), markerType, null)
        };
    }

    private void ClearMarker()
    {
        ChangeSprite(MarkerType.None);
    }
    
}
