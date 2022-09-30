using System;
using Core.Conditions;
using Core.StateMachine;
using Tutorial.AnalyticsSignals;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Zenject;

namespace Tutorial
{
    public enum ETutorialState
    {
        None = 0,
        Wait = 1,
        Ready = 2,
        Active = 3,
        Cooldown = 4,
        Completed = 5
    }

    public class Tutorial
    {
        private TutorialDesc _desc;
        private TutorialStepSo _currentStepDesc;
        private ConditionBase _skipCondition;
        private ConditionBase _activationCondition;
        private ConditionBase _completionCondition;
        private IState<ETutorialState> _rootState;
        private int _currentStepIndex = 0;
        private bool _timerCompleted;
        private float _elapsedTime;

        public string Name => _desc.name;
        public bool LockSaves => _desc.lockSaves;
        public bool IsReady { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsActive { get; private set; }

        public event Action<Tutorial> StateChanged = tutorial => { };

        private bool _wasShown;

        private ConditionBuilder _conditionBuilder;
        private TutorialStep _tutorialStep;
        private SignalBus _signalBus;

        public void Init(TutorialDesc desc, ConditionBuilder conditionBuilder, TutorialStep tutorialStep, SignalBus signalBus)
        {
            _desc = desc;
            _conditionBuilder = conditionBuilder;
            _tutorialStep = tutorialStep;
            _signalBus = signalBus;
            InitStateMachine();
            _rootState.Verbose = true;
            ChangeState(ETutorialState.None);
        }

        public void Update(float deltaTime)
        {
            _rootState.Update(deltaTime);
        }

        public void ChangeState(ETutorialState state)
        {
            _rootState.ChangeState(state);
            StateChanged.Invoke(this);
        }

        private void InitStateMachine()
        {
            _rootState = new StateMachineBuilder<ETutorialState>()
                .State(ETutorialState.None)
                .Enter(state =>
                {
                    _currentStepDesc = null;
                    DisposeConditions();
                    DisposeTimer();
                })
                .Condition(() => _currentStepIndex >= _desc.steps.Count, state => ChangeState(ETutorialState.Completed))
                .Condition(() => _currentStepIndex < _desc.steps.Count, state =>
                {
                    _currentStepDesc = _desc.steps[_currentStepIndex];
                    if (_currentStepDesc.skipCondition != null && _skipCondition == null)
                    {
                        _skipCondition = _conditionBuilder.CreateCondition(_currentStepDesc.skipCondition);
                    }

                    if (_currentStepDesc.activationCondition != null && _activationCondition == null)
                    {
                        _activationCondition = _conditionBuilder.CreateCondition(_currentStepDesc.activationCondition);
                    }
                    ChangeState(ETutorialState.Wait);
                })
                .End()
                .State(ETutorialState.Wait)
                .Enter(state => { DisposeTimer(); })
                .Condition(() => CheckCondition(_skipCondition), state => PromoteStep())
                .Condition(() => CheckCondition(_activationCondition), state => ChangeState(ETutorialState.Ready))
                .End()
                .State(ETutorialState.Ready)
                .Enter(state => { IsReady = true; })
                .Exit(state => { IsReady = false; })
                .Condition(() => CheckCondition(_skipCondition), state => PromoteStep())
                .Condition(() => CheckCondition(_activationCondition, true), state => ChangeState(ETutorialState.Wait))
                .End()
                .State(ETutorialState.Active)
                .Enter(state =>
                {
                    if (_currentStepDesc != null)
                    {
                        IsActive = true;
                        if (_currentStepDesc.completionCondition != null)
                        {
                            _completionCondition =
                                _conditionBuilder.CreateCondition(_currentStepDesc.completionCondition);
                        }

                        ShowStep(_currentStepDesc);
                    }
                })
                .Exit(state =>
                {
                    IsActive = false;
                    if (_completionCondition != null)
                    {
                        _completionCondition.Dispose();
                        _completionCondition = null;
                    }
                })
                .Condition(() => CheckCondition(_completionCondition), state =>
                {
                    HideStep();
                    PromoteStep();
                })
                .Condition(() => CheckCondition(_activationCondition, true), state =>
                {
                    HideStep();
                    ChangeState(ETutorialState.Wait);
                })
                .End()
                .State(ETutorialState.Cooldown)
                .Enter(state => { DisposeTimer(); })
                .Update((state, deltaTime) =>
                {
                    _elapsedTime += deltaTime;
                    _timerCompleted = _elapsedTime >= _currentStepDesc?.delayOnStartInSec;
                })
                .Condition(() => CheckCondition(_skipCondition), state => ChangeState(ETutorialState.Completed))
                .Condition
                (
                    () => !CheckCondition(_skipCondition) && CheckCondition(_activationCondition),
                    state =>
                    {
                        DisposeTimer();
                        ChangeState(ETutorialState.Wait);
                    }
                )
                .Condition(
                    () => !CheckCondition(_skipCondition) && !CheckCondition(_activationCondition) && _timerCompleted,
                    state => ChangeState(ETutorialState.Ready))
                .End()
                .State(ETutorialState.Completed)
                .Enter(state =>
                {
                    IsCompleted = true;
                    DisposeConditions();
                })
                .Exit(state => { IsCompleted = false; })
                .End()
                .Build();
        }

        private bool CheckCondition(ConditionBase condition, bool inverse = false) =>
            condition != null && (inverse ? !condition.IsTrue : condition.IsTrue);

        private void PromoteStep()
        {
            _currentStepIndex += 1;
            _signalBus.Fire(new TutorialStepSignal(_currentStepIndex, _desc.name));
            Debug.Log($"[Tutorial][PromoteStep] {_desc.name} New index: {_currentStepIndex}");
            ChangeState(ETutorialState.None);
        }

        private void HideStep()
        {
            if (_wasShown)
            {
                _wasShown = false;
                _tutorialStep.Reset();
            }
        }

        private void ShowStep(TutorialStepSo desc)
        {
            if (!_tutorialStep.TryShow(desc))
            {
                ChangeState(ETutorialState.Wait);
            }
            else
            {
                _wasShown = true;
            }
        }

        private void DisposeTimer()
        {
            _elapsedTime = 0;
            _timerCompleted = false;
        }

        private void DisposeConditions()
        {
            if (_skipCondition != null)
            {
                _skipCondition.Dispose();
                _skipCondition = null;
            }

            if (_completionCondition != null)
            {
                _completionCondition.Dispose();
                _completionCondition = null;
            }

            if (_activationCondition != null)
            {
                _activationCondition.Dispose();
                _activationCondition = null;
            }
        }
    }
}