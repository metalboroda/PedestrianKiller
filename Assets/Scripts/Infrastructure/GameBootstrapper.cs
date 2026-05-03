using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public class GameBootstrapper : MonoBehaviour
    {
        public static GameBootstrapper Instance { get; private set; }

        private void Awake()
        {
            InitSingleton();
        }

        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;

                DontDestroyOnLoad(gameObject);
            }
        }
    }
}