using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{

    //STATE TRACKING
    private AIState currentState = AIState.Patrol;
    private bool walkPointSet;
    private Vector3 walkPoint;
    private float walkPointRange = 10f;

    //REFERENCES
    public NavMeshAgent agent;
    public LayerMask whatIsPlayer;
    public LayerMask groundLayer;
    private Transform player;
    private AIController aiController;
    private Rigidbody rb;
    public AIState CurrentState => currentState;

    //HEALTH PROPERTIES
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    //ATTACK PROPERTIES
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked = false;

    //AI STATES
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;    
    public Transform attackPoint; //Assign in inspector

    public enum AIState
    {
        Idle,
        Chase,
        Attack,
        Patrol,
        PickedUp,
        Thrown,
        Captured
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        aiController = GetComponent<AIController>();
        currentHealth = maxHealth;  
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found!");
        }
    }

    private void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            playerInSightRange = distance <= sightRange;
            playerInAttackRange = Physics.CheckSphere(attackPoint.position, attackRange, whatIsPlayer);
        }

        UpdateAI(player != null ? player.position : Vector3.zero);
    }

    public void UpdateAI(Vector3 playerPosition)
    {
        if (currentState == AIState.PickedUp || currentState == AIState.Thrown || currentState == AIState.Captured)
        {
            agent.isStopped = true;
            return;
        }

        //State transition logic below:
        //If chasing the Player and is in attack range, switch to Attack state
        if (playerInAttackRange && currentState == AIState.Chase)
        {
            ChangeState(AIState.Attack);
        }
        
        //If player is not in attack range but is visible, switch to Chase
        else if (!playerInAttackRange && playerInSightRange && currentState == AIState.Attack)
        {
            ChangeState(AIState.Chase);
        }

        //If player is out of sight, switch to Patrol
        else if(!playerInSightRange && currentState != AIState.Patrol)
        {
            ChangeState(AIState.Patrol);
        }

            switch (currentState)
            {
                case AIState.Idle:
                    Idle();
                    break;
                case AIState.Chase:
                    Chase(playerPosition);
                    break;
                case AIState.Attack:
                    Attack();
                    break;
                case AIState.Patrol:
                    Patrol();
                    break;
                case AIState.Captured:
                    Debug.Log("Chicken captured");
                    break;
                case AIState.Thrown:
                    Debug.Log("Chicken thrown");
                    break;
                case AIState.PickedUp:
                    Debug.Log("Chicken has been picked up!");
                    break;
            }
    }

    private void Idle()
    {
        agent.isStopped = true;
    }

    //ATTACK LOGIC
    private void Attack()
    {
        if (player == null)
        {
            ChangeState(AIState.Idle);
            return;
        }

        agent.SetDestination(player.position);
        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerInAttackRange)
            {
                Debug.Log("Chicken attack! Player health before: " + playerHealth.currentHealth);
                playerHealth.TakeDamage(10);
                Debug.Log("Player health after attack: " + playerHealth.currentHealth);
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on player!");
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    //DAMAGE/ATTACK LOGIC (triggered when the chicken is attacked)
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Chicken took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            //Inform manager about the kill
            ChickenManager.instance.ChickenKilled();
            
            //Switch to Dark
            GameManager.instance.SetTone(GameTone.Dark);
            return;
        }

        if (currentState == AIState.PickedUp)
        {
            transform.parent = null;
            rb.isKinematic = false;
            agent.enabled = false;
            ChangeState(AIState.Chase);
            return;
        }

        if (currentState != AIState.Thrown && currentState != AIState.Captured)
        {
            ChangeState(AIState.Chase);
        }
    }

    private void Die()
    {
        Debug.Log("Chicken has died.");
        FindFirstObjectByType<UI_Chickens>().IncrementKilled();
        ChickenManager.instance.SetAllChickensToAggro();
        agent.enabled = false;
        gameObject.SetActive(false);
    }

    //CHASE LOGIC
    public void Chase(Vector3 playerPosition)
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(playerPosition);
       
    }

    //PATROL PATH LOGIC

    private void Patrol()
    {
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);

            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                walkPointSet = false;
                StartCoroutine(WaitBeforePatrolling());
            }
        }
    }
    private IEnumerator WaitBeforePatrolling()
    {
        yield return new WaitForSeconds(Random.Range(5f, 15f));
        walkPointSet = false;
    }

    //Generate patrol path randomly for now - will have set path in level
    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y + 1f, transform.position.z + randomZ);

        Debug.DrawRay(walkPoint, Vector3.down * 2f, Color.red, 2f);
        if (Physics.Raycast(walkPoint, -Vector3.up, 2f, groundLayer))
        {
            walkPointSet = true;
        }
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;

        if (newState == AIState.Idle)
        {
            Idle();
        }

    }
    private void OnEnable()
    {
        aiController?.RegisterEnemy(this);
    }

    private void OnDisable()
    {
        aiController?.UnregisterEnemy(this);
    }

    //PICKUP LOGIC

    private Coroutine pickupCoroutine;

    public void OnPickup(Transform carryPoint)
    {
        if (currentState == AIState.PickedUp) return;
        if (pickupCoroutine == null)
        {
            pickupCoroutine = StartCoroutine(SmoothPickup(carryPoint));
        }
    }

    private IEnumerator SmoothPickup(Transform carryPoint)
    {
        ChangeState(AIState.PickedUp);
        agent.enabled = false;
        rb.isKinematic = true; 
        rb.angularVelocity = Vector3.zero;

        transform.parent = carryPoint;
        Vector3 initialLocalPos = transform.localPosition;
        Quaternion initialLocalRot = transform.localRotation;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.localPosition = Vector3.Lerp(initialLocalPos, Vector3.zero, t);
            transform.localRotation = Quaternion.Lerp(initialLocalRot, Quaternion.identity, t);

            yield return null;
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        pickupCoroutine = null;

    }

    //THROW LOGIC
    public void OnThrow(Vector3 force)
    {
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }
        
        ChangeState(AIState.Thrown);
        rb.isKinematic = false;
        agent.enabled = false;
        transform.parent = null;
        
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);

        StartCoroutine(ReenableAfterThrow());
    }

    //ON BEING SET DOWN GENTLY  
    public void OnSetDown(Vector3 position)
    {
        StartCoroutine(SmoothSetDown(position));
    }

    private IEnumerator SmoothSetDown(Vector3 position)
    {
        ChangeState(AIState.Idle);
        rb.isKinematic = false;
        transform.parent = null;
        agent.enabled = true;

        while (Vector3.Distance(transform.position, position) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * 10f);
            yield return null;
        }

        transform.position = position;
    }

    private IEnumerator ReenableAfterThrow()
    {
        yield return new WaitForSeconds(1.0f);
        //Only renable chicken if not captured
        if (currentState == AIState.Thrown)
        {
            agent.enabled = true;
            ChangeState(AIState.Patrol);
        }
    }

    
}

