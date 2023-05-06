// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ObstacleAgent))]
public class NPCController : MonoBehaviour
{
    public float movementRange = 2f;
    public ObstacleAgent agent;

    public FlowMap flow;

    private bool isMoving = false;
    private Vector3 destination;
    private float waitTime;
    public float minWaitTime = 15f;
    public float maxWaitTime = 30f;

    private float OriginalY;

    public bool isIdle = false;
    public int patience;

    public float purchaseChance = 0.7f;
    public float purchaseTime = 10f;
    public float crossChance = 0.5f;
    private float minCrossChance = 0.1f;
    public float visitFountainChance = 0.4f;
    public float visitBChance = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        OriginalY = transform.position.y;
        waitTime = Random.Range(0f, 30f);

        patience = Random.Range(0, 99);
        // patience = 99;
        agent.SetPriority(patience);

        flow = GameObject.Find("FlowMap").GetComponent<FlowMap>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (isIdle)
        {   
            // add an sphere on top of the NPC
            return;
        }

        if (transform.position.y != OriginalY)
        {
            transform.position = new Vector3(transform.position.x, OriginalY, transform.position.z);
        }

        if (waitTime > 0f)
        {   
            waitTime -= Time.deltaTime;
        }
        else 
        {   
            // Have a chance of purchasing an item, or just wander.
            if (Roll(purchaseChance) && !flow.isMarketFull())
            {
                // purchase an item
                destination = PurchaseItem(transform.position);
            }
            else
            {
                // wander
                destination = Wander(transform.position);
            }

            if (destination == Vector3.zero) return;

            // check if the destination is on the NavMesh
            NavMeshHit hit;
            bool inNavMesh = NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas);
            
            if (inNavMesh)
            {   
                agent.SetDestination(hit.position);

                // wait a bit before moving again
                waitTime = Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }

    Vector3 Wander(Vector3 curPos)
    {
        int playerLoc = agent.GetAreaCode(curPos);
        int destCode;

        // scale cross chance based on market fullness
        int numMarket = flow.marketNPCs;
        crossChance = GetMin(crossChance - (numMarket * 0.1f), minCrossChance);

        // roll chances
        bool willCross = Roll(crossChance);
        bool willVisitFountain = Roll(visitFountainChance);
        bool willVisitB = Roll(visitBChance);

        switch (playerLoc)
        {
            case 0:
                // in A
                if (willCross || (willVisitB && !flow.isMarketFull()))
                {
                    destCode = 1;
                }
                else
                {
                    destCode = 0;
                }
                break;
            case 1:
                // in B
                if (willCross)
                {
                    destCode = 0;
                }
                else if (willVisitFountain)
                {
                    destCode = 4;
                }
                else
                {
                    destCode = 1;
                }
                break;
            case 2:
                // in alley
                destCode = -1;
                break;
            case 3:
                // in market
                if (willVisitB)
                {
                    destCode = 1;
                }
                else
                {
                    destCode = 0;
                }
                break;
            default:
                destCode = -1;
                break;
        }
        if (destCode == -1)
        {
            return agent.destination;
        }
        else
        {
            return agent.GetRandomAtArea(destCode);
        }
    }

    Vector3 PurchaseItem(Vector3 curPos)
    {
        // if the npc is outside the market area, move to the market and add 5 seconds to the wait time
        Vector3 topLeftMarket = new Vector3(2.5f, 0f, 5.5f);
        Vector3 bottomRightMarket = new Vector3(6.5f, 0f, 1.5f);

        if (curPos.x < topLeftMarket.x || curPos.x > bottomRightMarket.x || curPos.z > topLeftMarket.z || curPos.z < bottomRightMarket.z)
        {
            waitTime += purchaseTime;
            return new Vector3(Random.Range(topLeftMarket.x, bottomRightMarket.x), 0, Random.Range(topLeftMarket.z, bottomRightMarket.z));
        }

        return Vector3.zero;
    }

    private bool IsInMarketArea(Vector3 Position)
    {
        Vector3 topLeftMarket = new Vector3(2.5f, 0f, 5.5f);
        Vector3 bottomRightMarket = new Vector3(6.5f, 0f, 1.5f);

        if (Position.x >= topLeftMarket.x && Position.x <= bottomRightMarket.x && Position.z >= bottomRightMarket.z && Position.z <= topLeftMarket.z)
        {
            return true;
        }
        return false;
    }

    private float GetMin(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        return b;
    }

    private float GetMax(float a, float b)
    {
        if (a > b)
        {
            return a;
        }
        return b;
    }

    private bool Roll(float chance)
    {
        float roll = Random.Range(0f, 1f);
        return (roll < chance);
    }
}
