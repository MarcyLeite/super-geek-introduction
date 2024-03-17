using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Personagem : MonoBehaviour
{
    private float horizontal;
    private bool isFacingRight = true;
    private int jumpCount = 0;
    private bool isAirOnHook = false;
    private int hookFrameCounter = -1;

    private List<GameObject> passHookList = new();
    [SerializeField] private float jumpingPower = 16f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private int totalJump = 2;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask hookLayer;
    [SerializeField] private LineRenderer hookLine;
    [SerializeField] private float hookRange = 20f;
    void Start() {
        hookLine.enabled = false;
    }
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("speed", Math.Abs(horizontal));
        animator.SetBool("inAir", !IsGrounded());
        animator.SetFloat("ySpeed", rigidBody.velocity.y);
        Flip();

        if (IsGrounded() && rigidBody.velocity.y < 0.5f) {
            jumpCount = 0;
            isAirOnHook = false;
            foreach (GameObject passHook in passHookList) {
                Collider2D pointHookCollider = passHook.GetComponent<Collider2D>();
                pointHookCollider.enabled = true;
            }
            passHookList = new();
        }

        if (Input.GetButtonDown("Jump") && (IsGrounded() || jumpCount < totalJump))
        {
            isAirOnHook = false;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpingPower);
            jumpCount++;
        }

        if (Input.GetButtonUp("Jump") && rigidBody.velocity.y > 0f && !isAirOnHook)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.5f);
        }
        GameObject closestHook = getClosestHook();
        if (Input.GetButtonDown("Fire3") && closestHook) {
            jumpCount = totalJump - 1;
            isAirOnHook = true;
            Vector2 direction = Vector3.Normalize(closestHook.transform.position - gameObject.transform.position);
            rigidBody.velocity = direction * 30f;
            Collider2D hookPointCollider = closestHook.GetComponent<Collider2D>();
            hookPointCollider.enabled = false;

            HookScript hs = closestHook.GetComponent<HookScript>();
            hs.ToggleTarget(false);

            hookLine.enabled = true;
            hookFrameCounter = 0;
            hookLine.SetPosition(1, closestHook.transform.position);

            passHookList.Add(closestHook);
        }
    }
    void FixedUpdate() {
        if (hookFrameCounter > -1) {
            hookLine.SetPosition(0, transform.position);
            hookFrameCounter++;
        }
        if(hookFrameCounter == 20) {
            hookLine.enabled = false;
            hookFrameCounter = -1;
        }
        if (!isAirOnHook) {
            rigidBody.velocity = new Vector2(horizontal * speed, rigidBody.velocity.y);
        } else {
            float xVelocity = 0;
            if (rigidBody.velocity.x / Math.Abs(rigidBody.velocity.x) != horizontal) {
                xVelocity = horizontal * speed / 2;
            }
            else if (Math.Abs(rigidBody.velocity.x) < speed) {
                xVelocity = (speed - Math.Abs(rigidBody.velocity.x)) * horizontal;
            }
            rigidBody.velocity = new Vector2(rigidBody.velocity.x + xVelocity, rigidBody.velocity.y);
        }
    }

    bool IsGrounded() {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    GameObject getClosestHook() {
        Collider2D[] closestHookList = Physics2D.OverlapCircleAll(gameObject.transform.position, hookRange, hookLayer);
        if(closestHookList.Count() == 0) return null;
        GameObject closestHook = closestHookList[0].gameObject;
        foreach (Collider2D hookTest in closestHookList) {
            GameObject hookGm = hookTest.gameObject;
            HookScript hsTest = hookGm.GetComponent<HookScript>();
            hsTest.ToggleTarget(false);

            float testDistance = Vector2.Distance(gameObject.transform.position, hookTest.transform.position);
            float closestDistance = Vector2.Distance(gameObject.transform.position, closestHook.transform.position);
            if (testDistance < closestDistance) {
                closestHook = hookGm;
            }
        }

            HookScript hs = closestHook.GetComponent<HookScript>();
            hs.ToggleTarget(true);

        return closestHook;
    }

    void Flip() {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f) {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}

