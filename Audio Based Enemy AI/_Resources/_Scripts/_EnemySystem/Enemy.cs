using UnityEngine;
using UnityEngine.AI;

using System.Collections;
using System.Collections.Generic;

using DC.Scanner;
using Homebrew;
using Unity.VisualScripting;

//The enemy states
public enum EnemyState
{
    Patrolling,
    Alert,
    Chasing,
    Fleeing
};
public class Enemy : MonoBehaviour
{
    [Foldout("Enemy Settings",true)]

    [Tooltip("Current state of the enemy")]
    public EnemyState enemyState;

    [Tooltip("The enemy navmesh agent")]
    public NavMeshAgent Agent;

    [Tooltip("The enemy speed")]
    public float MoveSpeed;

    [Tooltip("The maximum distance at which the agent has reached the target")]
    public float MaxDist = 10;

    [Tooltip("Honestly, I recommend not changing the values in scene.")]
    public float MinDist = 5;

    [Tooltip("The indicator gameobject to show that the enemy is in chase mode")]
    public GameObject ChaseIndicator;

    [Tooltip("The indicator gameobject to show that the enemy is in alert mode")]
    public GameObject AlertIndicator;

    [Tooltip("The indicator gameobject to show that the enemy is in flee mode")]
    public GameObject FleeIndicator;
   
    [Foldout("Scanner Settings",true)]
    public TargetScanner targetScanner;
    public Transform Player;

    [Foldout("Patrol Settings",true)]

    [Tooltip("The patrol waypoints")]
    public List<Transform> PatrolWayPoints;

    [Foldout("Alert Settings",true)]

    [Tooltip("The waypoints followed in alert mode, assigned in runtime")]
    public List<Transform> AlertWayPoints;

    [Space(5)]
    [Tooltip("the distance the waypoints will be seperated by")]
    public float AlertWayPointDistance;

    [Tooltip("The length of the raycast that checks walls when spawning an alert waypoint")]
    public float WallCheckDistance = 5f;

    [Tooltip("Boolean flag used to indicate alert/wander mode, not meant to be changed")]
    public bool IsWandering;

    [Tooltip("Boolean flag used to indicate alert/wander mode, not meant to be changed")]
    public bool EnemyWasAlert;

    [Tooltip("Boolean flag used to indicate alert/wander mode, not meant to be changed")]
    public bool WanderToHighestSound;

    [Tooltip("The layer detected by the raycast as a wall/obstacle to not spawn a waypoint at")]
    public LayerMask WallCheckLayerMask;

    [Foldout("Chase & Hear Settings",true)]

    [Tooltip("The delay before the enemy returns to patrol")]
    public float ReturnToPatrolDelay = 2.5f;
   
    [Space(5)]

    [Tooltip("The gameobject assigned at runtime that is the sound object with highest value")]
    public GameObject HighestSoundObjectNoticed;

    [Tooltip("the value of the sound noticed")]
    public float HighestSoundNoticed;

    [Foldout("Flee Settings",true)]

    [Tooltip("the distance the enemy flees from the player")]
    public float fleeDistance;

    [Tooltip("boolean that indicates if the enemy is in flee mode, not meant to be changed")]
    public bool HasFledFromPlayer;

    [Tooltip("Layer of objects which are to be detected as SoundObjects")]
    public LayerMask SoundObjects;



    [Foldout("Debug Settings",true)]
    public Transform CurrentTarget;
    public Transform OldTarget;
    [Tooltip("Not meant to be changed but shows you in realtime, the index of the current patrol waypoint")]
    public int CurrentWaypointIndex;
    public bool RouteFinished;
    private int alertwaypointindex;
    


    void Start()
    {
        Agent.speed = MoveSpeed;
    }

    void Update()
    {
        if(ChaseIndicator != null)
        {
            ChaseIndicator.SetActive(enemyState == EnemyState.Chasing);
        }
        
        if(AlertIndicator != null)
        {
            AlertIndicator.SetActive(enemyState == EnemyState.Alert);
        }
        
        if(FleeIndicator != null)
        {
            FleeIndicator.SetActive(enemyState == EnemyState.Fleeing);
        }
        
        //the switch betw basic enemy logic takes place here
        switch (enemyState)
        {
            case EnemyState.Patrolling:
                PatrolBehavior();
                break;

            case EnemyState.Alert:
                AlertBehavior();

                if(!IsWandering)
                {
                    StartCoroutine(AlertWanderBehavior());
                }
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                break;

            case EnemyState.Fleeing:
                FleeFromPlayer();
                StartCoroutine(FleeTimedBehavior());
                break;
        }
    }

   // --------- Patrol Logic --------- //

