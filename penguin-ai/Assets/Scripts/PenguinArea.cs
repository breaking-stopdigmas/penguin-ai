using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using TMPro;

public class PenguinArea : Area
{
    public PenguinAgent penguinAgent;
    public GameObject penguinBaby;
    public Fish fishPrefab;
    public TextMeshPro cumulativeRewardText;
    public int fishSpawnQuantity = 4;
    public Material successMaterial;
    public Material snowMat;

    [HideInInspector]
    public float fishSpeed = 8f;
    [HideInInspector]
    public float feedRadius = 1f;

    private List<GameObject> fishList;


    public override void ResetArea()
    {
        RemoveAllFish();
        PlacePenguin();
        PlaceBaby();
        SpawnFish(fishSpawnQuantity, fishSpeed);
        StartCoroutine(NoMoreFishSwapGroundMaterial(0.5f));
    }

    IEnumerator NoMoreFishSwapGroundMaterial(float time)
    {

        var snows = GameObject.FindGameObjectsWithTag("snow");
        foreach (GameObject snow_obj in snows)
        {
            snow_obj.GetComponent<Renderer>().material = successMaterial;
        }
        yield return new WaitForSeconds(time); // Wait for 2 sec
        foreach (GameObject snow_obj in snows)
        {
            snow_obj.GetComponent<Renderer>().material = snowMat;
        }
    }

    public void RemoveSpecificFish(GameObject fishObject)
    {
        fishList.Remove(fishObject);
        Destroy(fishObject);
    }

    // used in fish script too
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = minRadius;

        if (maxRadius > minRadius)
        {
            radius = UnityEngine.Random.Range(minRadius, maxRadius);
        }
        float randomAngle = UnityEngine.Random.Range(minAngle, maxAngle);

        return center + Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward * radius;
    }

    // TODO
    // object pool? destroy = too costly
    private void RemoveAllFish()
    {
        if (fishList != null)
        {
            for (int i = 0; i < fishList.Count; ++i)
            {
                if (fishList[i] != null)
                {
                    Destroy(fishList[i]);
                }
            }
        }

        fishList = new List<GameObject>(); // or clear()?
    }


    private void PlacePenguin()
    {
        float minAngle = 0f, maxAngle = 360f;
        float minRadius = 0f, maxRadius = 9f;

        penguinAgent.transform.position = ChooseRandomPosition(transform.position, minAngle, maxAngle, minRadius, maxRadius) + Vector3.up * .5f; // prevent from spawning underneath the earth, so it will fall into the ground

        float randomAngle = UnityEngine.Random.Range(minAngle, maxAngle);
        penguinAgent.transform.rotation = Quaternion.Euler(0f, randomAngle, 0f);
    }


    private void PlaceBaby()
    {
        float minAngle = -45f, maxAngle = 45f;
        float minRadius = 4f, maxRadius = 9f;

        penguinBaby.transform.position = ChooseRandomPosition(transform.position, minAngle, maxAngle, minRadius, maxRadius) + Vector3.up * .5f; // prevent from spawning underneath the earth, so it will fall into the ground

        penguinBaby.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }


    private void SpawnFish(int fishSpawnQuantity, float fishSpeed)
    {
        for (int i = 0; i < fishSpawnQuantity; ++i)
        {
            GameObject fishObject = Instantiate<GameObject>(fishPrefab.gameObject);
            float minAngle = 100f, maxAngle = 260f;
            float minRadius = 2f, maxRadius = 13f;

            fishObject.transform.position = ChooseRandomPosition(transform.position, minAngle, maxAngle, minRadius, maxRadius);
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            fishObject.transform.rotation = Quaternion.Euler(0f, randomAngle, 0f);
            fishObject.transform.parent = transform;

            fishList.Add(fishObject);
            fishObject.GetComponent<Fish>().fishSpeed = fishSpeed;
        }
    }


    private void SetCumulativeRewardText()
    {
        cumulativeRewardText.text = penguinAgent.GetCumulativeReward().ToString("0.000");
    }


    private void Update()
    {
        SetCumulativeRewardText();
    }

}
