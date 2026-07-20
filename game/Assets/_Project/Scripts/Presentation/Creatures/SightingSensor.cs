using Aetherion.Domain.Creatures;
using Aetherion.Presentation.Bootstrap;
using UnityEngine;

namespace Aetherion.Presentation.Creatures
{
    /// <summary>Distance-based sighting sensor on the player.</summary>
    public sealed class SightingSensor : MonoBehaviour
    {
        [SerializeField] private float pollInterval = 0.2f;
        private float _timer;

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = pollInterval;

            if (GameBootstrap.Instance == null || GameBootstrap.Session?.World == null)
                return;

            var wilds = Object.FindObjectsOfType<WildCreatureView>();
            var pos = transform.position;
            foreach (var wild in wilds)
            {
                if (wild == null) continue;
                var d = Vector3.Distance(pos, wild.transform.position);
                if (d <= wild.SightRadius)
                    GameBootstrap.Instance.TryRegisterSighting(wild.DefId);
            }
        }
    }
}
