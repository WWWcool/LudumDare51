using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    [Serializable]
    public class ScrollRectMarker
    {
        public Vector2 offset;
        public Color outlineColor = Color.white;
        public Color solidColor;
    }

    public class AScrollRectMarkers<TEnum> : MonoBehaviour where TEnum : Enum
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private HashMap<TEnum, ScrollRectMarker> _markers = new HashMap<TEnum, ScrollRectMarker>();

        public HashMap<TEnum,ScrollRectMarker> GetMarkers => _markers;

        private void Reset()
        {
            ResetDependencies();
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (_scrollRect == null)
            {
                return;
            }

            foreach (var marker in _markers.Values)
            {
                var contentRect = _scrollRect.content;
                var screenRect = GetScreenSpaceMarkerRect(_scrollRect.viewport, contentRect);
                screenRect.x += marker.offset.x;
                screenRect.y += marker.offset.y;

                UnityEditor.Handles.DrawSolidRectangleWithOutline(screenRect, marker.solidColor, marker.outlineColor);
            }
#endif
        }

        public TEnum GetNameOfMarker(ScrollRectMarker marker)
        {
            if (!_markers.ContainsValue(marker))
            {
                return default(TEnum);
            }

            return _markers.GetKeyByValue(marker);
        }

        public void ScrollToMarker(TEnum markerName)
        {
            if (_scrollRect != null && _markers.ContainsKey(markerName))
            {
                var marker = _markers[markerName];

                var contentPosition = _scrollRect.viewport.position;
                contentPosition.x += marker.offset.x;
                contentPosition.y -= marker.offset.y;

                _scrollRect.velocity = Vector2.zero;
                _scrollRect.content.position = contentPosition;
            }
        }

        public void AddMarker(TEnum markerName, Vector2 offset, Color outlineColor = default, Color solidColor = default)
        {
            if (outlineColor == default)
            {
                outlineColor = Color.white;
            }

            if (solidColor == default)
            {
                solidColor = new Color(1f, 1f, 1f, 0);
            }

            var marker = new ScrollRectMarker()
            {
                offset = offset,
                solidColor = solidColor,
                outlineColor = outlineColor
            };

            _markers.Add(markerName, marker);
        }

        public void RemoveMarker(TEnum markerName)
        {
            if (_markers.ContainsKey(markerName))
            {
                _markers.Remove(markerName);
            }
        }

        public void ResetDependencies()
        {
            _scrollRect = GetComponent<ScrollRect>();
        }

        private Rect GetScreenSpaceMarkerRect(RectTransform viewport, RectTransform content)
        {
            var size = Vector2.Scale(viewport.rect.size, viewport.lossyScale);
            var position = content.position;
            position.y -= size.y;

            return new Rect(position, size);
        }
    }
}
