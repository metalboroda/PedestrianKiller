using UnityEngine;

namespace Assets.Scripts.Weapons
{
    public class WeaponRotation : MonoBehaviour
    {
        [Header("Sensitivity Settings")]
        [SerializeField] private float sensitivityX = 2f;
        [SerializeField] private float sensitivityY = 2f;

        [Header("Vertical Clamp (Pitch)")]
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Horizontal Clamp (Yaw)")]
        [SerializeField] private bool useHorizontalClamp = true;
        [SerializeField] private float minYaw = -60f;
        [SerializeField] private float maxYaw = 60f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.05f;

        private float _rotationX;
        private float _rotationY;

        private float _currentRotationX;
        private float _currentRotationY;

        private float _velocityX;
        private float _velocityY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _rotationX = 0f;
            _rotationY = 0f;
        }

        private void Update()
        {
            HandleInput();
            ApplyRotation();
        }

        private void HandleInput()
        {
            _rotationY += Input.GetAxisRaw("Mouse X") * sensitivityX;
            _rotationX -= Input.GetAxisRaw("Mouse Y") * sensitivityY;

            _rotationX = Mathf.Clamp(_rotationX, minPitch, maxPitch);

            if (useHorizontalClamp)
                _rotationY = Mathf.Clamp(_rotationY, minYaw, maxYaw);
        }

        private void ApplyRotation()
        {
            _currentRotationX = Mathf.SmoothDampAngle(_currentRotationX, _rotationX, ref _velocityX, smoothTime);
            _currentRotationY = Mathf.SmoothDampAngle(_currentRotationY, _rotationY, ref _velocityY, smoothTime);

            transform.localRotation = Quaternion.Euler(_currentRotationX, _currentRotationY, 0f);
        }
    }
}