using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreWidget : MonoBehaviour
    {
    
        [SerializeField] private TMP_Text _scoreText;
    
        public void SetScore(int score)
        {
            _scoreText.text = score.ToString();
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);
        }
    }
}
