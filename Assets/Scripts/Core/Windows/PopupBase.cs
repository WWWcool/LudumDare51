using System;
using UnityEngine;
using Plugins.WindowsManager;
using Utils;
using Zenject;

namespace Core.Windows
{
    public enum PopupBaseCloseType
    {
        None = 0,
        Close = 1,
        Screen = 2,
    }
    
    public class PopupBase : Window<PopupBase>
    {
        [SerializeField] private EPopupType windowId;
        [SerializeField] private Animator animator;
        [SerializeField] private string hideKey;

        public override string WindowId => windowId.ToString();
        public event Action Inited = () => { };
        
        public Action BeforeCloseWindow = () => {};
        public event Action<object[]> ShowArgsGot = args => {};
        public event Action<PopupBaseCloseType> Disposed = closeType => { };

        private PopupBaseCloseType _closeType = PopupBaseCloseType.None;
        private PopupAnimationController _popupAnimation;
        
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            WindowManager windowManager,
            UnityEngine.Camera worldCamera
        )
        {
            _windowManager = windowManager;
            Canvas.worldCamera = worldCamera;
        }
        
        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape) && _windowManager.IsTopWindow(WindowId))
            {
                CloseWindow();
            }
        }

        public void CloseWindow()
        {
            BeforeCloseWindow.Invoke();
            CloseWindow(PopupBaseCloseType.None);
        }
        
        public void CloseWindowWithScreenTap()
        {
            CloseWindow(PopupBaseCloseType.Screen);
        }
        
        public void CloseWindowWithCloseButton()
        {
            CloseWindow(PopupBaseCloseType.Close);
        }

        public void CloseWindow(PopupBaseCloseType closeType)
        {
            _closeType = closeType;
            if (animator != null)
            {
                animator.SetBool(hideKey, true);
                foreach (var hideable in GetComponentsInChildren<IHideable>())
                {
                    hideable.Hide();
                }
            }
            else
            {
                Close();
            }
        }

        public override void Activate(bool immediately = false)
        {
            if (animator != null)
            {
                _popupAnimation = animator.GetBehaviour<PopupAnimationController>();
                _popupAnimation.HideFinished += OnHideFinished;
            }
            ActivatableState = ActivatableState.Active;
            Canvas.ForceUpdateCanvases();
            Inited.Invoke();
            gameObject.SetActive(true);
        }

        public override void Deactivate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Inactive;
            Disposed.Invoke(_closeType);
            gameObject.SetActive(false);
        }

        public override void SetArgs(object[] args)
        {
            if(args != null)
            {
                ShowArgsGot.Invoke(args);
            }
        }

        public void SetCanvasSorting(string layerName, int order)
        {
            Canvas.overrideSorting = true;
            Canvas.sortingLayerName = layerName;
            Canvas.sortingOrder = order;
        }
        
        private void OnHideFinished()
        {
            Close();
        }

        private void Close()
        {
            if (ActivatableState != ActivatableState.Inactive)
            {
                _windowManager.CloseAll(WindowId);
            }
        }
    }
}