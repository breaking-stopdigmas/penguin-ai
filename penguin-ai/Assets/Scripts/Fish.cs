using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour
{
    public float fishSpeed;
    private float randomizedSpeed = 0f;
    private float nextActionTime = -1f;
    private Vector3 targetPosition;


    private void FixedUpdate()
    {
        if (fishSpeed > 0f)
        {
            Swim();
        }
    }


    private void Swim()
    {
        if (Time.fixedTime >= nextActionTime)
        {
            CalculateWaypoint();
        }
        else 
        {
            GoToWaypoint();
        }
    }


    private void CalculateWaypoint()
    {
        // Randomize the speed
        float speedVariance = UnityEngine.Random.Range(.5f, 1.5f);
        randomizedSpeed = fishSpeed * speedVariance;

        float minAngle = 100f, maxAngle = 260f;
        float minRadius = 2f, maxRadius = 13f;
        // Pick a random target
        targetPosition = PenguinArea.ChooseRandomPosition(transform.parent.position, minAngle, maxAngle, minRadius, maxRadius);

        // rortate towards the target
        transform.rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

        // calculate time to get there
        float timeToGetThere = Vector3.Distance(transform.position, targetPosition) / randomizedSpeed;

        nextActionTime = Time.fixedTime + timeToGetThere;
    }


    private void GoToWaypoint()
    {
        // make sure that the fish does not swim past the target (nor the boundaries of the arena?)
        Vector3 moveVector = randomizedSpeed * transform.forward * Time.fixedDeltaTime;
        float lengthOfMove = moveVector.magnitude;
        float distToWaypoint = Vector3.Distance(transform.position, targetPosition);

        if (lengthOfMove <= distToWaypoint)
        {
            transform.position += moveVector;
        }
        else
        {
            transform.position = targetPosition;
            nextActionTime = Time.fixedTime;
        }
    }
}
