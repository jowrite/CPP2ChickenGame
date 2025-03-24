using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, ThirdPersonInputs.IOverworldActions
{
    #region Variables
    CharacterController cc;
    ThirdPersonInputs inputs;
    Camera mainCamera;
    Animator anim;

    //movement & rotation
    [Header("Movement Variables")]
    [SerializeField] private float initSpeed = 5.0f;
    [SerializeField] private float maxSpeed = 15.0f;
    [SerializeField] private float moveAccel = 0.2f;
    [SerializeField] private float rotationSpeed = 30.0f;
    private float curSpeed = 5.0f;

    [Header("Weapon Variables")]
    [SerializeField] private Transform weaponAttachPoint;
    Weapon weapon = null;

    public LayerMask raycastCollisionLayer;
    
    //Character movement
    Vector2 inputDirection;
    Vector3 velocity;
    
    //Jump variables
    private bool isJumpPressed = false;
    [SerializeField] private float jumpHeight = 3.25f;
    [SerializeField] private float jumpTime = 1f;
    private float gravity;
    private float timeToApex; //max jump time divided by 2
    private float initialJumpVelocity;

    //Pickup variables for chickens
    //When a chicken is pickup, store it so we can later throw it
    private GameObject carriedEnemy;
    public Transform carryPoint;
    public float throwForce = 10f;

    [Header("Attack Variables")]
    public float attackRange = 2f;
    public float attackDamage = 5f;

    //HEALTH PROPERTIES
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    #endregion

    #region Setup Functions

    private void Awake()
    {
        inputs = new ThirdPersonInputs();
        //Set callbacks to this script
        inputs.Overworld.SetCallbacks(this);
        inputs.Overworld.Enable();

        //Scene management
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        InitJump();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitJump()
    {
        timeToApex = jumpTime / 2;
        gravity = (-2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = -(gravity * timeToApex);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
    }
    #endregion

    #region Player Movement Functions
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            inputDirection = ctx.ReadValue<Vector2>();
        }
        else if (ctx.canceled)
        {
            inputDirection = Vector2.zero;
        }
    }
    //Projects the 2D input into world space based on camera orientation
    private Vector3 ProjectionMoveDirection()
    {
        //Check if mainCamera is valid
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is null");
            return Vector3.zero;
        }
        
        Vector3 cameraFwd = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraFwd.y = 0;
        cameraRight.y = 0;
        cameraFwd.Normalize();
        cameraRight.Normalize();

        return cameraFwd * inputDirection.y + cameraRight * inputDirection.x;
    }
    //Updates the velocity vector based on the desired move direciton and jump/gravity
    private void UpdateCharacterVelocity(Vector3 moveDir)
    {
        //Horizontal movement (reset speed when no input)
        if (moveDir == Vector3.zero)
        {
            curSpeed = initSpeed;
            velocity.x = 0;
            velocity.z = 0;
        }
        else
        {
            curSpeed = Mathf.Clamp(curSpeed + moveAccel * Time.fixedDeltaTime, initSpeed, maxSpeed);
            velocity.x = moveDir.x * curSpeed;
            velocity.z = moveDir.z * curSpeed;
        }

        //Vertical movement
        if (cc.isGrounded)
        {
            //When grounded, check for jump
            if (isJumpPressed)
            {
                velocity.y = initialJumpVelocity;
            }
            else
            {
                //Small downward force to keep character grounded
                velocity.y = -cc.minMoveDistance;
            }
        }
        else
        {
            //In air, apply gravity
            velocity.y += gravity * Time.fixedDeltaTime;

            //Add logic for variable jump height
        }

    }
    #endregion

    #region Input Callbacks
    //Jump input callback
    public void OnJump(InputAction.CallbackContext ctx)
    {
        isJumpPressed = ctx.ReadValueAsButton();
        if (isJumpPressed)
        {
            anim.SetTrigger("Jump");
        }
    }

    public void OnAttack(InputAction.CallbackContext ctx)
    {

        //Prevent overlapping attack animations
        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Attack")) return;

        if (ctx.performed)
        {   
            Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

            anim.SetTrigger("Attack");

            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Enemy") || hit.CompareTag("Chicken"))
                {
                    EnemyAI enemy = hit.GetComponent<EnemyAI>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage((int)attackDamage);
                    }
                }
            }
        }
    }

    public void OnPickUp(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Pickup input recieved");

            //If already carrying a chicken, then throw it
            if (carriedEnemy != null)
            {
                Vector3 throwDirection = transform.forward + Vector3.up * 1.5f; //Adjust to create an arc
                EnemyAI enemyAI = carriedEnemy.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.OnThrow(throwDirection * throwForce);
                }
                carriedEnemy = null;
            }
            else
           {
                //Search for a chicken to pick up
                Collider[] hits = Physics.OverlapSphere(transform.position, 2.0f);
                foreach (Collider hit in hits)
                {
                    if (hit.CompareTag("Chicken"))
                    {
                        EnemyAI enemy = hit.GetComponent<EnemyAI>();
                        if (enemy != null  && enemy.CurrentState == EnemyAI.AIState.Patrol)
                        {                        
                            //Pick up enemy
                            carriedEnemy = hit.gameObject;
                            enemy.OnPickup(carryPoint);
                            break;
                        }
                    }
                }
            }
        }
    }

    #endregion

    void Update()
    {
        //Initialize healthbar
        currentHealth = maxHealth;
        //Update animation blend using horizontal movement magnitude only
        Vector2 horizontalVel = new Vector2(velocity.x, velocity.z);
        anim.SetFloat("blend", horizontalVel.magnitude);

        //Debug ray to check for collisions
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 10.0f), Color.red);
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, 10.0f, raycastCollisionLayer))
        {
            Debug.Log(hitInfo);
        }  
        
    }

    private void FixedUpdate()
    {
        if (!enabled) return;
        
        //Get desired movement direction in world space
        Vector3 desiredMoveDirection = ProjectionMoveDirection();
        
        //Update velocity based on input/jump/gravity logic
        UpdateCharacterVelocity(desiredMoveDirection);
        //Move the character multiplying by Time.fixedDeltatime for frame-rate independence
        cc.Move(velocity * Time.fixedDeltaTime);

        //rotate towards direction of movement
        if (desiredMoveDirection.magnitude > 0)
        {
            float step = rotationSpeed * Time.fixedDeltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, step);
        }  
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Automatically pick up a weapon if collided with one and not already holding one
        if (hit.collider.CompareTag("Weapon") && weapon == null)
        {
            weapon = hit.gameObject.GetComponent<Weapon>();
            if (weapon != null)
            { 
                weapon.Equip(GetComponent<Collider>(), weaponAttachPoint);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LevelTest")
        {
            this.enabled = false;
        }
        else
        {
            this.enabled = true;
            mainCamera = Camera.main;
        }
    }
}

