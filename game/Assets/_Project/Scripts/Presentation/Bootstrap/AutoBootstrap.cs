using UnityEngine;

namespace Aetherion.Presentation.Bootstrap
{
    /// <summary>
    /// Ensures Boot composition root exists when entering Play from any scene.
    /// Attach to an empty object in Boot.unity, or rely on RuntimeInitialize.
    /// </summary>
    public static class AutoBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrap()
        {
            if (GameBootstrap.Instance != null)
                return;

            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }
    }
}
