using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Aetherion.Presentation.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float lookSensitivity = 0.15f;
        [SerializeField] private Transform cameraPivot;

        private CharacterController _controller;
        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;
        private Transform _cameraTransform;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Start()
        {
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            var input = ReadMove();
            var look = ReadLook();

            _yaw += look.x * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch - look.y * lookSensitivity, -35f, 55f);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            var move = transform.right * input.x + transform.forward * input.y;
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;

            var velocity = move * moveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);

            if (_cameraTransform != null)
            {
                var targetPos = transform.position + Vector3.up * 1.6f - transform.forward * 6f + Vector3.up * 1.2f;
                // Camera follow handled by SimpleFollowCamera; only store pitch for future use.
                if (cameraPivot != null)
                    cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        public void Teleport(Vector3 position, float yaw)
        {
            _controller.enabled = false;
            transform.position = position;
            _yaw = yaw;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _controller.enabled = true;
        }

        private static Vector2 ReadMove()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return Vector2.zero;
            var x = 0f;
            var y = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            return new Vector2(x, y);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private static Vector2 ReadLook()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null) return Vector2.zero;
            // Only when right mouse held to avoid fighting UI in editor.
            if (!mouse.rightButton.isPressed) return Vector2.zero;
            return mouse.delta.ReadValue();
#else
            if (!Input.GetMouseButton(1)) return Vector2.zero;
            return new Vector2(Input.GetAxis("Mouse X") * 10f, Input.GetAxis("Mouse Y") * 10f);
#endif
        }
    }
}
