using System.Linq;
using System.Threading.Tasks;
using Core.Anchors;
using Core.Conditions;
using Core.Conditions.Commands;
using Core.Executor;
using Core.Executor.Commands;
using Core.Windows;
using Tutorial.Models;
using UnityEngine;
using Utils;
using Zenject;

namespace Tutorial.View
{
    public class TutorialStep : MonoBehaviour
    {
        [SerializeField] private FingerController fingerController;
        [SerializeField] private Transform fader;
        [SerializeField] private GameObject tapHandler;
        [SerializeField] private GameObject startLockLayer;
        [SerializeField] private UnityEventBool showSkipButton;
        [SerializeField] private GameObject lockLayer;
        [SortingLayer]
        [SerializeField] private string sortingLayerName;
        [SerializeField] private int sortingOrder;

        private bool IsVisible { get; set; }
        private Executor _executor = new Executor("TutorialStep");

        private TutorialStepSo _currentStep;
        private TutorialStepSo _scheduledStep;
        private bool _inProgress;
        private bool _submitOnStepEnd;
        private Anchor _anchor;

        private AnchorService _anchorService;
        private ConditionBuilder _conditionBuilder;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            AnchorService anchorService,
            ConditionBuilder conditionBuilder,
            WindowManager windowManager
        )
        {
            _anchorService = anchorService;
            _conditionBuilder = conditionBuilder;
            _windowManager = windowManager;
        }

        private void Awake()
        {
            ActivateTouchHandling(false);
        }

        private void LateUpdate()
        {
            if (_inProgress)
            {
                return;
            }

            if (_currentStep == null && IsVisible && _scheduledStep == null)
            {
                Hide();
            }
            else if (_scheduledStep != null)
            {
                _currentStep = _scheduledStep;
                _scheduledStep = null;
                _executor.Dispose();
                _executor = new Executor("TutorialStep");
                Show(_currentStep);
            }
        }

        private void OnDestroy()
        {
            _executor.Dispose();
        }

        public bool TryShow(TutorialStepSo desc)
        {
            if (_scheduledStep != null)
            {
                Debug.LogError($"[TutorialStep][OnShowHint] Another step has been already scheduled");
                return false;
            }

            _scheduledStep = desc;
            return true;
        }

        public void Reset()
        {
            _currentStep = null;
        }

        private void Show(TutorialStepSo desc)
        {
            _executor.AddCommand(new CmdLogMessage("[TutorialStep][Show] Tutorial appearance started"));
            _executor.AddCommand(new CmdCustom1<bool>(LockInput, true));
            _executor.AddCommand(new CmdCustom(FaderCleanUp));
            _executor.AddCommand(new CmdCustom(fingerController.Hide));
            if (desc.showFader)
            {
                _executor.AddCommand(new CmdCustom1<bool>(fader.gameObject.SetActive, true));
            }

            if (desc.activationCondition != null)
            {
                _executor.AddCommand(new CmdWaitCondition(desc.activationCondition, _conditionBuilder));
            }

            _executor.AddCommand(new CmdWaitTask1<float>(async delay => await Task.Delay((int)(delay * 1000f)), desc.delayOnStartInSec));
            _executor.AddCommand(new CmdCustom1<TutorialStepSo>(InitView, desc));
            var appearView = new CmdParallel();
            appearView.Enqueue(new CmdCustom1<TutorialStepSo>(UpdateFaderAndArrow, desc));
            if(desc.lockLayer)
            {
                _executor.AddCommand(new CmdCustom1<bool>(lockLayer.gameObject.SetActive, true));
            }
            _executor.AddCommand(appearView);
            _executor.AddCommand(new CmdCustom1<bool>(LockInput, false));
            _executor.AddCommand(new CmdCustom(() => _inProgress = false));
            _executor.AddCommand(new CmdCustom(() => IsVisible = true));
            _executor.AddCommand(new CmdLogMessage("[TutorialStep][Show] Tutorial appearance completed"));
        }

        private void Hide()
        {
            if (IsVisible)
            {
                _executor.AddCommand(new CmdLogMessage("[TutorialStep][Hide] Tutorial hide started"));
                _executor.AddCommand(new CmdCustom1<bool>(ActivateTouchHandling, false));
                _executor.AddCommand(new CmdCustom1<bool>(LockInput, false));
                _executor.AddCommand(new CmdCustom1<bool>(lockLayer.gameObject.SetActive, false));
                // Reset anchor sorting
                if (_anchor != null)
                {
                    _anchor.ResetSorting();
                    _anchor = null;
                }
                _executor.AddCommand(new CmdCustom(() => _inProgress = true));

                _executor.AddCommand(new CmdCustom1<bool>(SetDialogueHandlingActive, false));
                _executor.AddCommand(new CmdCustom(() => _inProgress = false));
                
                _executor.AddCommand(HideAuxUI());
                _executor.AddCommand(new CmdLogMessage("[TutorialStep][Hide] Tutorial hide completed"));
            }
        }

        private void InitView(TutorialStepSo desc)
        {
            showSkipButton.Invoke(desc.completionCondition.conditions.Any(x =>
                                      x.type == EConditionType.EventTrigger &&
                                      x.eventId == EEventTriggerId.ScreenTap) &&
                                  desc.showSkipButton);
        }

        private void FaderCleanUp()
        {
            fader.gameObject.SetActive(false);
        }

        private ICommand HideAuxUI()
        {
            return new CmdSequence(
                new CmdCustom(() => { IsVisible = false; }),
                new CmdCustom(FaderCleanUp),
                new CmdCustom(fingerController.StopSequence),
                new CmdCustom(fingerController.Hide)
            );
        }

        private void UpdateFaderAndArrow(TutorialStepSo desc)
        {
            fader.gameObject.SetActive(desc.showFader);
            if (desc.anchorType != EAnchorType.None)
            {
                UpdateAnchor(desc.anchorType, desc.anchorId, desc.arrowDir, desc.arrowOffset);
            }
        }

        private void UpdateAnchor(EAnchorType type, string id, EDirection arrowDir, Vector3 arrowOffset)
        {
            if (!_anchorService.TryGetAnchor(type, out _anchor, id))
            {
                Debug.LogWarning($"[TutorialStep][UpdateAnchor] Anchor: {type} with id: {id} not found");
                return;
            }

            lockLayer.gameObject.SetActive(true);
            _anchor.SetSorting(sortingLayerName, sortingOrder);
            fingerController.Show();
            fingerController.SetScreenPosition(_anchor.TargetPosition + arrowOffset);
            switch (arrowDir)
            {
                case EDirection.Up:
                    fingerController.Upside();
                    break;
                case EDirection.Left:
                    fingerController.LeftHand();
                    break;
                default:
                    fingerController.RightHand();
                    break;
            }

            fingerController.StartLoopTapAnimation();
        }

        private void LockInput(bool value)
        {
            startLockLayer.gameObject.SetActive(value);
            // _moveService.SetInputActive(!value);
        }

        private void ActivateTouchHandling(bool visible)
        {
            tapHandler.SetActive(visible);
        }

        private void SetDialogueHandlingActive(bool value)
        {
            // var inputManager = Engine.GetService<IInputManager>();
            // inputManager.ProcessInput = value;
        }
    }
}