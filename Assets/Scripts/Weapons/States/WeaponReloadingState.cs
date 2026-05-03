using Assets.Scripts.EventBus;
using Assets.Scripts.FSM;
using UnityEngine;

namespace Assets.Scripts.Weapons.States
{
    public class WeaponReloadingState : State<WeaponHandler>
    {
        private float _timer;

        public override void Enter()
        {
            _timer = 0;

            Context.PlayReloadAnimation();

            EventBus<Events.ReloadStarted>.Raise(new Events.ReloadStarted());
        }

        public override void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= Context.Config.reloadDuration)
            {
                Context.CompleteReload();
                Context.WeaponFsm.ChangeState(Context.StateFactory.GetState<WeaponReadyState>());
            }
        }
    }
}