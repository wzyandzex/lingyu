using Aetherion.Domain.Bonding;
using Aetherion.Domain.Creatures;
using Aetherion.Presentation.Bootstrap;
using Aetherion.Presentation.Creatures;
using UnityEngine;

namespace Aetherion.Presentation.Bonding
{
    /// <summary>
    /// Clear Walk/Hold telegraph: MOVE sideways on Walk, FREEZE + slightly crouch on Hold.
    /// </summary>
    public sealed class WuxianBondable : MonoBehaviour
    {
        public const string InteractKey = "interact.bond.try_resonate";

        [SerializeField] private float sightRadius = 6f;
        [SerializeField] private float walkDistance = 1.4f;

        private Vector3 _home;
        private Vector3 _baseScale;
        private Vector3 _walkTarget;
        private Renderer _renderer;
        private Color _baseColor;

        public CreatureDefId DefId => CreatureDefId.Parse("C001");

        private void Awake()
        {
            _home = transform.position;
            _baseScale = transform.localScale;
            _walkTarget = _home + transform.right * walkDistance;
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
                _baseColor = _renderer.material.color;

            var view = GetComponent<WildCreatureView>();
            if (view == null)
                view = gameObject.AddComponent<WildCreatureView>();
            view.Configure("C001", sightRadius);
        }

        private void Update()
        {
            var session = GameBootstrap.Session?.World?.ActiveBonding;
            if (session == null || session.IsTerminal)
            {
                // Gentle idle
                var t = Time.time * 0.7f;
                transform.position = _home + new Vector3(Mathf.Sin(t) * 0.12f, 0f, 0f);
                transform.localScale = _baseScale;
                SetColor(_baseColor);
                return;
            }

            if (session.State != BondingState.Testing && session.State != BondingState.ResonanceWindow)
            {
                transform.position = Vector3.Lerp(transform.position, _home, 1f - Mathf.Exp(-6f * Time.deltaTime));
                transform.localScale = _baseScale;
                SetColor(_baseColor);
                return;
            }

            if (session.Phase == BondingPhase.Walk)
            {
                // Slide between home and walkTarget so "moving" is obvious
                var u = 1f - session.PhaseRemaining01;
                var ping = u < 0.5f ? u * 2f : (1f - u) * 2f;
                transform.position = Vector3.Lerp(_home, _walkTarget, ping);
                transform.localScale = _baseScale;
                SetColor(new Color(0.55f, 0.95f, 0.75f)); // brighter green-teal = MOVE
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _home, 1f - Mathf.Exp(-10f * Time.deltaTime));
                var holdScale = Vector3.Scale(_baseScale, new Vector3(1.05f, 0.85f, 1.05f));
                transform.localScale = Vector3.Lerp(transform.localScale, holdScale,
                    1f - Mathf.Exp(-8f * Time.deltaTime));
                SetColor(new Color(0.85f, 0.88f, 0.95f)); // pale = STOP
            }
        }

        public void SetBondedVisual()
        {
            transform.localScale = _baseScale * 1.08f;
            SetColor(new Color(0.7f, 0.95f, 0.9f));
        }

        private void SetColor(Color c)
        {
            if (_renderer != null)
                _renderer.material.color = c;
        }
    }
}
