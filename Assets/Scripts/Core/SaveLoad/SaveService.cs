using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Core.SaveLoad
{
    [Serializable]
    public class LoadContext
    {
        public TimeSpan playerOfflineTime;
    }

    public class SaveService : MonoBehaviour
    {
        [SerializeField] private bool verbose;
        [SerializeField] private string defaultSaveKey;
        [SerializeField] private string timestampSaveKey;
        [SerializeField] private string privacyPolicySaveKey;
        [SerializeField] private string termsOfUseSaveKey;
        [SerializeField] private List<Saver> savers;

        public event Action<LoadContext> LoadFinished = context => { };
        public bool SaveLocked { get; set; }
        public IReadOnlyList<string> SaveKeys => _saveKeysHolder.Data.keys;
        public string CurrentKey => _currentKey;
        public bool IsGameNew => _newGame;

        private const string SaveKeysHolderKey = "SaveKeysHolderKey";
        private const string CurrentSaveKey = "DefaultKey";

        private SaveKeysHolder _saveKeysHolder;

        private bool _needSave;
        private string _currentKey;
        private bool _newGame;
        private bool _privacyPolicyAgreed;
        private bool _termsOfUseAgreed;

        private void Start()
        {
            _saveKeysHolder = new SaveKeysHolder(SaveKeysHolderKey, new List<string> {defaultSaveKey});
            _currentKey = PlayerPrefs.GetString(CurrentSaveKey, defaultSaveKey);

            foreach (var saver in savers)
            {
                saver.SaveNeeded += OnSaveNeeded;
            }

            Load();
        }

        private void OnDestroy()
        {
            foreach (var saver in savers)
            {
                saver.SaveNeeded -= OnSaveNeeded;
            }
        }

        private void Update()
        {
            if (_needSave && !SaveLocked)
            {
                _needSave = false;
                Save();
            }
        }

        public void SetAgreementState(bool termsOfUse)
        {
            if (termsOfUse)
            {
                _termsOfUseAgreed = true;
            }
            else
            {
                _privacyPolicyAgreed = true;
            }
            ForceSave();
        }
        
        public bool GetAgreementState(bool termsOfUse)
        {
            if (termsOfUse)
            {
                return _termsOfUseAgreed;
            }
            return _privacyPolicyAgreed;
        }
        
        public void ForceSave()
        {
            Save();
        }

        /// <summary>
        /// Creates new save controller to hold and use data.
        /// </summary>
        /// <param name="key">Key of created controller</param>
        /// <param name="empty">If true - creates controller from zero progress. Creates controller from current progress otherwise</param>
        public void CreateSave(string key, bool empty = false)
        {
            _saveKeysHolder.AddKey(key);
            SaveTo(key, empty);
        }

        /// <summary>
        /// Saves current progress to save with specified key.
        /// </summary>
        /// <param name="key">Specified key</param>
        /// <param name="empty">Create empty save</param>
        public void SaveTo(string key = null, bool empty = false)
        {
            key = String.IsNullOrEmpty(key) ? defaultSaveKey : key;
            foreach (var saver in savers)
            {
                if (verbose)
                {
                    var saveKey = key + saver.Key;
                    var dataToSave = empty ? "" : saver.DataSaved?.Invoke();
                    PlayerPrefs.SetString(saveKey, dataToSave);
                    print($"[SaveService][SaveTo] key: {saveKey} data: {dataToSave}");
                }
                else
                {
                    PlayerPrefs.SetString(key + saver.Key, empty ? "" : saver.DataSaved?.Invoke());
                }
            }

            PlayerPrefs.SetString(key + timestampSaveKey, Timestamp.GetStringTicks());
            PlayerPrefs.SetInt(key + privacyPolicySaveKey, _privacyPolicyAgreed ? 1 : 0);
            PlayerPrefs.SetInt(key + termsOfUseSaveKey, _termsOfUseAgreed ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Removes data about save with specified key.
        /// </summary>
        /// <param name="key">Specified key</param>
        public void RemoveSaveFrom(string key)
        {
            key = String.IsNullOrEmpty(key) ? defaultSaveKey : key;
            _saveKeysHolder.RemoveKey(key);
            foreach (var saver in savers)
            {
                PlayerPrefs.DeleteKey(key + saver.Key);
            }

            PlayerPrefs.DeleteKey(key + timestampSaveKey);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Finds save controller by specified key and applies it to current progress.
        /// </summary>
        /// <param name="key">Key of target save controller</param>
        /// <param name="force">If true, creates new save if can't find already existing</param>
        /// <param name="empty">If true, creates save from zero progress when force is true</param>
        /// <returns>True if load is exist. False otherwise</returns>
        public bool TryChooseSave(string key, bool force = false, bool empty = false)
        {
            if (_saveKeysHolder.IsSaveExist(key))
            {
                PlayerPrefs.SetString(CurrentSaveKey, key);
                PlayerPrefs.Save();
                return true;
            }

            if (force)
            {
                CreateSave(key, empty);
                PlayerPrefs.SetString(CurrentSaveKey, key);
                PlayerPrefs.Save();
                return true;
            }

            return false;
        }

        private void OnSaveNeeded()
        {
            _needSave = true;
        }

        private void Save()
        {
            SaveTo();
        }

        private void Load()
        {
            _privacyPolicyAgreed = PlayerPrefs.GetInt(_currentKey + privacyPolicySaveKey, 0) == 1;
            _termsOfUseAgreed = PlayerPrefs.GetInt(_currentKey + termsOfUseSaveKey, 0) == 1;
            var timestampKey = _currentKey + timestampSaveKey;
            if (!PlayerPrefs.HasKey(timestampKey))
            {
                _newGame = true;
            }

            var timestamp = PlayerPrefs.GetString(timestampKey, "0");
            var context = new LoadContext {playerOfflineTime = Timestamp.CalculateTimeDiff(timestamp)};
            foreach (var saver in savers)
            {
                if (verbose)
                {
                    var loadKey = _currentKey + saver.Key;
                    var data = PlayerPrefs.GetString(_currentKey + saver.Key, "");
                    saver.DataLoaded?.Invoke(data, context);
                    print($"[SaveService][Load] key: {loadKey} data: {data}");
                }
                else
                {
                    var data = PlayerPrefs.GetString(_currentKey + saver.Key, "");
                    saver.DataLoaded?.Invoke(data, context);
                }
            }

            foreach (var saver in savers)
            {
                saver.DataLoadFinished?.Invoke(context);
            }

            LoadFinished.Invoke(context);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Reset save data")]
#endif
        public static void ResetSaveData()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}