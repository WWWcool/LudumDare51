using Core.SaveLoad;
using Core.Windows;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Settings
{
    public class PrivacyPolicy : MonoBehaviour
    {
        [SerializeField] private PopupBase popupBase;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextAsset privacyPolicy;
        [SerializeField] private ScrollRect scrollRect;
        [SortingLayer] [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;
        [Space] [SerializeField] private bool isTermsOfUse;

        private TutorialService _tutorialService;
        private SaveService _saveService;

        [Inject]
        public void Construct(TutorialService tutorialService, SaveService saveService)
        {
            _tutorialService = tutorialService;
            _saveService = saveService;
            popupBase.Inited += OnInit;
            popupBase.Disposed += OnDisposed;
        }

        private void OnDestroy()
        {
            popupBase.Inited -= OnInit;
            popupBase.Disposed -= OnDisposed;
        }

        private void OnEnable() => contentText.text = privacyPolicy.text;

        private void Start()
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }

        private void OnInit()
        {
            if (_tutorialService.HasActiveTutorial)
            {
                popupBase.SetCanvasSorting(sortingLayerName, sortingOrder);
            }
        }

        private void OnDisposed(PopupBaseCloseType obj)
        {
            _saveService.SetAgreementState(isTermsOfUse);
        }
    }
}