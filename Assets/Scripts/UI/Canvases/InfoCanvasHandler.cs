using Assets.Scripts.Enums;
using Assets.Scripts.EventBus;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Canvases
{
    public class InfoCanvasHandler : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private Button backButton;

        private void OnEnable()
        {
            backButton.onClick.AddListener(() =>
            {
                EventBus<Events.UIButtonClicked>.Raise(new Events.UIButtonClicked
                {
                    ButtonType = UiButtonType.Back
                });
            });
        }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
        }
    }
}