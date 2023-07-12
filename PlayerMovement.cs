using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XInput;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    private PlayerInputManager inputControl;
    private bool isFreezeInput;

    [Header("Components")]
    private Rigidbody2D rb;
    private PlayerPhysicsCheck physicsCheck;

    [Header("Statements")]
    States playerState;
    enum States {
        None, 
        Moving,
        Jumping,
        Walling,
        Dashing,   
    };
    
    [Header("Movements")] 
    public float moveSpeed;                                    
    public float maxSpeed;
    public float forwardAcceleration;
    public float BackwardAcceleration;                                          // 反方向运动时候
    public float velPower;
    public float windFriction;
    public float velocityCut;

    public float moveAcceleration;
    public float moveVelPower;
    public float moveDeceleration;

    private Vector2 inputMovement;

    [Header("Environment")]
    public float fallingGravity;                                                // 设置重力
    public float normalGravity;
    public float wallGravity;
    public float groundFriction;
    public float airFriction;
    private bool isWind;                                                        // 环境是否存在风

    [Header("NormalJump")]
    public float normalJumpForce;
    public float coyoteExistTime;
    private float coyoteTimer;
    private float jumpInput;
    private bool normalJumpPressed;
    private bool canDoJump;

    [Header("Dash")]
    public float dashForce;
    public float dashXCut;
    public float dashYCut;
    public float dashCoolDownTime;
    public float dashExistTime;
    private float dashInput;
    private float dashLeftTime;
    private float dashStartTime;
    private bool dashPressed;
    private Camera mainCamera;

    private void Awake()
    {
        // Init Component
        inputControl = new PlayerInputManager();
        rb = GetComponent< Rigidbody2D >();
        physicsCheck = GetComponent< PlayerPhysicsCheck >();

        // Init Environment
        isWind = false;

        // Init States
        playerState = States.None;

        isFreezeInput = false;

        dashStartTime = -10f;

        canDoJump = true;
    }

    private void Update()
    {
        // Update Timers
        UpdateCoyoteTimer();

        SetPlayerStatement();

        // 读取输入  冻结的时候保持之前输入
        if ( !isFreezeInput )
        {
            inputMovement = inputControl.GamePlaying.Move.ReadValue< Vector2 >();
            jumpInput = inputControl.GamePlaying.Jump.ReadValue< float >();
            dashInput = inputControl.GamePlaying.Dash.ReadValue< float >();
        }

        if ( jumpInput >= 0.01f && ( coyoteTimer <= 0 || physicsCheck.isGround ) && canDoJump)
        {
            Debug.Log("JumpPressed!");
            normalJumpPressed = true;
        }

        if ( dashInput >= 0.01f && (dashStartTime + dashCoolDownTime) <= Time.time )
        {
            Debug.Log("DashPressed!");
            dashPressed = true;
        }
    }

    private void FixedUpdate()
    {
        SetGravity();
        Movement();

        if (normalJumpPressed)
        {
            normalJumpPressed = false;                          // 重置按键
            canDoJump = false;
            coyoteTimer = coyoteExistTime;                      // 重置计时器
            playerState = States.Jumping;                       // 设置状态
            StartCoroutine(FreezeInputController(3));           // 冻结帧
            NormalJump();
        }

        if (dashPressed)
        {
            dashPressed = false;
            playerState = States.Dashing;
            dashStartTime = Time.time;
            dashLeftTime = dashExistTime;
            StartCoroutine(FreezeInputController(3));
            StartDash();
        }
    }

    private void Movement()
    {
        // /* ====== 处理移动 ====== */
        // float max = maxSpeed * Mathf.Abs(inputMovement.x);                          // 得到此刻的加速度
        // float speedDirection = Mathf.Sign(inputMovement.x);
        // float newVelocity = rb.velocity.x;
        // float updateSpeed = 0f;
        // float targetSpeed = max - rb.velocity.x;
        
        // if(inputMovement.x != 0f)                                                   // 有输入
        // {
        //     if (Mathf.Abs(targetSpeed) <= 0.001f)                                   // 直接赋予最大速度
        //     {
        //         newVelocity = max;
        //     }
        //     else if (Mathf.Sign(rb.velocity.x) == speedDirection)                   // 同向
        //     {
        //         updateSpeed = Mathf.Pow(velPower, forwardAcceleration / Mathf.Abs(targetSpeed)) * speedDirection;
        //     }
        //     else
        //     {
        //         updateSpeed = -newVelocity;                                         // 得到相反速度
        //         updateSpeed += Mathf.Pow(velPower, forwardAcceleration / Mathf.Abs(targetSpeed)) * speedDirection;
        //     }
        // }

        // float frictionmult = isWind ? windFriction : 1;

        // /* ====== 处理无输入时减速 ====== */
        // if (inputMovement.x == 0f)                                                  // 没有输入
        // {
        //     if (!physicsCheck.isGround)                                                          // 在空中
        //     {
        //         frictionmult = 1;
        //         updateSpeed = -newVelocity;                                         // 令相加 = 0
        //     }
                
        //     else                                                                    // 在地面
        //     {
        //         updateSpeed = Mathf.Pow(velPower, BackwardAcceleration / Mathf.Abs(targetSpeed)) * speedDirection;
        //     }
        // }

        // /* ====== 处理摩擦参数 ====== */
        // updateSpeed *= frictionmult;

        // newVelocity += updateSpeed;
        // newVelocity = Mathf.Clamp(newVelocity, -max, max);
        // rb.velocity = new Vector2(newVelocity, rb.velocity.y);
        #region Run

        // 1. 得到移动速度
        float targetSpeed = inputMovement.x * moveSpeed;
        // 2. 判断方向 (SpeedDif)
        float speedDif = targetSpeed - rb.velocity.x;
        // 3. 根据方向判断加速减速 ( 可以设置不同的加减速率 )
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? moveAcceleration : moveDeceleration;
        // 4. 施加力进行移动
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, moveVelPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);
        
        #endregion
    
        if (Mathf.Abs(inputMovement.x) <= 0.001f && Mathf.Abs(rb.velocity.x) >= velocityCut)
        {
            float friction = physicsCheck.isGround ? groundFriction : airFriction;
            
            if (friction < Mathf.Abs(rb.velocity.x))
                rb.AddForce(Vector2.left * friction * Mathf.Sign(rb.velocity.x), ForceMode2D.Impulse);
            else
                rb.AddForce(Vector2.right * rb.velocity.x, ForceMode2D.Impulse);
            // Vector2 friction = Vector2.right * Mathf.Sign(-rb.velocity.x);

            // friction *= 

            // rb.AddForce(friction, ForceMode2D.Impulse);
        }

        if (isWind)
            rb.AddForce(Vector2.right * windFriction * Mathf.Sign(-rb.velocity.x), ForceMode2D.Impulse);    
    }

    IEnumerator FreezeInputController(int frameCount)
    {
        isFreezeInput = true;

        for(int i = 0; i < frameCount; i++)
            yield return new WaitForFixedUpdate();

        isFreezeInput = false;
    }

    private void NormalJump()
    {
        float jumpSpeed = normalJumpForce;

        jumpSpeed = (jumpSpeed + 0.5f * Time.fixedDeltaTime * -rb.gravityScale) / rb.mass;
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
    }

    private void StartDash()
    {
        float xInput = Mathf.Sign(inputMovement.x);
        float yInput = Mathf.Sign(inputMovement.y);

        if (Mathf.Abs(inputMovement.x) < dashXCut)
            xInput = 0f;
        if (Mathf.Abs(inputMovement.y) < dashYCut)
            yInput = 0f;

        Vector2 dashVelocity = new Vector2(xInput, yInput).normalized * dashForce;

        rb.velocity = dashVelocity;
        StartCoroutine(DashShadow());
        // ShadowPool.instance.GetFromPool();
    }

    IEnumerator DashShadow()
    {
        while (dashLeftTime > 0f)
        {
            ShadowPool.instance.GetFromPool();
            dashLeftTime -= Time.fixedDeltaTime;
        }

        yield return null;
    }

    private void UpdateCoyoteTimer()
    {
        coyoteTimer -= Time.deltaTime;

        if (coyoteTimer <= 0)
            coyoteTimer = -1;
    }

    private void SetPlayerStatement()
    {
        if (playerState == States.Jumping)
        {
            if (physicsCheck.isWall)
                playerState = States.Walling;
            if (physicsCheck.isGround)
            {
                playerState = States.None;
                canDoJump = true;
            }

                
        }
    }

    private void SetGravity()
    {
        if (rb.velocity.y < 0f)
            rb.gravityScale = fallingGravity;
        else if (rb.velocity.y >= 0f)
            rb.gravityScale = normalGravity;
        else if (playerState == States.Walling)
            rb.gravityScale = wallGravity;
    }

 

#region Enable
    void OnEnable()
    {
        inputControl.Enable();    
    }

    void OnDisable()
    {
        inputControl.Disable();
    }
#endregion

}
