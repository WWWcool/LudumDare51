using System.IO;
using GoogleSheetsToUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Core.Repositories.Editor
{
    public abstract class AbstractRepositoryEditor : UnityEditor.Editor
    {
        private IAbstractRepository _database;

        private void OnEnable()
        {
            _database = (IAbstractRepository) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download", UpdateItems);
            UIElementsEditorHelper.AddButton(container, "Read from csv", ReadItems);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        public void UpdateItems() =>
            SpreadsheetManager.ReadPublicSpreadsheet(
                new GSTU_Search(_database.AssociatedSheet, _database.AssociatedWorksheet), UpdateMethod);

        private void UpdateMethod(GstuSpreadSheet ss)
        {
            _database.UpdateRepository(ss);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        
        private void ReadItems()
        {
            StreamReader reader = new StreamReader(GetPath(_database.AssociatedWorksheet));
            var ss = GstuSpreadSheet.CreateSheetFromString(reader.ReadToEnd());
            reader.Close();
            _database.UpdateRepository(ss);
            EditorUtility.SetDirty(target);
        }
        
        private string GetPath(string fileName)
        {
            return Application.dataPath + "/Resources/CsvData/"  + fileName + ".csv";
        }
    }
}