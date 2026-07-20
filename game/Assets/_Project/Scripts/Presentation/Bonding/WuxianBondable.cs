using Aetherion.Domain.Bonding;
using Aetherion.Domain.Creatures;
using Aetherion.Presentation.Bootstrap;
using Aetherion.Presentation.Creatures;
using UnityEngine;

namespace Aetherion.Presentation.Bonding
{
    /// <summary>
    /// Single C001 entity: sighting + bond interact + Walk/Hold presentation.
    /// </summary>
    public sealed class WuxianBondable : MonoBehaviour
    {
        public const string InteractKey = "interact.bond.try_resonate";

        [SerializeField] private float sightRadius = 6f;
        [SerializeField] private float walkAmplitude = 0.55f;
        [SerializeField] private float walkSpeed = 1.1f;

        private Vector3 _home;
        private Vector3 _baseScale;
        private WildCreatureView _view;

        public CreatureDefId DefId => CreatureDefId.Parse("C001");

        private void Awake()
        {
            _home = transform.position;
            _baseScale = transform.localScale;
            _view = GetComponent<WildCreatureView>();
            if (_view == null)
                _view = gameObject.AddComponent<WildCreatureView>();
            _view.Configure("C001", sightRadius);
        }

        private void Update()
        {
            var session = GameBootstrap.Session?.World?.ActiveBonding;
            if (session == null || session.IsTerminal)
            {
                // Idle bob when not bonding
                var t = Time.time * 0.8f;
                transform.position = _home + new Vector3(Mathf.Sin(t) * 0.15f, 0f, Mathf.Cos(t * 0.7f) * 0.1f);
                return;
            }

            if (session.State != BondingState.Testing && session.State != BondingState.ResonanceWindow)
            {
                transform.position = _home;
                return;
            }

            // Public beat signal for Testing
            if (session.Phase == BondingPhase.Walk)
            {
                var t = Time.time * walkSpeed;
                transform.position = _home + transform.forward * (Mathf.Sin(t) * walkAmplitude);
                transform.localScale = _baseScale * (1f + 0.05f * Mathf.Abs(Mathf.Sin(t * 2f)));
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _home, 1f - Mathf.Exp(-8f * Time.deltaTime));
                transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, 1f - Mathf.Exp(-6f * Time.deltaTime));
            }
        }

        public void SetBondedVisual()
        {
            // Soft glow scale after success; stay in world as companion marker (simple)
            transform.localScale = _baseScale * 1.05f;
            var r = GetComponent<Renderer>();
            if (r != null)
                r.material.color = new Color(0.75f, 0.92f, 0.88f);
        }
    }
}
