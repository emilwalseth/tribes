using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public class AnimationManager : MonoBehaviour
    {

        public static AnimationManager Instance { get; private set; }
        private void Awake() => Instance = this;

        [SerializeField] private AnimationCurve _bounceCurve;
        [SerializeField] private AnimationCurve _bounceInCurve;
        [SerializeField] private AnimationCurve _easeCurve;


        private bool _drawFade = false;
        private float _fadeValue = 0.0f;


        public void DoBounceAnim(GameObject animObject, float duration = 0.25f, UnityAction animCallback = null)
        {
            StartCoroutine(BounceAnim(animObject, duration, animCallback));
        }
        
        public void DoMoveUpAnimation(GameObject animObject, float duration, float speed)
        {
            StartCoroutine(MoveAnim(animObject,  new Vector3(0, 1, 0), duration, speed));
        }
        
        public void DoFadeSpriteAnim(SpriteRenderer sprite, float duration, float fadeTo)
        {
            StartCoroutine(FadeSpriteAnim(sprite, duration, fadeTo));
        }
        
        public void DoFadeCamera(float duration, float fadeFrom, float fadeTo)
        {
            StartCoroutine(FadeCamera(duration, fadeFrom, fadeTo));
        }

        public void SetFadeValue(float fadeValue)
        {
            if (fadeValue == 0)
            {
                _drawFade = false;
                _fadeValue = 0;
                return;
            }
            _drawFade = true;
            _fadeValue = fadeValue;
        }
        
        public void DoMoveToAnimation(GameObject animObject, Vector3 destination, float duration, bool worldSpace = true)
        {
            StartCoroutine(MoveToAnim(animObject, destination, _easeCurve, false, duration, worldSpace));
        }
        
        
        public void DoRotateToAnimation(GameObject animObject, Quaternion target, float duration, bool worldSpace = true)
        {
            StartCoroutine(RotateToAnim(animObject, target, duration, worldSpace));
        }
        
        public void DoMoveFromToAnimation(GameObject animObject, Vector3 from, Vector3 to, bool inverse, float duration, bool worldSpace = true, UnityAction animCallback = null)
        {
            StartCoroutine(MoveFromToAnim(animObject, from, to, _bounceInCurve, inverse, duration, worldSpace, animCallback));
        }
        
        private static IEnumerator MoveAnim(GameObject animObject, Vector3 direction, float duration, float speed)
        {
            float moveAmount = (speed / duration) * Time.deltaTime;
            float targetTime = Time.realtimeSinceStartup + duration;
            
            while (Time.realtimeSinceStartup < targetTime)
            {
                if (!animObject) yield break;
                
                animObject.transform.position += moveAmount * direction.normalized;

                yield return null;
            }
        }
        
        private IEnumerator FadeCamera(float duration, float fadeFrom, float fadeTo)
        {
            _drawFade = true;
            _fadeValue = fadeFrom;
            float startTime = Time.realtimeSinceStartup;
            float percent = 0;
            
            while (Math.Abs(percent - 1) > 0)
            {
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                float value = _easeCurve.Evaluate(percent);
                _fadeValue = Mathf.Lerp(fadeFrom, fadeTo, value);
                yield return null;
            }
            _drawFade = false;
        }

        private void OnGUI()
        {
            if (!_drawFade) return;

            Texture2D texture = new (1, 1);
            texture.SetPixel(0, 0, new Color(0, 0, 0, _fadeValue));
            texture.Apply();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
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
        
        private IEnumerator MoveToAnim(GameObject animObject, Vector3 destination, AnimationCurve curve, bool inverse, float duration, bool worldSpace = true, UnityAction animCallback = null)
        {
            Vector3 startPos = worldSpace ? animObject.transform.position : animObject.transform.localPosition;
            float startTime = Time.realtimeSinceStartup;
            float percent = 0;
            
            while (Math.Abs(percent - 1) > 0)
            {   
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                float value = curve.Evaluate(inverse ? 1 - percent : percent);

                if (worldSpace)
                    animObject.transform.position = Vector3.Lerp(startPos, destination, value);
                else
                    animObject.transform.localPosition = Vector3.Lerp(startPos, destination, value);
                
                yield return null;
            }
        }
        
        private IEnumerator RotateToAnim(GameObject animObject, Quaternion target, float duration, bool worldSpace = true)
        {
            Quaternion startRot = worldSpace ? animObject.transform.rotation : animObject.transform.localRotation;
            float startTime = Time.realtimeSinceStartup;
            float percent = 0;
            
            while (Math.Abs(percent - 1) > 0)
            {   
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                float value = _easeCurve.Evaluate(percent);

                if (worldSpace)
                    animObject.transform.rotation = Quaternion.Lerp(startRot, target, value);
                else
                    animObject.transform.localRotation = Quaternion.Lerp(startRot, target, value);
                
                yield return null;
            }
        }
        
        private IEnumerator MoveFromToAnim(GameObject animObject, Vector3 from, Vector3 to, AnimationCurve curve, bool inverse, float duration, bool worldSpace = true, UnityAction animCallback = null)
        {
            float startTime = Time.realtimeSinceStartup;
            float percent = 0;
            
            while (Math.Abs(percent - 1) > 0)
            {   
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                float value = curve.Evaluate(inverse ? 1 - percent : percent);

                if (worldSpace)
                    animObject.transform.position = Vector3.LerpUnclamped(from, to, value);
                else
                    animObject.transform.localPosition = Vector3.LerpUnclamped(from, to, value);
                
                yield return null;
            }
            animCallback?.Invoke();
        }


        private IEnumerator ScaleAnim(GameObject animObject, float duration, bool inverse, AnimationCurve curve, UnityAction animCallback)
        {
            
            float percent = 0;
            float startTime = Time.realtimeSinceStartup;
            Vector3 start = animObject.transform.localScale;
            
            while (Math.Abs(percent - 1) > 0)
            {
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                
                float value = curve.Evaluate(inverse ? 1 - percent : percent);
                
                animObject.transform.localScale = start * value;
                yield return null;
            }
            animCallback?.Invoke();
            animObject.transform.localScale = start;
        }
        
        private IEnumerator BounceAnim(GameObject animObject, float duration, UnityAction animCallback)
        {
            
            float percent = 0;
            float startTime = Time.realtimeSinceStartup;
            Vector3 start = Vector3.one;
            Vector3 end = new (0, 0, 0);
            
            while (Math.Abs(percent - 1) > 0)
            {
                percent = Mathf.InverseLerp(startTime, startTime + duration, Time.realtimeSinceStartup);
                
                float value = _bounceCurve.Evaluate(percent);
                animObject.transform.localScale = Vector3.LerpUnclamped(start, end, value * 0.3f);
                yield return null;
            }

            animCallback?.Invoke();
        }
        
    }
}
