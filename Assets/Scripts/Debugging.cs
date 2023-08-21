using UnityEngine;
using System.Collections;
 
public class MobileUtilsScript : MonoBehaviour {
 
    private int FramesPerSec;
    private float frequency = 1.0f;
    private string fps;

    [SerializeField] private bool _showDebug;
 
 
 
    void Start(){
        StartCoroutine(FPS());
    }
 
    private IEnumerator FPS() {
        for(;;){
            // Capture frame-per-second
            int lastFrameCount = Time.frameCount;
            float lastTime = Time.realtimeSinceStartup;
            yield return new WaitForSeconds(frequency);
            float timeSpan = Time.realtimeSinceStartup - lastTime;
            int frameCount = Time.frameCount - lastFrameCount;
           
            // Display it
 
            fps = string.Format("FPS: {0}" , Mathf.RoundToInt(frameCount / timeSpan));
        }
    }
 
 
    void OnGUI()
    {
        if (!_showDebug) return;

        GUI.skin.label.fontSize = 100;
        GUI.Label(new Rect(Screen.width - 600,100,600,100), fps);
    }
}
