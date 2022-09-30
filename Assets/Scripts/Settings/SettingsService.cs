using Core.Localization;
using Core.SaveLoad;
using Hellmade;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Settings
{
    [Serializable]
    public class SettingsServiceData
    {
        public float musicVolume = 1;
        public float soundsVolume = 1;
        public float volume = 1;
        public bool vibration;
        public bool notifications;
        public string language;
        public float textSize = 1;
    }

    public class SettingsService : MonoBehaviour
    {
        [SerializeField] private Saver saver;
        [SerializeField] private string defaultLanguage = "en";
        [SerializeField] private bool forceDefaultLanguageOnStart;

        public event Action SettingsChanged;
        public event Action SettingsLoaded;
        
        [Space]
        public UnityEvent UnknownLanguageLoaded;

        public bool MusicState => _data.musicVolume > 0;
        public bool SoundState => _data.soundsVolume > 0;
        public bool VibrationState => _data.vibration;
        public bool NotificationsState => _data.notifications;
        public float TextSize => _data.textSize;
        
        public float MusicVolume 
        {
            get { return _data.musicVolume; }
            set { SetMusicVolume(value); }
        }

        public float SoundVolume
        {
            get { return _data.soundsVolume; }
            set { SetSoundsVolume(value); }
        }

        public float Volume
        {
            get { return _data.volume; }
            set { SetVolume(value); }
        }

        public string Language
        {
            get { return _data.language; }
            set { SetLanguage(value); }
        }

        private SettingsServiceData _data = new();

        private EazySoundManager _eazySoundManager;
        private LocalizationRepository _localizationRepository;

        [Inject]
        public void Construct(
            EazySoundManager eazySoundManager,
            LocalizationRepository localizationRepository
            )
        {
            _eazySoundManager = eazySoundManager;
            _localizationRepository = localizationRepository;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }

        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }

        public void Init(SettingsServiceData data)
        {
            _data = data;

            _eazySoundManager.GlobalMusicVolume = _data.musicVolume;
            _eazySoundManager.GlobalSoundsVolume = _data.soundsVolume;
            _eazySoundManager.GlobalVolume = _data.volume;

            if (!IsLanguageValid(Language))
            {
                if(TryGetValidSystemLanguage(out var validLanguage) && !forceDefaultLanguageOnStart)
                {
                    SetLanguage(validLanguage);
                }
                else
                {
                    SetLanguage(defaultLanguage);
                    UnknownLanguageLoaded?.Invoke();
                }
            }

            _localizationRepository.SetLanguage(Language);
        }

        public List<string> GetLanguageList()
        {
            return _localizationRepository.GetLanguagesList();
        }

        public bool InvertSoundState()
        {
            var volume = _data.soundsVolume < float.Epsilon ? 1f : 0f;
            SetSoundsVolume(volume);
            return volume > 0;
        }
        
        public bool InvertMusicState()
        {
            var volume = _data.musicVolume < float.Epsilon ? 1f : 0f;
            SetMusicVolume(volume);
            return volume > 0;
        }

        public bool InvertVibrationState()
        {
            _data.vibration = !_data.vibration;
            return _data.vibration;
        }

        public bool InvertNotificationsState()
        {
            _data.notifications = !_data.notifications;
            return _data.notifications;
        }
        
        public void SetTextSize(float value)
        {
            _data.textSize = value;
            saver.SaveNeeded();
            SettingsChanged?.Invoke();
        }
        
        public void SetMusicVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.musicVolume, (float newVolume) => _eazySoundManager.GlobalMusicVolume = newVolume);
        }

        public void SetSoundsVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.soundsVolume, (float newVolume) => _eazySoundManager.GlobalSoundsVolume = newVolume);
        }

        public void SetVolume(float volume)
        {
            SetClampedVolume(volume, ref _data.volume, (float newVolume) => _eazySoundManager.GlobalSoundsVolume = newVolume);
        }
        
        public void SetLanguage(string language)
        {
            if (IsLanguageValid(language))
            {
                _data.language = language;
                _localizationRepository.SetLanguage(language);
                saver.SaveNeeded.Invoke();
                SettingsChanged?.Invoke();
            }
        }
        
        private void SetClampedVolume(float volume, ref float dataValue, Action<float> setManagerVolume)
        {
            var clampedVolume = Mathf.Clamp01(volume);
            dataValue = clampedVolume;

            setManagerVolume(clampedVolume);

            saver.SaveNeeded();
            SettingsChanged?.Invoke();
        }

        private bool IsLanguageValid(string language)
        {
            var languageList = GetLanguageList();
            return language != null && languageList.Contains(language);
        }

        private bool TryGetValidSystemLanguage(out string language)
        {
            if(TryGetSystemLanguageCountryCode(out var countryCode))
            {
                if (IsLanguageValid(countryCode))
                {
                    language = countryCode;
                    return true;
                }
            }

            language = null;
            return false;
        }

        private bool TryGetSystemLanguageCountryCode(out string countryCode)
        {
            var allCultures = CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            var systemLanguage = Application.systemLanguage.ToString();

            var languageCultureInfo = allCultures.FirstOrDefault(c => c.EnglishName == systemLanguage);
            if (languageCultureInfo != null)
            {
                var languageCountryCode = languageCultureInfo.TwoLetterISOLanguageName;
                languageCountryCode = languageCountryCode.Substring(0, 1).ToUpper() + languageCountryCode.Remove(0, 1);

                countryCode = languageCountryCode;
                return true;
            }

            countryCode = null;
            return false;
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new SettingsServiceData()));
            SettingsLoaded?.Invoke();
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}