    private void PatrolBehavior()
    {
        //normal view angle is 60
        //sets the current target to the waypoint to go to
        //increments the index of the list to go to the next waypoint when within a certain distance
        //also checks if the player is in sight while patrolling

            targetScanner.viewAngle = 60;
            CurrentTarget = PatrolWayPoints[CurrentWaypointIndex].transform;
            OldTarget = CurrentTarget;
            Agent.SetDestination(CurrentTarget.transform.position);

            //Vector3,Distance is too trippy to figure out what's going on, I adjusted some values and it works somehow
            //Basically makes sure to go to the next waypoint if in minimum distance to the current target waypoint
            if (Vector3.Distance(transform.position, CurrentTarget.position) >= MinDist)
            {
                SetEnemyState(0);            
            }

            else if(Vector3.Distance(transform.position, CurrentTarget.position) <= MaxDist)
            {
                //Debug.Log(CurrentWaypointIndex);

                if(CurrentWaypointIndex >= 0&&CurrentWaypointIndex != PatrolWayPoints.Count-1)
                {
                    CurrentWaypointIndex += 1;
                    RouteFinished = false;
                }
                else if(CurrentWaypointIndex == PatrolWayPoints.Count-1)
                {
                    if(!RouteFinished)
                    {
                        SetEnemyState(0);
                        CurrentWaypointIndex = 0;
                        RouteFinished = true;
                    }
                }
                 
            }

            CheckForPlayer();
        
    }


    // --------- Flee Logic --------- //

