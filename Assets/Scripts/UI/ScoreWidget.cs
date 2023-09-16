using System;
using System.Linq;
using Data.Resources;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScoreWidget : MonoBehaviour
    {

        [SerializeField] private ResourceData _resourceData;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _scoreText;

        private void Start()
        {
            _icon.sprite = _resourceData.ResourceIcon;
            StatsManager.Instance.onStatsChanged += SetScore;
        }
        
        
        private void OnDisable()
        {
            if (!StatsManager.Instance) return;
            StatsManager.Instance.onStatsChanged -= SetScore;
        }

        private void SetScore()
        {
            int count = 0;
            if (StatsManager.Instance.Resources.TryGetValue(_resourceData, out int amount))
                count = amount;

            
            string score = count.ToString();
            
            if (score == _scoreText.text) return;
            
            _scoreText.text = score;
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);

        }
    }
}
