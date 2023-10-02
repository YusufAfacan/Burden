using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    InputHandler inputHandler;
    Animator anim;
    CameraHandler cameraHandler;
    PlayerLocomotion playerLocomotion;

    public bool isInteracting;
    public bool isSprinting;
    public bool isInAir;
    public bool isGrounded;

    private void Awake()
    {
        cameraHandler = CameraHandler.instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        anim = GetComponentInChildren<Animator>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    // Update is called once per frame
    void Update()
    {
        isInteracting = anim.GetBool("isInteracting");
        


        float delta = Time.deltaTime;

        inputHandler.TickInput(delta);
        playerLocomotion.HandleMovement(delta);
        playerLocomotion.HandleRollingAndSprinting(delta);
        playerLocomotion.HandleFalling(delta, playerLocomotion.moveDirection);
    }

    private void LateUpdate()
    {
        inputHandler.rollFlag = false;
        inputHandler.sprintFlag = false;

        isSprinting = inputHandler.b_input;

        if (isInAir)
        {
            playerLocomotion.inAirTime += Time.deltaTime;
        }

        float delta = Time.deltaTime;

        if (cameraHandler != null)
        {
            cameraHandler.FollowTarget(delta);
            cameraHandler.HandleCameraRotation(delta, inputHandler.mouseX, inputHandler.mouseY);
        }

    }

    private void FixedUpdate()
    {
        
    }
}
