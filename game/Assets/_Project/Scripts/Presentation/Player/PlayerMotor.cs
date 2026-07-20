using UnityEngine;

namespace Aetherion.Presentation.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float lookSensitivity = 0.12f;

        private CharacterController _controller;
        private float _yaw;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            var input = ReadMove();
            var look = ReadLook();

            _yaw += look.x * lookSensitivity;
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            var move = transform.right * input.x + transform.forward * input.y;
            if (move.sqrMagnitude > 1f)
                move.Normalize();

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            _verticalVelocity += gravity * Time.deltaTime;

            var velocity = move * moveSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
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
            // Legacy Input Manager — works in Unity 2022 Safe Mode recovery and without Input System package.
            var x = 0f;
            var y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return new Vector2(x, y);
        }

        private static Vector2 ReadLook()
        {
            if (!Input.GetMouseButton(1))
                return Vector2.zero;
            return new Vector2(Input.GetAxis("Mouse X") * 12f, Input.GetAxis("Mouse Y") * 12f);
        }
    }
}
