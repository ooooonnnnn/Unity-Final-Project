using Enemy;
using Interface;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visual;
    [SerializeField] private MonoBehaviour attackBehaviour;
    [SerializeField] private SpellCaster spellCaster;

    private IEnemyAttack attack;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private Transform target;
    private Transform enemyTransform;

    private float pathUpdateTimer;
    private bool isRotationLocked;
    private Quaternion lockedRotation;

    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float pathUpdateInterval = 0.15f;
    [SerializeField] private float destinationThreshold = 0.25f;
    [SerializeField] private float rangedBuffer = 0.75f;

    public static event System.Action<EnemyBase> OnEnemyKilled;

    private void Awake()
    {
        enemyTransform = transform;

        agent.speed = data.moveSpeed;
        agent.updateRotation = false;
        agent.avoidancePriority = Random.Range(70, 90);
        agent.stoppingDistance = 0f;

        attack = attackBehaviour as IEnemyAttack;
        if (attack == null)
        {
            Debug.LogError($"{name}: AttackBehaviour does not implement IEnemyAttack");
        }
    }

    private void OnEnable()
    {
        EnemyManager.Register(this);
    }

    private void OnDisable()
    {
        EnemyManager.Unregister(this);
    }

    public void Initialize(Transform playerTarget)
    {
        target = playerTarget;

        attack?.Initialize(transform, target);
        if (spellCaster)
        {
            spellCaster.SetTarget(playerTarget);
        }
    }

    public void Tick(float dt)
    {
        if (!target || attack == null)
            return;

        HandleRotation(dt);

        if (attack is AttackRanged)
            HandleRanged(dt);
        else
            HandleMelee(dt);

        UpdateAnimation();
    }

    private void HandleMelee(float dt)
    {
        bool inRange = attack.IsInRange();

        if (!inRange)
        {
            ChasePlayer(dt);
        }
        else
        {
            agent.isStopped = true;
            attack.Tick(dt);
        }
    }

    private void HandleRanged(float dt)
    {
        Vector3 toEnemy = enemyTransform.position - target.position;
        toEnemy.y = 0f;

        float distance = toEnemy.magnitude;
        float min = data.minAttackRange;
        float max = data.maxAttackRange;
        float ideal = (min + max) * 0.5f;

        bool tooClose = distance < (min - rangedBuffer);
        bool tooFar = distance > (max + rangedBuffer);
        bool inBand = !tooClose && !tooFar;

        if (inBand)
        {
            agent.isStopped = true;
            attack.Tick(dt);
            return;
        }

        agent.isStopped = false;

        pathUpdateTimer += dt;
        if (pathUpdateTimer < pathUpdateInterval)
            return;

        pathUpdateTimer = 0f;

        Vector3 dirAway = toEnemy.sqrMagnitude > 0.001f ? toEnemy.normalized : enemyTransform.forward;
        Vector3 desiredPosition = target.position + dirAway * ideal;
        desiredPosition.y = enemyTransform.position.y;

        if (!agent.hasPath || (agent.destination - desiredPosition).sqrMagnitude > destinationThreshold * destinationThreshold)
        {
            agent.SetDestination(desiredPosition);
        }
    }

    private void ChasePlayer(float dt)
    {
        agent.isStopped = false;

        pathUpdateTimer += dt;
        if (pathUpdateTimer < pathUpdateInterval)
            return;

        pathUpdateTimer = 0f;

        Vector3 desiredPosition = target.position;

        if (!agent.hasPath || (agent.destination - desiredPosition).sqrMagnitude > destinationThreshold * destinationThreshold)
        {
            agent.SetDestination(desiredPosition);
        }
    }

    private void HandleRotation(float dt)
    {
        bool isAttackingAnim = IsInAttackState();

        if (isAttackingAnim)
        {
            if (!isRotationLocked)
            {
                lockedRotation = enemyTransform.rotation;
                isRotationLocked = true;
            }

            enemyTransform.rotation = lockedRotation;
            return;
        }

        isRotationLocked = false;

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
        enemyTransform.rotation = Quaternion.RotateTowards(
            enemyTransform.rotation,
            targetRotation,
            rotationSpeed * dt
        );
    }

    private void UpdateAnimation()
    {
        Vector3 flatVelocity = agent.velocity;
        flatVelocity.y = 0f;

        bool isMoving = flatVelocity.sqrMagnitude > 0.01f && !agent.isStopped;
        animator.SetBool(IsMovingHash, isMoving);
    }

    private bool IsInAttackState()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        return state.IsName("Attack");
    }

    public void Die()
    {
        OnEnemyKilled?.Invoke(this);
        gameObject.SetActive(false);
    }
}