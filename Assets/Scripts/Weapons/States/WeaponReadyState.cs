using Assets.Scripts.FSM;
using UnityEngine;

namespace Assets.Scripts.Weapons.States
{
    public class WeaponReadyState : State<WeaponHandler>
    {
        public override void Update()
        {
            if (Input.GetMouseButton(0))
            {
                if (Context.CurrentAmmo > 0)
                {
                    if (Time.time >= Context.LastFireTime + Context.Config.fireRate)
                        Context.Fire();
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                        Context.PlayEmptyFireAnimation();
                }
            }
        }
    }
}