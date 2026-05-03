using UnityEngine;

namespace Assets.Scripts.VFX
{
    public class BloodPuddleFollower : MonoBehaviour
    {
        private Transform _targetBone;
        private Vector3 _initialForward;
        
        private LayerMask _groundLayer;
        private float _castDistance;
        private float _offsetFromGround;
        private float _offsetY;
        
        public void Init(Transform targetBone, LayerMask groundLayer, float castDistance, float offsetFromGround, float offsetY)
        {
            _targetBone = targetBone;
            _groundLayer = groundLayer;
            _castDistance = castDistance;
            _offsetFromGround = offsetFromGround;
            _offsetY = offsetY;
            
            _initialForward = transform.forward;

            transform.SetParent(null);
        }

        private void LateUpdate()
        {
            if (!_targetBone) return;
            
            Vector3 rayOrigin = _targetBone.position + Vector3.up * _offsetY;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _castDistance, _groundLayer))
            {
                transform.position = hit.point + hit.normal * _offsetFromGround;
                
                Vector3 forwardOnFloor = Vector3.ProjectOnPlane(_initialForward, hit.normal).normalized;

                if (forwardOnFloor != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(forwardOnFloor, hit.normal);
            }
            else
            {
                transform.position = new Vector3(rayOrigin.x, transform.position.y, rayOrigin.z);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_targetBone) return;

            Vector3 rayOrigin = _targetBone.position + Vector3.up * _offsetY;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(rayOrigin, 0.05f);
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * _castDistance);
        }
    }
}