using Unity.Cinemachine;
using UnityEngine;

namespace Camera
{
    public class OmriRTSCamera : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineFollow followComponent;
        [SerializeField] private CinemachineFollowZoom zoomComponent;

        [Header("Moving")] 
        [SerializeField] private float moveSpeed;
        private Vector3 moveDelta;
        [Header("Zooming")]
        [SerializeField] private float zoomSpeed;
        [SerializeField] private float minWidth;
        [SerializeField] private float maxWidth;

        [Header("Following Target")] 
        public Transform followTarget;
        [SerializeField] private bool following;
    
        public void SetMoveInput(Vector2 move)
        {
            following = false;
        
            moveDelta = new Vector3(move.x, 0f, move.y) * (moveSpeed * Time.deltaTime);
        }

        public void Zoom(float zoomAmount)
        {
            zoomComponent.Width += -zoomAmount * zoomSpeed * Time.deltaTime;
            zoomComponent.Width = Mathf.Clamp(zoomComponent.Width, minWidth, maxWidth);
        }

        private void Update()
        {
            FollowTarget();
        
            transform.position += moveDelta;
        }

        private void FollowTarget()
        {
            if (following)
            {
                transform.position = Vector3.Lerp(transform.position, followTarget.position, Time.deltaTime * 5f); // Smooth following
            }
        }

        public void Recenter()
        {
            following = true;
        }
    
        public void ChangeFollowingCharacter(CharacterComponents character)
        {
            followTarget = character.cameraFollowTarget;
        }
    }
}