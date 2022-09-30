﻿using System;
using Plugins.WindowsManager;
using UnityEngine;

namespace Core.Windows
{
    public class WindowManager : WindowManagerBase
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private int _startSortingOrder;
        [SerializeField] private int _stepSortingOrder = 1;

        protected override int StartCanvasSortingOrder => _startSortingOrder;
        protected override int StepCanvasSortingOrder => _stepSortingOrder;

        public bool IsAnyWindowOpened()
        {
            foreach (var openedWindow in _openedWindows)
            {
                if (openedWindow.IsActive()) return true;
            }

            return false;
        }
        
        public bool TryShowAndGetWindow<T>(string windowId, out T component) where T : MonoBehaviour
        {
            return TryGetWindowComponent(windowId, ShowWindow(windowId), out component, popup => popup.CloseWindow());
        }
        
        public bool TryGetWindow<T>(string windowId, out T component) where T : MonoBehaviour
        {
            return TryGetWindowComponent(windowId, GetWindow(windowId), out component);
        }

        private bool TryGetWindowComponent<T>(string windowId, IWindow window, out T component, Action<PopupBase> failed = null) where T : MonoBehaviour
        {
            component = null;
            var popup = window as PopupBase;
            if (popup == null)
            {
                return false;
            }

            component = popup.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[WindowManager][TryGetWindowComponent] Get component: {typeof(T).Name} in window: {windowId} failed");
                failed?.Invoke(popup);
            }

            return component != null;
        }
    }
}