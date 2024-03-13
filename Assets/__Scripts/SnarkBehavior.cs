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
    
    //Balance
    [SerializeField] private float stalkSpeed;
    [SerializeField] private float hideSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDamage;

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

    private IEnumerator  tempCoroutine;
    //private bool endOfPath = false;
    
    //Functions
    public void Seen() {
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
        //debug
        if (prevState != currentState) {
            prevState = currentState;
            Debug.Log(currentState);    
        }
        
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
                Debug.Log("Away");
            }
            else {
                awayFromPlayer = ((Vector2)player.position - rb.position).normalized;
                Debug.Log("Towards");
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
            Vector2 pathDirection = ((Vector2)path.vectorPath[currentWaypoint] - position).normalized;
            //rb.AddForce(pathDirection * (Time.fixedDeltaTime * attackSpeed), ForceMode2D.Force);
            rb.velocity = pathDirection * (Time.fixedDeltaTime * attackSpeed);
            
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
