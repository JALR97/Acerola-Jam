using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Pathfinding;
using UnityEngine.Serialization;

[SuppressMessage("ReSharper", "BadControlBracesIndent")]
public class SnarkBehavior : MonoBehaviour
{
    //Structures
    private enum SnarkState{
        STAND_BY,
        STALK,
        PREP,
        HIDE,
        FIGHT
    }
    
    //Components
    [SerializeField] private Seeker seeker;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerBack;
    [SerializeField] private Animator _animator;
    
    [SerializeField] private Collider2D dashCollider;
    [SerializeField] private Collider2D normalCollider;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    //Balance
    [SerializeField] private float stalkSpeed;
    [SerializeField] private float hideSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float dashSpeed = 3000;
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackcooldown;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashStopTime = 1.5f;
    
    [SerializeField] private float fadeDistance = 20f;
    
    [SerializeField] private float aggressiveTime;
    [SerializeField] private float stalkLimitTime;
    [SerializeField] private float hideTime = 3;
    [SerializeField] private float pathingCallTime = 0.5f;
    
    [SerializeField] private float inactiveDistance;
    [FormerlySerializedAs("attackDistance")] [SerializeField] private float aggroDistance;
    [SerializeField] private float stalkCloseDistance;
    //Internal
    private bool seenOnce = false;
    private SnarkState currentState = SnarkState.STAND_BY;
    private SnarkState prevState = SnarkState.STAND_BY;//debug var
    private float minDistanceToWaypoint = 3.0f;
    private float distanceToPlayer;
    private Path path = null;
    private int currentWaypoint = 0;
    private float prepTimer = 0;
    private bool dashing = false;
    private float timer = 0;
    private float fadeProp = 0;

    private bool seen = false;
    private IEnumerator  tempCoroutine;
    //private bool endOfPath = false;
    
    //Functions
    public void Seen() {
        seen = true;
        if (!seenOnce && (currentState == SnarkState.STALK || currentState == SnarkState.PREP)) {
            currentState = SnarkState.HIDE;
            Invoke("Reasses", hideTime);
            seenOnce = true;
        }else if (seenOnce && (currentState == SnarkState.STALK || currentState == SnarkState.PREP)) {
            currentState = SnarkState.FIGHT;
            tempCoroutine = PathingCall(SnarkState.FIGHT, player);
            StartCoroutine(tempCoroutine);
        }
    }

    public void UnSeen() {
        seen = false;
    }
    private void Reasses() {
        if (distanceToPlayer <= stalkCloseDistance) {
            currentState = SnarkState.FIGHT;
            path = null;
            tempCoroutine = PathingCall(SnarkState.FIGHT, player);
            StartCoroutine(tempCoroutine);
        }
        else {
            currentState = SnarkState.STALK;
            path = null;
            tempCoroutine = PathingCall(SnarkState.STALK, playerBack);
            StartCoroutine(tempCoroutine);
        }
    }
    
