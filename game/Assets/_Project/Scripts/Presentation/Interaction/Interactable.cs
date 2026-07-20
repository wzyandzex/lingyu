using UnityEngine;

namespace Aetherion.Presentation.Interaction
{
    public sealed class Interactable : MonoBehaviour
    {
        [SerializeField] private string localizationKey = "interact.stone.intro";
        [SerializeField] private float radius = 2.2f;

        public string LocalizationKey => localizationKey;
        public float Radius => radius;

        public void Configure(string locKey, float interactRadius)
        {
            localizationKey = locKey;
            radius = interactRadius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.9f, 0.8f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
