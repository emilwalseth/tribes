using System;
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
            StatsManager.Instance.onStatsChanged -= SetScore;
        }

        private void SetScore()
        {
            int newCount = StatsManager.Instance.Resources[_resourceData];

            string score = newCount.ToString();
            
            if (score == _scoreText.text) return;
            
            _scoreText.text = score;
            AnimationManager.Instance.DoBounceAnim(gameObject, 0.25f);

        }
    }
}
