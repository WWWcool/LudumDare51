using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Core.LoadingScreen
{
    public class LoadingScreenBarSystem : MonoBehaviour
    {
        [SerializeField] private int gameSceneIndex = 1;
        [SerializeField] private RotateBar rotateBar;

        private float _loadProgress = 0;
        private Sequence _sequence;

        private async void Start()
        {
            _sequence = DOTween.Sequence();
            _sequence.Append(DOTween.To(() => _loadProgress, x => _loadProgress = x, 0.5f, 1f).
                OnUpdate(() => { rotateBar.UpdateProgress(_loadProgress); }
            ));
            await Load();
        }

        private async Task Load()
        {
            if (SceneManager.GetActiveScene().buildIndex == gameSceneIndex)
            {
                return;
            }
            
            // Debug.Log($"[NaninovelService][Start] Start init");
            // 1. Initialize Naninovel.
            // await RuntimeInitializer.InitializeAsync().Yield();
            // Debug.Log($"[NaninovelService][Start] Finish init");
            // Debug.Log($"[NaninovelService][Start] Start switchCommand");
            // // 2. Enter adventure mode.
            // var switchCommand = new SwitchToAdventureMode { ResetState = false };
            // await switchCommand.ExecuteAsync().Yield();;
            // Debug.Log($"[NaninovelService][Start] Finish switchCommand");
            var waitNextScene = SceneManager.LoadSceneAsync(gameSceneIndex);
            waitNextScene.completed += op => _sequence.Kill();
            while (!waitNextScene.isDone)
            {
                var sceneLoadProgress = _loadProgress + waitNextScene.progress / 2;
                rotateBar.UpdateProgress(sceneLoadProgress);
                await Task.Yield();
            }
        }
    }
}