using System;
using UnityEngine;

namespace Assets.VehicleController
{
    [HelpURL("https://distubredone322.gitbook.io/custom-vehicle-controller/guides/vehicle-damage-system/vehicle-attachments-aligner")]
    public class VehicleAttachmentsAligner : MonoBehaviour
    {
        [SerializeField]
        private AttachmentParameters[] _attachments;
        private float[] _offsets;

        private void Awake()
        {
            _offsets = new float[_attachments.Length];
        }

        public void RepairAttachments()
        {
            foreach (var att in _attachments)
                att.Repair();

            _offsets = new float[_attachments.Length];
        }

        public void ProcessCollision(Vector3 point, Vector3 normal, float damageArea, float bodyStr)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 down = -transform.up;
            for(int i = 0; i < _attachments.Length;i++)
            {
                if (_offsets[i] >= _attachments[i].MaxOffset)
                    continue;

                float distSQ = (point - _attachments[i].AttachmentTransform.position).sqrMagnitude;
                float damageAreaSQ = damageArea * damageArea;
                if (distSQ > damageAreaSQ)
                    continue;

                Vector3 moveDir = GetMoveDir(_attachments[i], forward, right);
                float dot = Vector3.Dot(normal, moveDir);

                if (dot < 0.7f)
                    continue;

                float falloffStrength = Mathf.Pow(1 - distSQ / damageAreaSQ, 5);
                float deformStrength = Mathf.Clamp(falloffStrength * dot * (1 - bodyStr), 0, _attachments[i].MaxOffset);
                _offsets[i] += deformStrength;
                _attachments[i].MoveAttachment(moveDir * deformStrength);
            }
        }

        public Vector3 GetMoveDir(AttachmentParameters param, Vector3 fwd, Vector3 right)
        {
            switch(param.MoveDirection)
            {
                case MoveDirection.Forward:
                    return fwd;
                case MoveDirection.Backward:
                    return -fwd;
                case MoveDirection.Right:
                    return right;
                default:
                    return -right;
            }
        }
    }

    [Serializable]
    public class AttachmentParameters
    {
        private bool _moved = false;
        private Vector3 _defaultLocalPosition;
        public Transform AttachmentTransform;
        public MoveDirection MoveDirection;
        [Min(0.01f)]
        public float MaxOffset = 0.07f;

        public void MoveAttachment(Vector3 movement)
        {
            if (!_moved)
                _defaultLocalPosition = AttachmentTransform.localPosition;

            _moved = true;
            AttachmentTransform.position += movement;
        }

        public void Repair()
        {
            if (!_moved)
                return;

            AttachmentTransform.localPosition = _defaultLocalPosition;
            _moved = false;
            _defaultLocalPosition = Vector3.zero;
        }
    }

    public enum MoveDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Down,
    }
}
