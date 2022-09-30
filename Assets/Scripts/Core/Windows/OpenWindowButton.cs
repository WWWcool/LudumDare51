using UnityEngine;
using Zenject;

namespace Core.Windows
{
    public class OpenWindowButton : MonoBehaviour
    {
        [SerializeField] private EPopupType windowId;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            WindowManager windowManager
        )
        {
            _windowManager = windowManager;
        }

        public void OnClick()
        {
            _windowManager.ShowWindow(windowId.ToString());
        }
    }
}