using System;
using Tutorial;
using UnityEngine;
using Zenject;

namespace Core.Anchors
{
    public class Anchor : MonoBehaviour
    {
        [SerializeField] private EAnchorType anchorType;
        [SerializeField] private string anchorId;
        [SerializeField] private Transform anchorTarget;
        [SerializeField] private Canvas canvas;

        public Vector3 TargetPosition => anchorTarget == null ? Vector3.zero : anchorTarget.position;
        public event Action Clicked = () => {};
        public event Action<bool> StateChanged = enabled => {};
        public string Id => _id;

        private bool _add;
        private string _id;
        private string _sortingLayerName;
        private int _sortingOrder;
        
        private AnchorService _anchorService;
        
        [Inject]
        public void Construct(AnchorService anchorService)
        {
            _anchorService = anchorService;
            _id = String.IsNullOrEmpty(anchorId) ? null : anchorId;
            if (gameObject.activeSelf)
            {
                _anchorService.AddAnchor(anchorType, this);
            }
        }

        private void OnEnable()
        {
            if(_anchorService != null)
            {
                _anchorService.AddAnchor(anchorType, this);
            }
            StateChanged.Invoke(true);
        }
        
        private void OnDisable()
        {
            ResetSorting();
            StateChanged.Invoke(false);
            _anchorService.RemoveAnchor(anchorType, this);
        }

        private void OnDestroy()
        {
            _anchorService.RemoveAnchor(anchorType, this);
        }

        public void Reinit(string id)
        {
            _anchorService.RemoveAnchor(anchorType, this);
            _id = id;
            _anchorService.AddAnchor(anchorType, this);
            Debug.Log($"[Anchor][Reinit] Readded anchor {anchorType} id: {Id}");
        }

        public void OnClick()
        {
            Clicked.Invoke();
        }

        public void SetSorting(string sortingLayerName, int sortingOrder)
        {
            if(canvas != null)
            {
                // Debug.Log($"[Anchor][SetSorting] Layer {sortingLayerName} order: {sortingOrder}");
                _sortingLayerName = canvas.sortingLayerName;
                _sortingOrder = canvas.sortingOrder;
                canvas.overrideSorting = true;
                canvas.sortingLayerName = sortingLayerName;
                canvas.sortingOrder = sortingOrder;
            }
        }
        
        public void ResetSorting()
        {
            if(canvas != null)
            {
                // Debug.Log($"[Anchor][ResetSorting] Layer {_sortingLayerName} order: {_sortingOrder}");
                canvas.overrideSorting = false;
                canvas.sortingLayerName = _sortingLayerName;
                canvas.sortingOrder = _sortingOrder;
            }
        }
    }
}