using Assets.Scripts.Character;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class ColliderHelper : MonoBehaviour
    {
        [ContextMenu("Remove All Colliders (Including Children)")]
        private void RemoveAllColliders()
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>(true);

            if (allColliders.Length == 0)
            {
                Debug.Log("Колайдерів не знайдено.");
                return;
            }

            int count = allColliders.Length;

            for (int i = count - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(allColliders[i]);

            Debug.Log($"Успішно видалено {count} колайдерів.");
        }

        [ContextMenu("Set All Colliders to Trigger")]
        private void SetAllCollidersToTrigger()
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>(true);

            if (allColliders.Length == 0)
            {
                Debug.Log("Колайдерів не знайдено.");
                return;
            }
            
            Undo.RecordObjects(allColliders, "Set Colliders to Trigger");

            foreach (var col in allColliders)
            {
                col.isTrigger = true;
            }

            Debug.Log($"Успішно встановлено IsTrigger для {allColliders.Length} колайдерів.");
        }

        [ContextMenu("Setup DecalMeshRef on Colliders")]
        private void SetupDecalMeshRef()
        {
            int count = ReplaceComponentOnColliders<DecalMeshRef>();
            
            Debug.Log($"Обробка завершена: {count} об'єктів отримали DecalMeshRef.");
        }

        [ContextMenu("Setup DamageableBodyPart on Colliders")]
        private void SetupDamageableBodyPart()
        {
            int count = ReplaceComponentOnColliders<DamageableBodyPart>();
            
            Debug.Log($"Обробка завершена: {count} об'єктів отримали DamageableBodyPart.");
        }

        private int ReplaceComponentOnColliders<T>() where T : Component
        {
            Collider[] allColliders = GetComponentsInChildren<Collider>(true);
            int affectedCount = 0;

            foreach (var col in allColliders)
            {
                GameObject targetGo = col.gameObject;

                T existingComponent = targetGo.GetComponent<T>();
                
                if (existingComponent)
                {
                    Undo.DestroyObjectImmediate(existingComponent);
                }

                Undo.AddComponent<T>(targetGo);
                
                affectedCount++;
            }

            return affectedCount;
        }
    }
}