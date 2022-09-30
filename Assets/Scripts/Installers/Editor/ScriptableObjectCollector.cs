using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils;

namespace Installers.Editor
{
    public class ScriptableObjectCollector<T> : UnityEditor.Editor where T : ScriptableObject
    {
        private ICollectable<T> _data;

        private void OnEnable()
        {
            _data = (ICollectable<T>) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download", SetCollectedItems);
            UIElementsEditorHelper.AddButton(container, "Update all Repositories", UpdateRepositories);
            UIElementsEditorHelper.AddButton(container, "Reset repositories update", ResetRepositoriesUpdate);
            UIElementsEditorHelper.AddButton(container, "Save all assets", SaveAllAssets);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        private void SetCollectedItems()
        {
            _data.SetData(ScriptableObjectHelpers.GetAllInstances<T>(_data.GetRootFolder()));
        }

        private void UpdateRepositories()
        {
            if(_data is ScriptableInstaller installer)
            {
                installer.UpdateRepositories();
            }
        }
        
        private void ResetRepositoriesUpdate()
        {
            if(_data is ScriptableInstaller installer)
            {
                installer.ResetUpdate();
            }
        }

        private void SaveAllAssets()
        {
            AssetDatabase.SaveAssets();
        }
    }
}