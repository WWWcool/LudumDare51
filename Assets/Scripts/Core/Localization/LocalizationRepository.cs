using System;
using System.Collections.Generic;
using System.Linq;
using Core.Repositories;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Core.Localization
{
    [Serializable]
    public class LocalizeSaveData
    {
        public string fieldName;
        public string language;
        public string value;
    }

    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Repositories/Localization")]
    public class LocalizationRepository : AbstractRepository<List<LocalizeModel>>, ISerializationCallbackReceiver
    {
        public override string AssociatedSheet => "1GH1ioa_gDuyAH3eth6CF-oM6--QC9WOl3OsndbdbT2Q";
        public override string AssociatedWorksheet => "Data";

        [SerializeField] private List<LocalizeSaveData> localizeSave = new List<LocalizeSaveData>();
        [SerializeField] private List<string> languageList = new List<string>();

        public event Action LanguageChanged = () => { };

        private string _currentLanguage = "Default";

        public List<string> GetLanguagesList() => new List<string>(languageList);

        public void SetLanguage(string language)
        {
            if (languageList.Contains(language))
            {
                _currentLanguage = language;
                LanguageChanged.Invoke();
            }
        }

        public string GetTextInCurrentLocale(string fieldName)
        {
            return GetLocalizeText(fieldName, _currentLanguage);
        }
        
        public string GetLocalizeText(string fieldName, string language = "Default") 
        {
            if (!IsContainsField(fieldName, language))
            {
                Debug.LogWarning($"LocalizationDatabase is not contains ({fieldName} in {language} language)");
                return "NULL";
            }

            return data.GetBy(value => value.fieldName == fieldName).values[language];
        }
        
        public bool IsContainsField(string fieldName, string language = "Default")
        {
            var fields = data.Where(value => value.fieldName == fieldName).ToList();
            if (fields.Count > 0)
            {
                var field = fields.First();
                return field.values.ContainsKey(language);
            }

            return false;
        }

        public override void UpdateRepository(GstuSpreadSheet spreadSheet)
        {
            data = new List<LocalizeModel>();
            foreach (var cell in spreadSheet.columns["Key"])
            {
                if (cell.value == "Key")
                    continue;
                
                if (cell.value.StartsWith("//"))
                    continue;
                
                if (cell.value.Trim() == string.Empty)
                    continue;

                if (cell.value == "--")
                    break;

                var row = spreadSheet.rows[cell.value];
                var model = new LocalizeModel {fieldName = cell.value};
                foreach (var rowCell in row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    if (rowCell.columnId == "Key")
                        continue;

                    if (!model.values.ContainsKey(rowCell.columnId))
                    {
                        model.values.Add(rowCell.columnId, rowCell.value);
                    }
                }

                data.Add(model);
            }
        }

        public void OnBeforeSerialize()
        {
            localizeSave.Clear();
            languageList.Clear();
            foreach (var model in data)
            {
                foreach (var modelValue in model.values)
                {
                    localizeSave.Add(new LocalizeSaveData()
                    {
                        fieldName = model.fieldName,
                        language = modelValue.Key,
                        value = modelValue.Value
                    });

                    if (!languageList.Contains(modelValue.Key) && modelValue.Key != "Default")
                    {
                        languageList.Add(modelValue.Key);
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
            data = new List<LocalizeModel>();
            foreach (var save in localizeSave)
            {
                var model = data.GetBy(value => value.fieldName == save.fieldName);
                if (model != default)
                {
                    if (!model.values.ContainsKey(save.language))
                    {
                        model.values.Add(save.language, save.value);
                    }
                }
                else
                {
                    var newModel = new LocalizeModel {fieldName = save.fieldName};
                    newModel.values.Add(save.language, save.value);
                    data.Add(newModel);
                }
            }
        }
    }
}