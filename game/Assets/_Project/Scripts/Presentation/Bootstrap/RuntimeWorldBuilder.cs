using Aetherion.Presentation.Camera;
using Aetherion.Presentation.Interaction;
using Aetherion.Presentation.Player;
using Aetherion.Presentation.UI;
using UnityEngine;

namespace Aetherion.Presentation.Bootstrap
{
    /// <summary>Builds a minimal R01 greybox when no authored scene is available.</summary>
    public static class RuntimeWorldBuilder
    {
        public static void BuildR01Shell()
        {
            if (GameObject.Find("R01_RuntimeRoot") != null)
                return;

            var root = new GameObject("R01_RuntimeRoot");

            // Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "PH_Ground";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localScale = new Vector3(4f, 1f, 4f);
            var groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
                groundRenderer.material.color = new Color(0.25f, 0.42f, 0.28f);

            // Light
            var lightGo = new GameObject("Directional Light");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.98f, 0.92f);
            light.intensity = 1.1f;

            // Fog-ish ambient
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.55f, 0.65f, 0.62f);
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.55f, 0.6f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.42f, 0.32f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.18f, 0.12f);

            // Player
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.SetParent(root.transform, false);
            player.transform.position = new Vector3(0f, 1f, 0f);
            Object.Destroy(player.GetComponent<Collider>());
            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.up;
            player.AddComponent<PlayerMotor>();

            // Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.58f, 0.62f);
            cam.nearClipPlane = 0.1f;
            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<SimpleFollowCamera>();
            follow.SetTarget(player.transform);

            // Interact stone (POI-R01-02 style)
            var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "InteractStone";
            stone.transform.SetParent(root.transform, false);
            stone.transform.position = new Vector3(4f, 0.75f, 3f);
            stone.transform.localScale = new Vector3(0.8f, 1.5f, 0.4f);
            var stoneRenderer = stone.GetComponent<Renderer>();
            if (stoneRenderer != null)
                stoneRenderer.material.color = new Color(0.55f, 0.52f, 0.45f);
            var interactable = stone.AddComponent<Interactable>();
            interactable.Configure("interact.stone.intro", 2.2f);

            // HUD
            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(root.transform, false);
            var hud = hudGo.AddComponent<HudController>();
            if (GameBootstrap.Instance != null)
                GameBootstrap.Instance.RegisterHud(hud);

            // Interaction scanner on player
            var scanner = player.AddComponent<InteractionScanner>();
            scanner.Configure(hud);

            Debug.Log("[World] Runtime R01 shell built (placeholder greybox).");
        }
    }
}
