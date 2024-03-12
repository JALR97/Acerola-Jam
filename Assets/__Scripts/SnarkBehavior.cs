using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SnarkBehavior : MonoBehaviour
{
    //Structures
    private enum SnarkState{
        STAND_BY,
        STALK,
        HIDE,
        FIGHT
    }
    
    //Components
    [SerializeField] private Seeker seeker;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerBack;
    
    //Balance
    [SerializeField] private float roamSpeed;
    [SerializeField] private float stalkSpeed;
    [SerializeField] private float hideSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDamage;

    [SerializeField] private float aggressiveTime;
    [SerializeField] private float stalkLimitTime;
    
    [SerializeField] private float inactiveDistance;
    [SerializeField] private float attackDistance;
    [SerializeField] private float stalkCloseDistance;
    //Internal
    private SnarkState currentState = SnarkState.STAND_BY;
    private float minDistanceToWaypoint = 3.0f;
    private float distanceToPlayer;
    private Path path;
    private int currentWaypoint = 0;
    private bool endOfPath = false;
    
    //Functions
    public void Seen() {
        if (currentState == SnarkState.STALK) {
            currentState = SnarkState.HIDE;
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

    private void FixedUpdate() {
        distanceToPlayer = Vector2.Distance(player.position, transform.position);
        if (distanceToPlayer <= inactiveDistance && currentState == SnarkState.STAND_BY) {
            currentState = SnarkState.STALK;
            seeker.StartPath(rb.position, player.position, OnPathComplete);
        }
        else if (distanceToPlayer > inactiveDistance) {
            currentState = SnarkState.STAND_BY;
            path = null;
        }
        
        if (path == null) {
            return; //no path
        }

        if (currentWaypoint >= path.vectorPath.Count) {
            endOfPath = true;
            return;
        }else
            endOfPath = false;
        
        //Where we should be moving to according to path
        var position = rb.position;
        Vector2 pathDirection = ((Vector2)path.vectorPath[currentWaypoint] - position).normalized;
        //Movement might go here
        //
        
        //check path progress
        float distance = Vector2.Distance(path.vectorPath[currentWaypoint], position);
        if (distance <= minDistanceToWaypoint) {
            currentWaypoint++;
        }
    }
}
