using System.Collections;
using Lean.Pool;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.VFX
{
    public class DecalHandler : MonoBehaviour, IPoolable
    {
        [Header("Destroy Settings")]
        [SerializeField] private float destroyDelay = 5f;
        [SerializeField] private float destroySpeed = 1f;

        [Header("Scale Settings")]
        [SerializeField] private float minScale = 0.4f;
        [SerializeField] private float maxScale = 0.2f;

        [Header("References")]
        [SerializeField] private DecalProjector decalProjector;
        [Space]
        [SerializeField] private Transform pivot;

        private Coroutine _fadeRoutine;

        public void OnSpawn()
        {
            pivot.transform.eulerAngles = new Vector3(
                pivot.transform.eulerAngles.x,
                pivot.transform.eulerAngles.y,
                Random.Range(0, 360));

            pivot.localScale = Vector3.one * Random.Range(minScale, maxScale);

            decalProjector.fadeFactor = 1f;

            _fadeRoutine = StartCoroutine(FadeAndDespawn());
        }

        private IEnumerator FadeAndDespawn()
        {
            yield return new WaitForSeconds(destroyDelay);

            while (decalProjector.fadeFactor > 0)
            {
                decalProjector.fadeFactor -= destroySpeed * Time.deltaTime;
                yield return null;
            }

            LeanPool.Despawn(gameObject);
        }

        public void OnDespawn()
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);

                _fadeRoutine = null;
            }
        }
    }
}