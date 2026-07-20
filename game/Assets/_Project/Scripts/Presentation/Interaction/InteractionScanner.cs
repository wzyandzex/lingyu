using Aetherion.Presentation.Bootstrap;
using Aetherion.Presentation.UI;
using UnityEngine;

namespace Aetherion.Presentation.Interaction
{
    public sealed class InteractionScanner : MonoBehaviour
    {
        [SerializeField] private float scanRadius = 3f;
        [SerializeField] private HudController hud;

        private Interactable _current;

        public void Configure(HudController hudController)
        {
            hud = hudController;
        }

        private void Update()
        {
            _current = FindNearest();
            var canInteract = _current != null;
            if (GameBootstrap.Instance != null)
                GameBootstrap.Instance.SetInteractHint(canInteract);
            else if (hud != null)
                hud.SetInteractHint(canInteract ? "交互 [E]" : string.Empty);

            if (canInteract && Input.GetKeyDown(KeyCode.E))
            {
                if (GameBootstrap.Instance != null)
                    GameBootstrap.Instance.ShowInteractionText(_current.LocalizationKey);
                else
                    Debug.Log($"[Interact] {_current.LocalizationKey}");
            }
        }

        private Interactable FindNearest()
        {
            var cols = Physics.OverlapSphere(transform.position, scanRadius);
            Interactable best = null;
            var bestDist = float.MaxValue;
            foreach (var col in cols)
            {
                var interactable = col.GetComponentInParent<Interactable>();
                if (interactable == null) continue;
                var d = Vector3.Distance(transform.position, interactable.transform.position);
                if (d <= interactable.Radius && d < bestDist)
                {
                    best = interactable;
                    bestDist = d;
                }
            }

            return best;
        }
    }
}
