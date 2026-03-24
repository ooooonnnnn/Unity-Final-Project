using System;
using System.Linq;
using Interface;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Player
{
    public class CharacterComponents : MonoBehaviour, IDamageable
    {
        public static CharacterComponents Instance;
        
        public string GroundLayerName = "Ground";

        [SerializeField] private InputActionReference moveAction;
        private PlayerInput.ActionEvent moveInputEvent;
        
        public const float MAX_HEALTH = 100f;
        public event Action OnPlayerDied;
        public event Action<float> OnHealthChanged;

        public NavMeshAgent navMeshAgent => _navMeshAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;
        public Transform cameraFollowTarget => _cameraFollowTarget;
        [SerializeField] private Transform _cameraFollowTarget;

        public Collider playerCollider => _PlayerCollider;
        [SerializeField] private Collider _PlayerCollider;
        
        [SerializeField] private SpellCaster spellCaster;


        private float health;

        private void Awake()
        {
            Instance = this;
            

            
        }

        private void Start()
        {
            
            health = MAX_HEALTH;
            OnHealthChanged?.Invoke(health);
            
            moveAction.action.Enable();
            moveAction.action.performed += MoveCharacter;
            
            ManagersMaster.Instance.VoiceInputPipeline.OnPipelineDone.AddListener(spellCaster.CastSpellFromParameters);
        }
        
        private void OnDestroy()
        {
            ManagersMaster.Instance.VoiceInputPipeline.OnPipelineDone.RemoveListener(spellCaster.CastSpellFromParameters);
            moveAction.action.performed -= MoveCharacter;
        }
        

        private void OnValidate()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>(); //editor time
            spellCaster = GetComponent<SpellCaster>();
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                OnPlayerDied?.Invoke();
                return;
            }

            OnHealthChanged?.Invoke(health);
        }
        
        public void MoveCharacter(InputAction.CallbackContext ctx)
        {
            if (ctx.phase == InputActionPhase.Canceled) return;
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(ctx.ReadValue<Vector2>());
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);
            if (!Physics.Raycast(ray, out RaycastHit colliderHit, 10000, LayerMask.GetMask(GroundLayerName)))
            {
                return;
            }

            // print(colliderHit.point);
            
            if (!navMeshAgent.enabled)
                return;
            navMeshAgent.SetDestination(colliderHit.point);
        }
    }
}