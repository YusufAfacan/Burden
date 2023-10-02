using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    Transform cameraObject;
    InputHandler inputHandler;
    public Vector3 moveDirection;

    [HideInInspector]
    public Transform myTransform;
    [HideInInspector]
    public AnimatorHandler animatorHandler;

    public new Rigidbody rigidbody;
    public GameObject normalCamera;

    public float groundDetectionRayStartPoint = 0.5f;
    public float minimumDistanceNeededToBeginFall = 1f;
    public float groundDirectionRayDistance = 0.2f;
    public LayerMask ignoreForGroundCheck;
    public float inAirTime;

    [Header("Stats")]
    [SerializeField]
    float movementSpeed = 5;
    [SerializeField]
    float sprintSpeed = 7;
    [SerializeField]
    float rotationSpeed = 10;
    public float fallingSpeed = 45f;
    PlayerManager playerManager;
    


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        playerManager = GetComponentInParent<PlayerManager>();
        cameraObject = Camera.main.transform;
        myTransform = transform;
        animatorHandler.Initiliaze();
        playerManager.isGrounded = true;
    }

    private void Update()
    {
    }

    #region Movement
    Vector3 normalVector;
    Vector3 targetPosition;

    public void HandleMovement(float delta)
    {
        if (inputHandler.rollFlag)
        {
            return;
        }

        if (playerManager.isInteracting)
        {
            return;
        }

        moveDirection = cameraObject.forward * inputHandler.vertical;
        moveDirection += cameraObject.right * inputHandler.horizontal;
        moveDirection.Normalize();
        moveDirection.y = 0;

        float speed = movementSpeed;

        if(inputHandler.sprintFlag)
        {
            speed = sprintSpeed;
            playerManager.isSprinting = true;
            moveDirection *= speed;
        }
        else
        {
            moveDirection *= speed;
        }

        

        Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
        rigidbody.velocity = projectedVelocity;

        animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0, playerManager.isSprinting);

        if (animatorHandler.canRotate)
        {
            HandleRotation(delta);
        }
    }

    private void HandleRotation(float delta)
    {
        Vector3 targetDir = Vector3.zero;
        float moveOverride = inputHandler.moveAmount;

        targetDir = cameraObject.forward * inputHandler.vertical;
        targetDir += cameraObject.right * inputHandler.horizontal;

        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
        {
            targetDir = myTransform.forward;
        }

        float rs = rotationSpeed;

        Quaternion tr = Quaternion.LookRotation(targetDir);
        Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

        myTransform.rotation = targetRotation;
    }

    public void HandleRollingAndSprinting(float delta)
    {
        if (animatorHandler.anim.GetBool("isInteracting"))
        {
            return;
        }

        if (inputHandler.rollFlag)
        {
            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            if (inputHandler.moveAmount > 0)
            {
                
                animatorHandler.PlayTargetAnimation("Rolling", true);
                moveDirection.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                myTransform.rotation = rollRotation;
            }
            else
            {
                animatorHandler.PlayTargetAnimation("Backstep", true);
            }
        }
    }

    public void HandleFalling(float delta, Vector3 moveDirection)
    {
        playerManager.isGrounded = false;
        RaycastHit hit;
        Vector3 origin = myTransform.position;
        origin.y += groundDetectionRayStartPoint;

        if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
        {
            moveDirection = Vector3.zero;
        }

        if (playerManager.isInAir)
        {
            rigidbody.AddForce(-Vector3.up * fallingSpeed);
            rigidbody.AddForce(moveDirection * fallingSpeed / 15f);
        }

        Vector3 dir = moveDirection;
        dir.Normalize();
        origin += dir * groundDirectionRayDistance;

        targetPosition = myTransform.position;

        Debug.DrawLine(origin, targetPosition - Vector3.up * minimumDistanceNeededToBeginFall, Color.red);
        if (Physics.Raycast(origin, targetPosition - Vector3.up * minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
        {
            normalVector = hit.normal;
            Vector3 tp = hit.point;
            playerManager.isGrounded = true;
            targetPosition.y = tp.y;

            if(playerManager.isInAir)
            {
                if (inAirTime > 0.5f)
                {
                    animatorHandler.PlayTargetAnimation("Land", true);
                    inAirTime = 0;
                }

                else
                {
                    animatorHandler.PlayTargetAnimation("Locomotion", true);
                    inAirTime = 0;
                }

                playerManager.isInAir = false;
            }
        }
        else
        {
            if(playerManager.isGrounded)
            {
                playerManager.isGrounded = false;
            }
            if (playerManager.isInAir == false)
            {
                if (playerManager.isInteracting == false)
                {
                    animatorHandler.PlayTargetAnimation("Falling", true);
                    Debug.Log("fall");
                }

                Vector3 vel = rigidbody.velocity;
                vel.Normalize();
                rigidbody.velocity = vel * (movementSpeed / 2);
                playerManager.isInAir = true;
            }
        }

        if (playerManager.isGrounded)
        {
            if (playerManager.isInteracting || inputHandler.moveAmount > 0)
            {
                myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime);
            }
            else
            {
                myTransform.position = targetPosition;
            }
        }

        
    }

    #endregion


}
