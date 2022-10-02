using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SourceConfigPopup
{
    public class IntervalView : MonoBehaviour
    {
        [SerializeField] private RectTransform leftButtonPos;
        [SerializeField] private RectTransform rightButtonPos;
        [SerializeField] private RectTransform background;
        private RectTransform _rect;
        private Vector2 _startSize;
        private Vector2 _startRectPos;
        private float _startPos;
        private float _delta;

        private void Start()
        {
            _rect = transform as RectTransform;
        }

        public void LeftBeginDrag(BaseEventData data)
        {
            _startSize = background.sizeDelta;
            _startRectPos = background.anchoredPosition;
            _startPos = leftButtonPos.anchoredPosition.x;
            _delta = 0f;
        }

        public void LeftDrag(BaseEventData data)
        {
            var delta = (data as PointerEventData).delta.x;
            _delta += delta;
            leftButtonPos.anchoredPosition = new Vector2(_startPos + _delta, leftButtonPos.anchoredPosition.y);
            background.sizeDelta = new Vector2(_startSize.x - _delta, _startSize.y);
            background.anchoredPosition = new Vector2(_startRectPos.x + _delta / 2, background.anchoredPosition.y);
        }

        public void LeftEndDrag(BaseEventData data)
        {
            leftButtonPos.anchoredPosition = new Vector2(_startPos + _delta, leftButtonPos.anchoredPosition.y);
            background.sizeDelta = new Vector2(_startSize.x - _delta, _startSize.y);
        }

        public void RightBeginDrag(BaseEventData data)
        {
            _startSize = background.sizeDelta;
            _startRectPos = background.anchoredPosition;
            _startPos = rightButtonPos.anchoredPosition.x;
            _delta = 0f;
        }

        public void RightDrag(BaseEventData data)
        {
            var delta = (data as PointerEventData).delta.x;
            _delta += delta;
            rightButtonPos.anchoredPosition = new Vector2(_startPos + _delta, rightButtonPos.anchoredPosition.y);
            background.sizeDelta = new Vector2(_startSize.x + _delta, _startSize.y);
            background.anchoredPosition = new Vector2(_startRectPos.x + _delta / 2, background.anchoredPosition.y);
        }

        public void RightEndDrag(BaseEventData data)
        {
            rightButtonPos.anchoredPosition = new Vector2(_startPos + _delta, rightButtonPos.anchoredPosition.y);
            background.sizeDelta = new Vector2(_startSize.x + _delta, _startSize.y);
        }

        public void CenterBeginDrag(BaseEventData data)
        {
            _startRectPos = _rect.anchoredPosition;
            _delta = 0f;
        }

        public void CenterDrag(BaseEventData data)
        {
            var delta = (data as PointerEventData).delta.x;
            _delta += delta;
            _rect.anchoredPosition = new Vector2(_startRectPos.x + _delta / 2, _rect.anchoredPosition.y);
        }

        public void CenterEndDrag(BaseEventData data)
        {
        }
    }
}