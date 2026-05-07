using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Character
{
    public class CharacterMaterialHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private SkinnedMeshRenderer hairRenderer;

        [Header("Random MAterials")]
        [SerializeField] private bool randomizeMaterial = true;
        [Space]
        [SerializeField] private Material[] bodyMaterials;
        [SerializeField] private Material[] hairMaterials;

        private void Awake()
        {
            int randomMaterial = Random.Range(0, bodyMaterials.Length);

            if (bodyRenderer && randomizeMaterial)
                bodyRenderer.material = bodyMaterials[randomMaterial];

            if (hairRenderer && randomizeMaterial)
                hairRenderer.material = hairMaterials[randomMaterial];
        }
    }
}