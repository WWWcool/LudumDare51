using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.LoadingScreen
{
    public class RotateBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image slider;

        public void UpdateProgress(float progress)
        {
            slider.fillAmount = progress;
            progressText.text = $"{(100 * progress):####}%";
        }
    }
}