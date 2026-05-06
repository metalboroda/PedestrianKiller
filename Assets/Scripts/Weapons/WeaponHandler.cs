using Assets.Scripts.EventBus;
using Assets.Scripts.FSM;
using Assets.Scripts.ScriptableObjects.Weapons;
using Assets.Scripts.Weapons.States;
using DG.Tweening;
using UnityEngine;

namespace Assets.Scripts.Weapons
{
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Current Config")]
        [SerializeField] private WeaponConfigSo currentWeapon;

        [Header("References")]
        [SerializeField] private Transform pivot;
        [Space]
        [SerializeField] private ParticleSystem muzzleFlash;

        public WeaponConfigSo Config => currentWeapon;
        public int CurrentAmmo => _currentAmmo;
        public float LastFireTime { get; private set; }

        public FiniteStateMachine<WeaponHandler> WeaponFsm { get; private set; }
        public StateFactory<WeaponHandler> StateFactory { get; private set; }

        private int _currentAmmo;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
            WeaponFsm = new FiniteStateMachine<WeaponHandler>(this);
            StateFactory = new StateFactory<WeaponHandler>(this);
            _currentAmmo = currentWeapon.magazineSize;
        }

        private void Start()
        {
            WeaponFsm.Initialize(StateFactory.GetState<WeaponReadyState>());
            NotifyAmmoChanged();
        }

        private void Update()
        {
            if (!currentWeapon) return;
            
            WeaponFsm.CurrentState.Update();
            
            if (Input.GetKeyDown(KeyCode.R) && _currentAmmo < currentWeapon.magazineSize)
                RequestReload();
        }

        private void LateUpdate()
        {
            if (!currentWeapon) return;
            
            RotateWeapon();
        }

        public void Fire()
        {
            _currentAmmo--;
            LastFireTime = Time.time;

            ApplyRecoilAnimation();
            NotifyAmmoChanged();

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            EventBus<Events.FireRequested>.Raise(new Events.FireRequested
            {
                Config = currentWeapon,
                ShootingRay = ray
            });

            if (muzzleFlash) muzzleFlash.Play();
        }

        public void RequestReload()
        {
            if (WeaponFsm.CurrentState is not WeaponReloadingState)
            {
                WeaponFsm.ChangeState(StateFactory.GetState<WeaponReloadingState>());
            }
        }

        public void CompleteReload()
        {
            _currentAmmo = currentWeapon.magazineSize;
            NotifyAmmoChanged();
        }

        private void NotifyAmmoChanged()
        {
            EventBus<Events.AmmoChanged>.Raise(new Events.AmmoChanged
            {
                CurrentAmmo = _currentAmmo,
                MaxAmmo = currentWeapon.magazineSize
            });
        }

        private void RotateWeapon()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, currentWeapon.maxDistance))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(currentWeapon.maxDistance);

            Vector3 direction = (targetPoint - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentWeapon.rotationSpeed * Time.deltaTime);
            }
        }

        private void ApplyRecoilAnimation()
        {
            if (!pivot) return;
            
            pivot.DOKill();
            pivot.localPosition = Vector3.zero;
            pivot.localRotation = Quaternion.identity;

            pivot.DOPunchPosition(Vector3.back * currentWeapon.recoilStrength, currentWeapon.recoilDuration, currentWeapon.recoilVibrato);
            pivot.DOPunchRotation(new Vector3(-currentWeapon.recoilRotationStrength, 0, 0), currentWeapon.recoilDuration, currentWeapon.recoilVibrato);
        }
        
        public void PlayEmptyFireAnimation()
        {
            if (!pivot) return;
            
            pivot.DOKill();
            pivot.localPosition = Vector3.zero;
            pivot.localRotation = Quaternion.identity;
            
            pivot.DOPunchRotation(new Vector3(-1.5f, 0, 0), 0.1f, 15);
        }

        public void PlayReloadAnimation()
        {
            if (!pivot) return;
            
            pivot.DOKill();

            Sequence reloadSeq = DOTween.Sequence();
            float partTime = currentWeapon.reloadDuration / 3f;

            reloadSeq.Append(pivot.DOLocalMoveY(-0.15f, partTime).SetEase(Ease.OutQuad));
            reloadSeq.Join(pivot.DOLocalRotate(new Vector3(15, 5, 0), partTime));
            reloadSeq.Append(pivot.DOPunchPosition(Vector3.down * 0.03f, partTime, 2));
            reloadSeq.Append(pivot.DOLocalMoveY(0f, partTime).SetEase(Ease.InQuad));
            reloadSeq.Join(pivot.DOLocalRotate(Vector3.zero, partTime));
        }
    }
}