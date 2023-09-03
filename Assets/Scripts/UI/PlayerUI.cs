using Managers;
using UnityEngine;

namespace UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField] private ScoreWidget _woodCountWidget;
    
        private void Start()
        {
            ScoreManager.Instance.onWoodChanged += newScore => _woodCountWidget.SetScore(newScore);
        }
    }
}
