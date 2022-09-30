using Settings;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class TextSizeSetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float delta = 0f;
        
        private float _baseSize;

        private SettingsService _settingsService;

        [Inject]
        public void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsService.SettingsChanged += OnSettingsChanged;
            _settingsService.SettingsLoaded += OnSettingsChanged;
            _baseSize = text.fontSize;
            UpdateValue();
        }

        private void OnDestroy()
        {
            _settingsService.SettingsChanged -= OnSettingsChanged;
            _settingsService.SettingsLoaded -= OnSettingsChanged;
        }

        private void OnSettingsChanged()
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            text.fontSize = _baseSize * (_settingsService.TextSize - delta);
        }
    }
}
