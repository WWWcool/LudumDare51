using Core.SaveLoad;
using Core.Windows;
using TMPro;
using UnityEngine;
using Zenject;
using UI;

namespace Settings
{
    public class SettingsPopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;

        // [SerializeField] private SwitchImageButton soundButton;
        // [SerializeField] private SwitchImageButton musicButton;
        // [SerializeField] private SwitchImageButton vibroButton;
        // [SerializeField] private SwitchImageButton notificationsButton;
        [SerializeField] private SwitchNumberOption textSizeOption;
        [SerializeField] private TextMeshProUGUI versionText;

        private SettingsService _settingsService;

        [Inject]
        public void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;

            popupBase.Inited += OnInit;
            _settingsService.SettingsChanged += OnSettingsChanged;
            textSizeOption.ValueChanged += OnTextSizeChanged;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= OnInit;
            _settingsService.SettingsChanged -= OnSettingsChanged;
            textSizeOption.ValueChanged -= OnTextSizeChanged;
        }

        private void OnInit()
        {
            UpdateState();
        }

        public void OnSaveReset()
        {
            SaveService.ResetSaveData();
        }

        public void OnSoundStateChanged()
        {
            var state = _settingsService.InvertSoundState();
            // soundButton.UpdateStateImage(state);
        }

        public void OnMusicStateChanged()
        {
            var state = _settingsService.InvertMusicState();
            // musicButton.UpdateStateImage(state);
        }

        public void OnVibrationStateChanged()
        {
            var state = _settingsService.InvertVibrationState();
            // vibroButton.UpdateStateImage(state);
        }

        public void OnNotificationsStateChanged()
        {
            var state = _settingsService.InvertNotificationsState();
            // notificationsButton.UpdateStateImage(state);
        }

        private void OnTextSizeChanged(float value)
        {
            _settingsService.SetTextSize(value);
        }

        private void UpdateState()
        {
            // soundButton.UpdateStateImage(_settingsService.SoundState);
            // musicButton.UpdateStateImage(_settingsService.MusicState);
            // vibroButton.UpdateStateImage(_settingsService.VibrationState);
            // notificationsButton.UpdateStateImage(_settingsService.NotificationsState);
            textSizeOption.Value = _settingsService.TextSize;
            versionText.text = $"Build: v{Application.version}";
        }

        private void OnSettingsChanged()
        {
            UpdateState();
        }
    }
}