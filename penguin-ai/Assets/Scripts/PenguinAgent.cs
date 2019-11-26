using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class PenguinAgent : Agent
{
    public GameObject heartPrefab;
    public GameObject regurgitatedFishPrefab;

    private PenguinArea penguinArea;
    private Animator animator;
    private RayPerception3D rayPerception;
    private GameObject baby;
    Renderer m_GroundRenderer;

    private int fishes_eatened = 0;
    private bool isFull;

    /*
        possible actions = move forward, dont move
                           turn left, turn right, dont turn
    */                        
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // convert actions to axis values (that can actually be used to control the agent)
        float forward = vectorAction[0];
        float leftOrRight = 0f;
        if (vectorAction[1] == 1f)
        {
            leftOrRight = -1f;
        }
        else if (vectorAction[1] == 2f)
        {
            leftOrRight = 1f;
        }

        // set animator parameters [move by root motion]
        animator.SetFloat("Vertical", forward);
        animator.SetFloat("Horizontal", leftOrRight);

        // chasten every step
        AddReward(-1f/agentParameters.maxStep);
    }


    public override void AgentReset()
    {
        isFull = false;
        fishes_eatened = 0;
        penguinArea.ResetArea();
    }


    private float GetDistanceToBaby()
    {
        return Vector3.Distance(baby.transform.position, transform.position);
    }


    public override void CollectObservations()
    {
        var distanceToBaby = GetDistanceToBaby();
        var directionToBaby = (baby.transform.position - transform.position).normalized;
        var directionPenguinIsLooking = transform.forward;
        AddVectorObs(distanceToBaby);
        AddVectorObs(directionToBaby);
        AddVectorObs(directionPenguinIsLooking);
        // has the penguin eaten?
        AddVectorObs(isFull);

        // RayPeception (sight)
        // ====================
        // rayDistance : how far to raycast 
        // rayAngles : angles to raycast [0 = right, 90 = forward, 180 = left]
        // detectableObjects :  list of g.o. tags that the agent can see
        // startOffset : starting height from center of agent
        // endOffset : ending height from center of agent

        float rayDistance = 20f;
        float[] rayAngles = { 30f, 60f, 90f, 120f, 150f };
        string[] detectableObjects = { "baby", "fish", "wall" };
        float startOffset = 0f;
        float endOffset = 0f;

        AddVectorObs(rayPerception.Perceive(rayDistance, rayAngles, detectableObjects, startOffset, endOffset));
    }


    private void Start()
    {
        penguinArea = GetComponentInParent<PenguinArea>();
        baby = penguinArea.penguinBaby;
        animator = GetComponent<Animator>();
        rayPerception = GetComponent<RayPerception3D>();
    }


    private void FixedUpdate() {
        if (GetDistanceToBaby() < penguinArea.feedRadius)
        {
            // close enough, try to feed the baby
            RegurgitateFish();
        }
    }


    private void OnCollisionEnter(Collision col)
    {
        if (col.transform.CompareTag("fish"))
        {
            EatFish(col.gameObject);
        }
        else if (col.transform.CompareTag("baby"))
        {
            RegurgitateFish();
        }        
        else if (col.transform.CompareTag("wall"))
        {
            AddReward(-1f/agentParameters.maxStep);
        }
    }


    private void EatFish(GameObject fishObject)
    {
        if (isFull) return; 
        isFull = true;

        penguinArea.RemoveSpecificFish(fishObject);
        fishes_eatened++;
        AddReward(1f);
    }


    private void RegurgitateFish()
    {
        if (!isFull) return;
        isFull = false;
        AddReward(2f);
        if (fishes_eatened >= penguinArea.fishSpawnQuantity)
        {
            Done();
            AddReward(3f);
            return;
        }

        spawnPrefab(regurgitatedFishPrefab, Vector3.zero);
        spawnPrefab(regurgitatedFishPrefab, Vector3.up);
    }


    private void spawnPrefab(GameObject prefab, Vector3 tilt)
    {
        GameObject go = Instantiate<GameObject>(prefab);
        go.transform.parent = transform.parent; // Penguin Area
        go.transform.position = baby.transform.position + tilt;
        Destroy(go, 4f);
    }


    public override float[] Heuristic()
    {
        float[] playerInput = { 0f, 0f };

        if (Input.GetKey(KeyCode.W))
        {
            playerInput[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerInput[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerInput[1] = 2;
        }

        return playerInput;
    }
}
