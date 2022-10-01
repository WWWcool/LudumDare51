using Core.Windows;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class ExamplePopup : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private TextMeshProUGUI text;
        
        [Inject]
        public void Construct()
        {
            popupBase.Inited += OnInit;
            popupBase.Disposed += OnDisposed;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= OnInit;
            popupBase.Disposed -= OnDisposed;
        }

        public void OnOkClick()
        {
            popupBase.CloseWindowWithCloseButton();
        }
        
        private void OnInit()
        {
            text.text = $"Some text + object id: {gameObject.GetInstanceID().ToString()}";
        }

        private void OnDisposed(PopupBaseCloseType obj)
        {
            if(obj == PopupBaseCloseType.Close)
            {
                Debug.Log($"Log on popup close + again object id: {gameObject.GetInstanceID().ToString()}");
            }
        }
    }
}