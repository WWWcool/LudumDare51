using System;
using System.Collections.Generic;
using Core.Conditions;
using Core.SaveLoad;
using Tutorial.AnalyticsSignals;
using Tutorial.Models;
using Tutorial.View;
using UnityEngine;
using Zenject;

namespace Tutorial
{
    [Serializable]
    public class TutorialData
    {
        public string id;
        public bool passed;
    }

    [Serializable]
    public class TutorialServiceData
    {
        public List<TutorialData> tutorials = new List<TutorialData>();
    }

    public class TutorialService : MonoBehaviour
    {
        [SerializeField] private TutorialRepository tutorialRepository;
        [SerializeField] private TutorialStep tutorialStep;
        [Space] [SerializeField] private Saver saver;

        public bool IsLocked { get; set; }
        public bool HasActiveTutorial => _currentTutorial != null;
        
        private TutorialServiceData _data;
        private List<Tutorial> _activeTutorials = new List<Tutorial>();
        private List<Tutorial> _removeTutorials = new List<Tutorial>();
        private Tutorial _currentTutorial;

        private bool _canStart;
        
        private ConditionBuilder _conditionBuilder;
        private SaveService _saveService;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(ConditionBuilder conditionBuilder, SaveService saveService, SignalBus signalBus)
        {
            _conditionBuilder = conditionBuilder;
            _saveService = saveService;
            _signalBus = signalBus;

            saver.DataLoaded += OnDataLoaded;
            saver.DataSaved += OnDataSaved;
        }

        private void Start ()
        {
            _canStart = true;
        }
        
        private void OnDestroy()
        {
            saver.DataLoaded -= OnDataLoaded;
            saver.DataSaved -= OnDataSaved;
        }

        private void Update()
        {
            if (_activeTutorials.Count > 0 && _canStart)
            {
                foreach (var activeTutorial in _activeTutorials)
                {
                    activeTutorial.Update(Time.deltaTime);
                }
                foreach (var tutorial in _removeTutorials)
                {
                    _activeTutorials.Remove(tutorial);
                }
                _removeTutorials.Clear();
            }
        }

        private void Init(TutorialServiceData data, LoadContext context)
        {
            _data = data;
            _activeTutorials.Clear();
            
            if (!tutorialRepository.tutorialEnabled)
            {
                return;
            }
            foreach (var tutorialDesc in tutorialRepository.tutorials)
            {
                var passed = false;
                var tutorialData = _data.tutorials.Find(data => tutorialDesc.name == data.id);
                if (tutorialData != null)
                {
                    passed = tutorialData.passed;
                }
                if (!passed)
                {
                    var tutorial = new Tutorial();
                    tutorial.Init(tutorialDesc, _conditionBuilder, tutorialStep, _signalBus);
                    tutorial.StateChanged += OnTutorialStateChanged;
                    _activeTutorials.Add(tutorial);
                }
            }
        }
        
        private void OnTutorialStateChanged(Tutorial tutorial) {
            if (_currentTutorial != null && _currentTutorial != tutorial)
            {
                return;
            }

            if ( _currentTutorial != null && _currentTutorial.IsCompleted )
            {
                var savesLocked = _currentTutorial.LockSaves;
                _data.tutorials.Add(new TutorialData{id = _currentTutorial.Name, passed = true});
                _removeTutorials.Add(_currentTutorial);
                
                _currentTutorial.StateChanged -= OnTutorialStateChanged;
                _currentTutorial = null; // Tutorial will be disposed with aspect removal. Completed tutorial could be repeated again by some conditions.
                if (savesLocked)
                {
                    _saveService.SaveLocked = false;
                    Debug.Log($"[TutorialService][OnTutorialStateChanged] Saves unlocked");
                    _saveService.ForceSave();
                }
                else
                {
                    saver.SaveNeeded.Invoke();
                }
            }
            if ( _currentTutorial == null && !IsLocked ) {
                foreach (var activeTutorial in _activeTutorials)
                {
                    if ( activeTutorial.IsReady ) {
                        _currentTutorial = activeTutorial;
                        _signalBus.Fire(new TutorialStepSignal(0, _currentTutorial.Name));
                        if (_currentTutorial.LockSaves)
                        {
                            _saveService.SaveLocked = true;
                            Debug.Log($"[TutorialService][OnTutorialStateChanged] Saves locked");
                        }
                        _currentTutorial.ChangeState(ETutorialState.Active);
                        break;
                    }
                }
            }
            else if ( _currentTutorial != null && _currentTutorial.IsReady ) {
                _currentTutorial.ChangeState(ETutorialState.Active);
            }
        }

        private void OnDataLoaded(string data, LoadContext context)
        {
            Init(saver.Unmarshal(data, new TutorialServiceData()), context);
        }

        private string OnDataSaved()
        {
            return saver.Marshal(_data);
        }
    }
}