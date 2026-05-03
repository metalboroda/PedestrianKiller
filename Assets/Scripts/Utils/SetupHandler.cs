using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class SetupHandler : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = -1;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
        }
    }
}