    void OnPathComplete(Path p) {
        if (!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void Start() {
        //If seeker.isDone() -- Make sure not to start a path before the prev is done
        //seeker.StartPath(rb.position, player.position, OnPathComplete);
        
    }
    IEnumerator PathingCall(SnarkState startingState, Transform target) {
        while (currentState == startingState) {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
            yield return new WaitForSeconds(pathingCallTime);
        }
    }

    private void Update() {
        if (seen) {
            fadeProp = 1;
        }
        else {
            if (distanceToPlayer <= aggroDistance) {
                fadeProp = 1;
            }
            else if(distanceToPlayer >= fadeDistance) {
                fadeProp = 0;
            }
            else {
                fadeProp = 1 - (distanceToPlayer - aggroDistance) / (fadeDistance - aggroDistance);
            }
        }
        Color tempCol = spriteRenderer.color;
        tempCol.a = fadeProp;
        spriteRenderer.color = tempCol;
    }

    private void FixedUpdate() {
        //State change conditions
        distanceToPlayer = Vector2.Distance(player.position, transform.position);
        if (distanceToPlayer <= inactiveDistance && currentState == SnarkState.STAND_BY) {
            currentState = SnarkState.STALK;
            tempCoroutine = PathingCall(SnarkState.STALK, playerBack);
            StartCoroutine(tempCoroutine);
        }else if (distanceToPlayer <= aggroDistance && currentState != SnarkState.FIGHT) {
            currentState = SnarkState.FIGHT;
            path = null;
            tempCoroutine = PathingCall(SnarkState.FIGHT, player);
            StartCoroutine(tempCoroutine);
        }else if (distanceToPlayer <= stalkCloseDistance && currentState == SnarkState.STALK) {
            currentState = SnarkState.PREP;
            path = null;
        }
        
        //Movement section based on state
        var position = rb.position;
        //Stalk
        if (currentState == SnarkState.STALK && path != null) {
            Vector2 pathDirection = ((Vector2)path.vectorPath[currentWaypoint] - position).normalized;
            //rb.AddForce(pathDirection * (Time.fixedDeltaTime * stalkSpeed), ForceMode2D.Force);
            rb.velocity = pathDirection * (Time.fixedDeltaTime * stalkSpeed);
            
        //Prep
        }else if (currentState == SnarkState.PREP) {
            Vector2 awayFromPlayer;
            if (distanceToPlayer < stalkCloseDistance) {
                awayFromPlayer = (rb.position - (Vector2)player.position).normalized;
            }
            else {
                awayFromPlayer = ((Vector2)player.position - rb.position).normalized;
            }
            rb.AddForce(awayFromPlayer * (Time.fixedDeltaTime * hideSpeed), ForceMode2D.Force);
            //rb.velocity = awayFromPlayer * (Time.fixedDeltaTime * hideSpeed);
            prepTimer += Time.fixedDeltaTime;
            if (prepTimer >= stalkLimitTime) {
                currentState = SnarkState.FIGHT;
                path = null;
                tempCoroutine = PathingCall(SnarkState.FIGHT, player);
                StartCoroutine(tempCoroutine);
            } 
            
        //Fight
        }else if (currentState == SnarkState.FIGHT && path != null) {
            timer += Time.deltaTime;
            if (!dashing && timer >= attackcooldown) {
                if (distanceToPlayer <= aggroDistance) {
                    StartDash();
                    rb.velocity = Vector2.zero;
                }
                else {
                    Vector2 pathDirection = ((Vector2)path.vectorPath[currentWaypoint] - position).normalized;
                    rb.velocity = pathDirection * (Time.fixedDeltaTime * attackSpeed);
                }
            }
            else if(dashing) {
                if (timer >= dashTime) {
                    Vector2 dashDirection = ((Vector2)player.position - position).normalized;
                    rb.velocity = dashDirection * (Time.fixedDeltaTime * dashSpeed);
                    dashing = false;
                    _animator.CrossFade("SnarkWalk", 2, 0);
                    timer = 0;
                }
            }else if (!dashing && timer < attackcooldown && timer >= dashStopTime) {
                rb.velocity = Vector2.zero;
                dashCollider.enabled = false;
                normalCollider.enabled = true;
            }
            
            
        //Hide
        }else if (currentState == SnarkState.HIDE) {
            var awayFromPlayer = (rb.position - (Vector2)player.position).normalized;
            //rb.AddForce(awayFromPlayer * (Time.fixedDeltaTime * hideSpeed), ForceMode2D.Force);
            rb.velocity = awayFromPlayer * (Time.fixedDeltaTime * hideSpeed);
            return;
        }
        
        //check path progress
        if (path == null) {
            return;
        }
        float distance = Vector2.Distance(path.vectorPath[currentWaypoint], position);
        if (distance <= minDistanceToWaypoint) {
            currentWaypoint = Mathf.Min(currentWaypoint + 1, path.vectorPath.Count - 1);
        }
    }
    

    private void StartDash() {
        dashCollider.enabled = true;
        normalCollider.enabled = false;
        _animator.CrossFade("SnarkHorn", 0, 0);
        timer = 0;
        dashing = true;
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.CompareTag("Player")) {
            col.GetComponent<Health>().Damage(attackDamage);
        }
    }

    //Gizmos - Debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, inactiveDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stalkCloseDistance);
    }
}
