using System.Collections;
using RootMotion.Dynamics;
using UnityEngine;

namespace Assets.Scripts.Character
{
    public class CharacterRagdollHandler : MonoBehaviour
    {
        [Header("Injured Settings")]
        [SerializeField] private float injuredPinWeight;
        [SerializeField] private float injuredPinSpeed = 1f;
        [SerializeField] private float injuredMuscleWeight = 0.1f;
        [SerializeField] private float injuredMuscleSpeed = 1f;

        [Header("Dead Settings")]
        [SerializeField] private float deadPinWeight;
        [SerializeField] private float deadPinSpeed = 0.5f;
        [SerializeField] private float deadMuscleWeight = 0.01f;
        [SerializeField] private float deadMuscleSpeed = 2.5f;

        [Header("References")]
        [SerializeField] private PuppetMaster puppetMaster;

        private Coroutine _activeTransition;

        public void EnableInjuredRagdoll()
        {
            StopActiveTransition();
            _activeTransition = StartCoroutine(DoSetPuppetInjured());
        }

        public void EnableDeadRagdoll()
        {
            StopActiveTransition();
            _activeTransition = StartCoroutine(DoSetPuppetDead());
        }

        private void StopActiveTransition()
        {
            if (_activeTransition != null)
                StopCoroutine(_activeTransition);
        }

        private IEnumerator DoSetPuppetInjured()
        {
            puppetMaster.mode = PuppetMaster.Mode.Active;

            while (!Mathf.Approximately(puppetMaster.pinWeight, injuredPinWeight))
            {
                puppetMaster.pinWeight = Mathf.MoveTowards(
                    puppetMaster.pinWeight, injuredPinWeight, injuredPinSpeed * Time.deltaTime);

                yield return null;
            }

            while (!Mathf.Approximately(puppetMaster.muscleWeight, injuredMuscleWeight))
            {
                puppetMaster.muscleWeight = Mathf.MoveTowards(
                    puppetMaster.muscleWeight, injuredMuscleWeight, injuredMuscleSpeed * Time.deltaTime);

                yield return null;
            }
        }

        private IEnumerator DoSetPuppetDead()
        {
            puppetMaster.mode = PuppetMaster.Mode.Active;

            while (!Mathf.Approximately(puppetMaster.pinWeight, deadPinWeight))
            {
                puppetMaster.pinWeight = Mathf.MoveTowards(
                    puppetMaster.pinWeight, deadPinWeight, deadPinSpeed * Time.deltaTime);

                yield return null;
            }

            while (!Mathf.Approximately(puppetMaster.muscleWeight, deadMuscleWeight))
            {
                puppetMaster.muscleWeight = Mathf.MoveTowards(
                    puppetMaster.muscleWeight, deadMuscleWeight, deadMuscleSpeed * Time.deltaTime);

                yield return null;
            }
        }
    }
}