    Vector3 SoundPos;
    void FleeFromPlayer()
    {
        //finds a direction opposite to the highest sound noticed
        //replace with SoundPos with Player.transform.position to modify the flee direction
        //if the flee location is on the navmesh, flee from the player

        if(!HasFledFromPlayer)
        {
            if(HighestSoundObjectNoticed != null)
            {
                SoundPos = HighestSoundObjectNoticed.transform.position;
            } 

            Vector3 fleeDirection = (transform.position - SoundPos).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTarget, out hit, fleeDistance, NavMesh.AllAreas))
            {
                Agent.SetDestination(hit.position);
                HasFledFromPlayer = true;
            }

            
        }
       
    }

    

    IEnumerator FleeTimedBehavior()
    {
        //when the flee location is reached, return to patrol
        
        Debug.Log("Checking if reached!");
        CheckIfEnemyReachedSpot();

        if(ReachedSpot)
        {
            yield return new WaitForSeconds(ReturnToPatrolDelay);

            enemyState = EnemyState.Patrolling;
            HasFledFromPlayer = false;
            ClearAlertVariables();
            ReachedSpot = false;
            yield break;
        }
        

    }


    // --------- Alert Logic --------- //

    private void AlertBehavior()
    {
        //runs in update so once the sound object triggers the alert state this takes effect
        //this is triggered by a sound object calling the ListenBehavior()
        //that sets the enemy state to alert
        //here, the sound value is compared, if it is > 0.6, the fov narrows to 15
        //if the sound value is less than 0.6, fov widens to 120
        //the enemy can detect the player while in alert mode as i am calling the CheckForPlayer() here
        //The enemy then shoots a raycast in 4 directions and checks if it hits no walls
        //if it hits no walls, spawn an alert waypoint there and add it to the list
        //if it does, do not spawn an alert way point
        //then set the enemy destination to the highest sound object's position

        if(enemyState == EnemyState.Alert)
        {
            if(HighestSoundNoticed > 0.6f)
            {
                targetScanner.viewAngle = 15f;
            }
            else
            {
                targetScanner.viewAngle = 120;
            }
            
            CheckForPlayer();
            
            //make sure the highest sound object noticed is not null
            if(HighestSoundObjectNoticed != null)
            {
                //bool check to make sure following code runs only once
                
                if(!WanderToHighestSound)
                {
                    //set agent destination to highest sound obj noticed
                    //even if atp it is deleted, it is fine as setdestination() needs only vector coordinates
                    
                    CheckAlertWaypointDirection(transform.forward);
                    CheckAlertWaypointDirection(-transform.forward);
                    CheckAlertWaypointDirection(transform.right);
                    CheckAlertWaypointDirection(-transform.right);
                    
                
                    Agent.SetDestination(HighestSoundObjectNoticed.transform.position);

                    WanderToHighestSound = true;
                }
               
                
                
            }
            
        }
        
    }

    IEnumerator AlertWanderBehavior()
    {
        Debug.Log("Checking if reached!");
    
        while (true)  // This ensures continuous checking
        {
            IsWandering = true;

            //CurrentTarget = AlertWayPoints[alertwaypointindex];
            Agent.SetDestination(AlertWayPoints[alertwaypointindex].transform.position);

            // Wait until the enemy reaches the current waypoint
            while (Vector3.Distance(transform.position, AlertWayPoints[alertwaypointindex].transform.position) > MaxDist)
            {
                yield return null;  // Wait for the next frame before checking again
            }

            // Wait for 1 second after reaching the waypoint
            yield return new WaitForSeconds(2.5f);
            
            alertwaypointindex++;      
            
            if (alertwaypointindex >= AlertWayPoints.Count)  // Check if the index exceeds the array length
            {
                break;  
            }
        
        }
        
        yield return new WaitForSeconds(2.5f);

        enemyState = EnemyState.Patrolling;

        ClearAlertVariables();
        WanderToHighestSound = false;
        yield break;
        
    }

    private void ClearAlertVariables()
    {
        //add a destroy script to the objects
        foreach(Transform obj in AlertWayPoints)
        {
            obj.AddComponent<DestroyObj>();
        }

        //reset the alert waypoints and all alert related flags
        AlertWayPoints.Clear();
        alertwaypointindex = 0;
        EnemyWasAlert = false;
        IsWandering = false;
    }

    //simpler alert behavior, no bugs, stays at highest sound spot and returns to patrol
    //call this instead if you'd like
    IEnumerator AlertWanderBehaviorBackup()
    {
        Debug.Log("Checking if reached!");
        CheckIfEnemyReachedSpot();

        if(ReachedSpot)
        {
            yield return new WaitForSeconds(ReturnToPatrolDelay);

            enemyState = EnemyState.Patrolling;
            EnemyWasAlert = false;
            WanderToHighestSound = false;
            ReachedSpot = false;
            yield break;
        }
        
    }

    // --------- Enemy Checks --------- //
    bool ReachedSpot = false;
    private void CheckIfEnemyReachedSpot()
    {
        if (!Agent.pathPending) // Check if the path is computed
        {
            if (Agent.remainingDistance <= Agent.stoppingDistance) // Check if the agent is close enough to the end of the path
            {
                if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f) // Check if the agent has stopped moving
                {
                    Debug.Log("Reached Spot!");
                    ReachedSpot = true;
                }
            }
        }
    }

    private void CheckForPlayer()
    {
        //the asset im using does not have a direct method of finding if there are any targets in view
        //but its my best bet and shaves off so much development time
        if(targetScanner.GetTargetList() != null)
        {
           
            enemyState = EnemyState.Chasing;
            //CurrentTarget = Player;
        }       
        else
        {
            //if the enemy was once alert and noticed you
            //it sets the enemywasalert flag
            //in this case, go back to patrol

            if(EnemyWasAlert)
            {
                enemyState = EnemyState.Alert;
                    
            }
            else
            {
                enemyState = EnemyState.Patrolling;
            }
            
        }

    }

    private void ChasePlayer()
    {
        CurrentTarget = Player;
        Agent.SetDestination(CurrentTarget.transform.position);
        CheckForPlayer();
    }

    RaycastHit hit;
    private void CheckAlertWaypointDirection(Vector3 direction)
    {
        
        if (!Physics.Raycast(transform.position, direction, out hit, WallCheckDistance, WallCheckLayerMask))
        {
            // No hit, instantiate empty GameObject
            Vector3 spawnPosition = transform.position + direction * WallCheckDistance;
            GameObject alertObject = new GameObject("AlertWaypoint");

            alertObject.transform.position = spawnPosition;

            //add a gizmo script to the waypoints so you can see it in scene view
            alertObject.AddComponent<EnemyWaypoints>();
            EnemyWaypoints WaypointGizmo = alertObject.GetComponent<EnemyWaypoints>(); 
            WaypointGizmo.WayPointColor = new Color(255,0,222,255);
            WaypointGizmo.GizmoRadius = 1;

            //add them to the list
            AlertWayPoints.Add(alertObject.transform);
            
           
        }
    }
    
    // --------- Public Methods --------- //
    
    public void ListenBehavior(GameObject SoundObject, float SoundNoticed)
    {
        //set enemy state to alert and current target to highest sound noticed

        HighestSoundObjectNoticed = SoundObject;
        HighestSoundNoticed = SoundNoticed;

        //in hindsight i do not remember why i put this check here since i check in the SoundObject script anyways
        // but the idea is to only call alert if within this range
        if(HighestSoundNoticed >= 0.2f && HighestSoundNoticed <= 0.6f)
        {
            enemyState = EnemyState.Alert;
            CurrentTarget = HighestSoundObjectNoticed.transform;
        }
       
    }

    
    public void SetEnemyState(int i)
    {
        enemyState = (EnemyState)i;
    }

    private void OnDrawGizmos() 
    {
        targetScanner.ShowGizmos();
    }

    
}
