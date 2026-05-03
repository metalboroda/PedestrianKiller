using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.GameManagement
{
    public class CameraManager : MonoBehaviour
    {
        private readonly List<GameObject> _cameras = new();

        private void Awake()
        {
            InitCameraList();
        }

        private void InitCameraList() { }

        private void SwitchCamera(GameObject newCamera)
        {
            if (!newCamera) return;

            foreach (GameObject cam in _cameras)
            {
                if (cam)
                {
                    cam.gameObject.SetActive(false);
                }
            }

            newCamera.gameObject.SetActive(true);
        }
    }
}