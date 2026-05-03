using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI.Canvases
{
    public class LoadingCanvasHandler : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string gameSceneName;
        [Space]
        [SerializeField] private float gameSceneDelay = 0.01f;

        private void Awake()
        {
            StartCoroutine(DoLoadGameScene());
        }

        private IEnumerator DoLoadGameScene()
        {
            yield return new WaitForSeconds(gameSceneDelay);

            SceneManager.LoadScene(gameSceneName);
        }
    }
}