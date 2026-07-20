using Aetherion.Domain.Creatures;
using UnityEngine;

namespace Aetherion.Presentation.Creatures
{
    /// <summary>Placeholder wild creature view. Flat silhouette for C002.</summary>
    public sealed class WildCreatureView : MonoBehaviour
    {
        [SerializeField] private string defId = "C002";
        [SerializeField] private float sightRadius = 5f;

        public CreatureDefId DefId => CreatureDefId.Parse(defId);
        public float SightRadius => sightRadius;

        public void Configure(string creatureDefId, float radius = 5f)
        {
            defId = creatureDefId;
            sightRadius = radius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.8f, 0.4f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, sightRadius);
        }
    }
}
