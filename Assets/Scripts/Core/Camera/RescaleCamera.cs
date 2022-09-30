using UnityEngine;

namespace Core.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [DisallowMultipleComponent]
    public class RescaleCamera : MonoBehaviour
    {
        [SerializeField] private Vector2Int aspect;
        
        private int _screenSizeX = 0;
        private int _screenSizeY = 0;
        private UnityEngine.Camera _camera;

        private void Rescale()
        {
            if (Screen.width == _screenSizeX && Screen.height == _screenSizeY) return;

            var targetAspect = aspect.x / (float)aspect.y;
            var windowAspect = Screen.width / (float)Screen.height;
            var scaleHeight = windowAspect / targetAspect;
            UnityEngine.Camera camera = GetComponent<UnityEngine.Camera>();
 
            if (scaleHeight < 1.0f)
            {
                Rect rect = camera.rect;
 
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
 
                camera.rect = rect;
            }
            else // add pillarbox
            {
                float scalewidth = 1.0f / scaleHeight;
 
                Rect rect = camera.rect;
 
                rect.width = scalewidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scalewidth) / 2.0f;
                rect.y = 0;
 
                camera.rect = rect;
            }
 
            _screenSizeX = Screen.width;
            _screenSizeY = Screen.height;
        }
 
 
        // void OnPreCull()
        // {
        //     if (Application.isEditor) return;
        //     Rect wp = _camera.rect;
        //     Rect nr = new Rect(0, 0, 1, 1);
        //
        //     _camera.rect = nr;
        //     GL.Clear(true, true, Color.black);
        //
        //     _camera.rect = wp;
        //
        // }
 
        // Use this for initialization
        void Start () {
            _camera = GetComponent<UnityEngine.Camera>();
            Rescale();
        }
    }
}