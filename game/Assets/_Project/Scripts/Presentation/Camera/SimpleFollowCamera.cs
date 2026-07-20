using UnityEngine;

namespace Aetherion.Presentation.Cameras
{
    public sealed class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -7f);
        [SerializeField] private float followLerp = 8f;
        [SerializeField] private float lookHeight = 1.4f;

        public void SetTarget(Transform t) => target = t;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = target.position + target.rotation * offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followLerp * Time.deltaTime));
            var lookAt = target.position + Vector3.up * lookHeight;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookAt - transform.position, Vector3.up),
                1f - Mathf.Exp(-followLerp * Time.deltaTime));
        }
    }
}
