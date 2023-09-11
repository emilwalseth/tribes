using System;
using System.Collections;
using UnityEngine;

namespace Managers
{
    public class AnimationManager : MonoBehaviour
    {

        public static AnimationManager Instance { get; private set; }
        private void Awake() => Instance = this;

        [SerializeField] private AnimationCurve _bounceCurve;


        public void DoBounceAnim(GameObject animObject, float duration = 0.25f)
        {
            StartCoroutine(BounceAnim(animObject, duration));
        }
        
        public void DoMoveUpAnimation(GameObject animObject, float duration, float speed)
        {
            StartCoroutine(MoveAnim(animObject,  new Vector3(0, 1, 0), duration, speed));
        }
        
        public void DoFadeSpriteAnim(SpriteRenderer sprite, float duration, float fadeTo)
        {
            StartCoroutine(FadeSpriteAnim(sprite, duration, fadeTo));
        }
        
        private static IEnumerator MoveAnim(GameObject animObject, Vector3 direction, float duration, float speed)
        {
            float moveAmount = (speed / duration) * Time.deltaTime;
            float targetTime = Time.realtimeSinceStartup + duration;
            
            while (Time.realtimeSinceStartup < targetTime)
            {
                animObject.transform.position += moveAmount * direction.normalized;
                
                yield return null;
            }
        }

        private IEnumerator FadeSpriteAnim(SpriteRenderer sprite, float duration, float fadeTo)
        {
            float startOpacity = sprite.color.a;
            float startTime = Time.realtimeSinceStartup;
            float percent = 0;
            
            while (Math.Abs(percent - 1) > 0)
            {
                Color color = sprite.color;
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                sprite.color = new Color(color.r, color.g, color.b, Mathf.Lerp(startOpacity, fadeTo, percent));
                yield return null;
            }

        }

        private IEnumerator BounceAnim(GameObject animObject, float duration)
        {
            
            float percent = 0;
            float startTime = Time.realtimeSinceStartup;
            
            while (Math.Abs(percent - 1) > 0)
            {
                Vector3 start = new Vector3(1, 1, 1);
                Vector3 end = new Vector3(0, 0, 0);
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                
                float value = _bounceCurve.Evaluate(percent);
                
                animObject.transform.localScale = Vector3.LerpUnclamped(start, end, value * 0.3f);
                yield return null;
            }

        }
    }
}
