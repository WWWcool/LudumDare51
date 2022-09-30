using Tutorial.Models;
using UnityEditor;
using UnityEngine.UIElements;
using Utils;

namespace Tutorial.Editor
{
    [CustomEditor(typeof(TutorialRepository))]
    public class TutorialRepositoryEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            UIElementsEditorHelper.FillDefaultInspector(container, serializedObject);
            return container;
        }
    }
}