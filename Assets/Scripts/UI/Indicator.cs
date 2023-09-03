using System;
using System.Collections;
using Managers;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Indicator : Billboard
    {
        private SpriteRenderer _spriteRenderer;


        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(DoAnim());
        }
        
        public void SetImage(Sprite sprite)
        {
            _spriteRenderer.sprite = sprite;
        }
        
        IEnumerator DoAnim()
        {
            AnimationManager.Instance.DoMoveUpAnimation(gameObject, 0.5f, 0.7f);
            yield return new WaitForSeconds(0.3f);
            AnimationManager.Instance.DoFadeSpriteAnim(_spriteRenderer, 0.2f, 0);
            yield return new WaitForSeconds(0.3f);
            Destroy(gameObject);
        }
    }
}
