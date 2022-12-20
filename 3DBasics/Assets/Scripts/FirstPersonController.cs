using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool IsMoving => (Mathf.Abs(moveDirection.x) > 0.1f) || (Mathf.Abs(moveDirection.z) > 0.1f);
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKey(jumpKey) && controller.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && controller.isGrounded;


    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canjump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canHeadBob = true;
    [SerializeField] private bool canSlide = true; 
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool canUseFootStep = true;
    [SerializeField] private bool hasRegenerativeHealth = true;
    [SerializeField] private bool canUseStamina = true;
    

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slideSlopeSpeed = 9f;
    [SerializeField] private float airMovementSpeed = 0.25f;

    [Header("Look Parameters")]
    [SerializeField, Range(100, 400)] private float lookSpeed = 150;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 70f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField,Range(1,20)] private float jumpForce = 8f;
    [SerializeField] private float gravity = 30f;

    [Header("Crouching Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);
    private bool isCrouching = false;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Zoom Parameter")]
    [SerializeField] private ZoomType zoomType = ZoomType.HoldZoom;
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;
    private ZoomType defaultZoomType = ZoomType.HoldZoom;

    [Header("Interaction")]
    [SerializeField]private Vector3 interactionRayPoint= default;
    [SerializeField]private float  interactioDistance= default;
    [SerializeField]private LayerMask interactioLayer= default;
    private Interactable currInteractable = null;

    [Header("Foot Step Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footStepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] metalClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footStepTimer = 0f;
    private float getCurrentOffset => isCrouching?baseStepSpeed*crouchStepMultiplier : IsSprinting? baseStepSpeed*sprintStepMultiplier : baseStepSpeed;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healtValueIncreament = 1f;
    [SerializeField] private float healthTimeIncreament = 0.1f;
    private float currHealth;
    private Coroutine regenratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaUseMultiplier = 5f;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5f;
    [SerializeField] private float staminaValueIncreament = 2f;
    [SerializeField] private float staminaTimeIncreament = 0.1f;
    private float currStamina;
    private Coroutine regenratingStamina;
    public static Action<float> OnStaminaChange;

    [Header("Shooting toogle")]
    [SerializeField] public bool canShoot = true;

    private Vector2 screenBounds;



    //Sliding Parameters
    private Vector3 hitPointNormal;
    private bool shouldSlide
    {
        get
        {
            if(controller.isGrounded && Physics.Raycast(transform.position,Vector3.down,out RaycastHit slopeHit, 2f)){
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }



    //References.
    private Camera playerCamera;
    private CharacterController controller;

    //Inputs
    private Vector3 moveDirection;
    private Vector2 currInput;
    private float mouseX;
    Vector3 targetVelocity;
    private float mouseY;
    float currSpeed = 0;
    private float rotationX = 0f;

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;   
    }
    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }
    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();
        currHealth = maxHealth;
        currStamina = maxStamina;
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    private void Update()
    {
        if (!CanMove)
        {
            return;
        }
        handleMoveInputs();
        handleMouseLook();

        if (canjump)
        {
            handleJump();
        }
        if (canCrouch)
        {
            handleCrouch();
        }
        if (canHeadBob)
        {
            handleHeadBob();
        }
        if (canZoom)
        {
            handleZoom();
        }

        if (canInteract)
        {
            handleInteractionCheck();
            handleInteractionInput();
        }
        /*if (canUseFootStep)
        {
            handleFootStep();
        }*/
        if (canUseStamina)
        {
            handleStamina();
        }

        applyFinalMovements();
    }

    private void handleMoveInputs()
    {
        
        if (controller.isGrounded) 
        {
            currSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed; 
        }
       
        float x = Input.GetAxis("Horizontal")*currSpeed;
        float z = Input.GetAxis("Vertical")* currSpeed;
        currInput = new Vector2(x,z);
        //targetVelocity = new Vector3(x, 0, z);

        float moveDirY = moveDirection.y;

        //One way to handle moving direction.
        /*moveDirection = (Vector3.right * currInput.x + Vector3.forward * currInput.y);*/
        
        moveDirection = (transform.TransformDirection(Vector3.right * currInput.x) + transform.TransformDirection(Vector3.forward * currInput.y)); 
        moveDirection.y = moveDirY;
    }
    private void handleMouseLook()
    {
        if (isCursorOutOfBounds())
        {
            return;
        }
        float tempX = Input.GetAxis("Mouse X");
        float tempY = Input.GetAxis("Mouse Y");

        mouseX = tempX * lookSpeed * Time.deltaTime;
        mouseY = tempY * lookSpeed * Time.deltaTime;


        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX,-upperLookLimit,lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0f,mouseX,0f);
    }

    private void handleJump()
    {
       
        if (ShouldJump) 
        {
            //if(isCrouching)
            /*#region Testing
            jumpHeight = CalculateHieght(jumpForce);
            Debug.Log(jumpHeight);
            #endregion*/
            if (isCrouching)
            {
                StartCoroutine(CrouchStand());
                return;
            }
             moveDirection = new Vector3(controller.velocity.x*currSpeed,jumpForce, controller.velocity.z*currSpeed);
             Debug.Log(moveDirection);

           // targetVelocity = new Vector3(controller.velocity.x,jumpForce,controller.velocity.z);

        }
    }
    private void handleCrouch()
    {
        if (ShouldCrouch)
        {
            
            StartCoroutine(CrouchStand());
           
        }
    }

    private void handleHeadBob()
    {
        if (!controller.isGrounded)
            return;
        if(IsMoving)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x,
                defaultYPos+Mathf.Sin(timer)* (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }
    private void handleZoom()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            if(zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToogleZoom(true));
        }
        if (Input.GetKeyUp(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToogleZoom(false));
        }
    }

    private void handleInteractionCheck()
    {
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactioDistance))
        {
            if(hit.collider.gameObject.layer == 9 && (currInteractable == null || hit.collider.gameObject.GetInstanceID() != currInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currInteractable);

                if(currInteractable != null)
                {
                    currInteractable.OnFocus();
                }
            }
        }
        else if(currInteractable)
        {
            currInteractable.OnLooseFocus();
            currInteractable = null;
        }
    }

    private void handleInteractionInput()
    {
        if(Input.GetKey(interactKey) && currInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint),out RaycastHit hit,interactioDistance,interactioLayer))
        {
            currInteractable.OnInteract();
        }
    }

    private void handleFootStep()
    {
        if(!controller.isGrounded) return;
        if (currInput == Vector2.zero) return;

        footStepTimer -= Time.deltaTime;

        if(footStepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position,Vector3.down,out RaycastHit hit,3))
            {
                switch (hit.collider.tag)
                {
                    case "Footstep/WOOD":
                        footStepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0,woodClips.Length-1)]);
                        break;
                    case "Footstep/METAL":
                        footStepAudioSource.PlayOneShot(metalClips[UnityEngine.Random.Range(0, metalClips.Length - 1)]);
                        break ;
                    case "Footstep/GRASS":
                        footStepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length - 1)]);
                        break;
                    default:
                        footStepAudioSource.PlayOneShot(grassClips[UnityEngine.Random.Range(0, grassClips.Length - 1)]);
                        break ;
                }
            }
        }
        footStepTimer = getCurrentOffset;
    }

    private void handleStamina()
    {
        if (IsSprinting && currInput != Vector2.zero)
        {  
            if(regenratingStamina != null)
            {
                StopCoroutine(regenratingStamina);
                regenratingStamina = null;
            }
            currStamina -= staminaUseMultiplier * Time.deltaTime;

            if (currStamina < 0)
            {
                currStamina = 0;
            }

            OnStaminaChange?.Invoke(currStamina); 

            if(currStamina <= 0)
                canSprint = false;
        }

        if(!IsSprinting && currStamina<maxStamina && regenratingStamina == null)
        {
            regenratingStamina = StartCoroutine(regenrateStamina());
        }
    }
    private void applyFinalMovements()
    {
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (canSlide && shouldSlide)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slideSlopeSpeed;
        }
        controller.Move(moveDirection*Time.deltaTime);
    }

    private float CalculateHieght(float y)
    {
        float height = 0f;
        if(y > 0)
            height = Mathf.Pow(y, 2) / 2 * gravity;
        
        return height;
    }

    private void ApplyDamage(float dmgAmt)
    {
        currHealth -= dmgAmt;
        //Debug.Log(currHealth);
        OnDamage?.Invoke(currHealth);

        if (currHealth <= 0)
        {
            currHealth = 0;
            KillPlayer();
        }
        else if (regenratingHealth != null)
            StopCoroutine(regenratingHealth);

        regenratingHealth = StartCoroutine(regenrateHealth());
    }

    private void KillPlayer()
    {
        currHealth = 0;
        if (regenratingHealth != null)
            StopCoroutine(regenratingHealth);
    }

    private IEnumerator regenrateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait  = new WaitForSeconds(healthTimeIncreament);

        while(currHealth < maxHealth)
        {
            currHealth  += healtValueIncreament;
            if (currHealth > maxHealth)
                currHealth = maxHealth;

            OnHeal?.Invoke(currHealth);
            yield return timeToWait;
        }

        regenratingHealth = null;
    }

    private IEnumerator regenrateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncreament);

        while(currStamina < maxStamina)
        {
            if (currStamina > 0)
                canSprint = true;

            currStamina += staminaValueIncreament;

            if(currStamina > maxStamina)
            {
                currStamina = maxStamina;
            }

            OnStaminaChange?.Invoke(currStamina);
            yield return timeToWait;
        }
        Debug.Log("can sprint : "+canSprint);
        regenratingStamina = null;
    }

    //handling crouch to stand transition and vice versa
    private IEnumerator CrouchStand()
    {
       
        if(isCrouching && Physics.Raycast(playerCamera.transform.position,Vector3.up,1f))
            yield break;
       
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standHeight : crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        while(timeElapsed< timeToCrouch)
        {
            controller.height = Mathf.Lerp(currentHeight,targetHeight,timeElapsed/timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter,targetCenter,timeElapsed/timeToCrouch);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
        controller.center = targetCenter;

        isCrouching = !isCrouching;
       // Debug.Log($"Is Crouching: {isCrouching}, Current Height: {controller.height}, Current Center: {controller.height}");
        duringCrouchAnimation = false;
 
    }
    private IEnumerator ToogleZoom(bool isEnteringZoomState)
    {
        float tragetFOV = isEnteringZoomState ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;
        while(timeElapsed< timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV,tragetFOV,timeElapsed/timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //Making sure it reaches the target FOV
        playerCamera.fieldOfView = tragetFOV;
        zoomRoutine = null;
    }

    private bool isCursorOutOfBounds()
    {
       // Debug.Log(screenBounds);
        if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
        {
            //Debug.Log($"Screen bounds are: {screenBounds} and mouse Position is: {Input.mousePosition}");
            return true;
        }
        return false; 
    }


}
