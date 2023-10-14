using Managers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CheatManager))]
    public class CheatInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            CheatManager cheatManager = (CheatManager) target;
            
            
            if (GUILayout.Button("Add Wood"))
            {
                // Only if playing the game
                if (Application.isPlaying)
                    CheatManager.AddWood(100);
            }
            
        }
    }
}
