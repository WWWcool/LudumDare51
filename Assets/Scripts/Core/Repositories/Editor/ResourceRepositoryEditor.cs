using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Utils;

namespace Core.Repositories.Editor
{
    [CustomEditor(typeof(ResourceRepository))]
    public class ResourceRepositoryEditor : UnityEditor.Editor
    {
        private ResourceRepository _target;

        private void OnEnable()
        {
            _target = (ResourceRepository) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            UIElementsEditorHelper.AddButton(container, "Download", UpdateItems);
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);

            return container;
        }

        private void UpdateItems()
        {
            _target.DownloadImages();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}