using System.Collections;
using System.Collections.Generic;
using Core.Audio;
using Core.Localization;
using Core.Repositories;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using Tutorial.Models;
using UnityEditor;
using UnityEngine;
using Utils;
using Zenject;

namespace Installers
{
    public class RepositoryState
    {
        public bool needUpdate;
        public IAbstractRepository repository;

        public List<RepositoryState> deps;
        public bool HaveDeps => deps != null && deps.Count > 0;

        public RepositoryState(IAbstractRepository repository)
        {
            this.repository = repository;
        }

        public void AddDep(RepositoryState dep)
        {
            if (deps == null)
            {
                deps = new List<RepositoryState>();
            }

            deps.Add(dep);
        }
    }

    [CreateAssetMenu(fileName = "ScriptableInstaller", menuName = "Installers/ScriptableInstaller")]
    public class ScriptableInstaller : ScriptableObjectInstaller<ScriptableInstaller>, ICollectable<SoundSource>
    {
        [SerializeField] private LocalizationRepository localizationRepository;
        [SerializeField] private string folderToScan;
        [SerializeField] private List<SoundSource> soundSources = new List<SoundSource>();
        [SerializeField] private ResourceRepository resourceRepository;
        [SerializeField] private TutorialRepository tutorialRepository;

        private List<RepositoryState> _repositoryStates = new List<RepositoryState>();
        private bool _updateStarted;

        public override void InstallBindings()
        {
            Container.Bind<LocalizationRepository>().FromScriptableObject(localizationRepository).AsSingle();

            foreach (var soundSource in soundSources)
            {
                Container.QueueForInject(soundSource);
            }
            Container.Bind<ResourceRepository>().FromScriptableObject(resourceRepository).AsSingle();
            Container.Bind<TutorialRepository>().FromScriptableObject(tutorialRepository).AsSingle();

            GameSignalsInstaller.Install(Container);
        }

        public void SetData(List<SoundSource> data)
        {
            soundSources = data;
        }

        public string GetRootFolder()
        {
            return folderToScan;
        }

        public List<RepositoryState> CollectRepositoryStates()
        {
            var repositoryStates = new List<RepositoryState>();
            repositoryStates.Add(new RepositoryState(localizationRepository));
            return repositoryStates;
        }
        
        public void UpdateRepositories()
        {
#if UNITY_EDITOR
            if (!_updateStarted)
            {
                _updateStarted = true;
                Debug.Log($"[ScriptableInstaller][UpdateRepositories] Update started!");

                _repositoryStates = CollectRepositoryStates();

                foreach (var repositoryState in _repositoryStates)
                {
                    StartUpdateRepository(repositoryState);
                }

                EditorCoroutineRunner.StartCoroutine(FinishUpdate());
            }
#endif
        }

        public void ResetUpdate()
        {
#if UNITY_EDITOR
            _updateStarted = false;
#endif
        }

        private void StartUpdateRepository(RepositoryState repositoryState)
        {
            repositoryState.needUpdate = true;
            SpreadsheetManager.ReadPublicSpreadsheet(
                new GSTU_Search(
                    repositoryState.repository.AssociatedSheet,
                    repositoryState.repository.AssociatedWorksheet
                ),
                ss =>
                {
                    repositoryState.needUpdate = false;
                    repositoryState.repository.UpdateRepository(ss);
                    UpdateDependencies(repositoryState);
                    SaveAsset((ScriptableObject) repositoryState.repository);
                }
            );
        }

        private void SaveAsset(Object target)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
#endif
        }
        
        private void UpdateDependencies(RepositoryState repositoryState)
        {
            if (repositoryState.HaveDeps)
            {
                var deps = new List<RepositoryState>(repositoryState.deps);
                // remove all for scan below
                repositoryState.deps.Clear();
                foreach (var repositoryStateDep in deps)
                {
                    var contains = false;
                    foreach (var state in _repositoryStates)
                    {
                        if (state.HaveDeps && state.deps.Contains(repositoryStateDep))
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        _repositoryStates.Add(repositoryStateDep);
                        StartUpdateRepository(repositoryStateDep);
                    }
                }
            }
        }

        private IEnumerator FinishUpdate()
        {
            var needUpdate = true;
            while (needUpdate)
            {
                needUpdate = false;
                foreach (var repositoryState in _repositoryStates)
                {
                    if (repositoryState.needUpdate)
                    {
                        needUpdate = true;
                        break;
                    }
                }

                yield return null;
            }
            resourceRepository.DownloadImages();
            SaveAsset(resourceRepository);

            _updateStarted = false;

            Debug.Log($"[ScriptableInstaller][FinishUpdate] Update finished!");
            Debug.Log($"<color=lime>End test!</color>");
        }
    }
}