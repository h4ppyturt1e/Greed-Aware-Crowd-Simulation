using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent)), 
 RequireComponent(typeof(NavMeshObstacle)), 
 RequireComponent(typeof(ThirdPersonCharacter)),
 RequireComponent(typeof(FlowMap)),
 RequireComponent(typeof(GameObject))]
public class ObstacleAgent : MonoBehaviour
{
    [SerializeField]
    private float CarvingTime = 0.5f;
    [SerializeField]
    private float CarvingMoveThreshold = 0.1f;

    public NavMeshAgent Agent;
    public NavMeshObstacle Obstacle;
    public ThirdPersonCharacter Character;

    public FlowMap Flow;
    public GameObject MarkerSphere;

    private float LastMoveTime;
    private Vector3 LastPosition;

    public bool isMoving = false;
    public Vector3 destination = Vector3.zero;
    
    private int OriginalPriority = 0;

    private bool MovementUpdate = true;
    private bool MarketUpdate = true;
    public float stoppingDistance;

    public string crossing = "";
    private Dictionary<int, string> locDict = new Dictionary<int, string>();
    private bool passingAlleyway = false;
    private Vector3 alleyWayPoint = new Vector3(-5f, 0f, 3f);

    private Vector3 immobilePosition;
    public float OriginalSpeed;
    public bool showSphere = false;

    void Start()
    {   
        Flow.isDataCollection = true;
        Flow.totalNPCs++;
        Flow.idleNPCs++;
        stoppingDistance = Agent.stoppingDistance;

        // initialize location dictionary
        locDict.Add(0, "A");
        locDict.Add(1, "B");
        locDict.Add(2, "ALLEY");
        locDict.Add(3, "MARKET");

        immobilePosition = transform.position;
        OriginalSpeed = Agent.speed;
    }

    private void Awake()
    {
        Obstacle.enabled = false;
        Obstacle.carveOnlyStationary = false;
        Obstacle.carving = true;

        LastPosition = transform.position;
    }

    private void Update()
    {   
        if (Input.GetKeyDown(KeyCode.Space)) showSphere = !showSphere;

        crossing = CheckCrossing();
        string destStr = crossing.Split('-')[1];

        if (Agent.enabled) {
            // set priority to original priority
            Agent.avoidancePriority = OriginalPriority;
            
            // scale marker sphere with priority level
            MarkerSphere.transform.position = new Vector3(transform.position.x, 3.2f, transform.position.z);
            float sphereScale = (100 - OriginalPriority)/100f * 0.5f;
            float sphereScaleY = (destStr == "MARKET") ? sphereScale * 2f : sphereScale;
            MarkerSphere.transform.localScale = (showSphere) ? new Vector3(sphereScale, sphereScaleY, sphereScale) : Vector3.zero;

            // if currently passing through alleyway
            if (passingAlleyway)
            {
                // if reached alleyway, then proceed to final destination
                if (Vector3.Distance(transform.position, destination) < stoppingDistance)
                {
                    passingAlleyway = false;
                    Agent.SetDestination(destination);
                }
            }

            // if not at desired position, keep setting destination
            UpdateDestination();

            // update locked position
            immobilePosition = transform.position;

        } else {
            MarkerSphere.transform.localScale = Vector3.zero;
            Agent.avoidancePriority = 0;
            
            // make immovable
            transform.position = immobilePosition;
        }

        // update flow map
        UpdateFlowMap();

        // update obstacle agent
        UpdateObstacleAgent();
    }

    private void UpdateDestination()
    {
        if (Vector3.Distance(transform.position, destination) > stoppingDistance)
            {
                Vector3 playerLocation = transform.position;
                int playerLoc = GetAreaCode(playerLocation);
                bool isMarketFull = Flow.isMarketFull();

                // if player in alleyway or market, just move to desired position
                string playerLocStr = locDict[playerLoc];
                if (playerLocStr == "ALLEY" || playerLocStr == "MARKET")
                {
                    Agent.SetDestination(destination);
                }
                else if (playerLocStr == "A" || playerLocStr == "B")
                {   
                    // if player in A or B, check if market is full
                    if (isMarketFull)
                    {   
                        // if market is full, check if player is crossing between A and B
                        if (crossing == "A-B" || crossing == "B-A")
                        {   
                            // if player is crossing between A and B, then move to alleyway
                            Agent.SetDestination(alleyWayPoint);
                            passingAlleyway = true;
                        }
                    }
                    else
                    {   
                        // if market is not full, then freely move
                        Agent.SetDestination(destination);
                    }
                }
                else {
                    // if player not in A, B, Alley, or Market, then just move to desired position
                    Agent.SetDestination(destination);
                }
            }
    }

    private void UpdateFlowMap()
    {   
        if (Agent.enabled) {
            if (MovementUpdate) 
            {
                Flow.idleNPCs--;
                Flow.movingNPCs++;
                MovementUpdate = false;

            }
        } else {
            if (!MovementUpdate)
            {
                Flow.idleNPCs++;
                Flow.movingNPCs--;
                MovementUpdate = true;
            }
        }

        if (IsInMarketArea(transform.position))
        {   
            if (MarketUpdate)
            {
                Flow.marketNPCs++;
                MarketUpdate = false;
            }
        } else {
            if (!MarketUpdate)
            {
                Flow.marketNPCs--;
                MarketUpdate = true;
            }
        }
    }

    private void UpdateObstacleAgent()
    {
        if (Vector3.Distance(LastPosition, transform.position) > CarvingMoveThreshold)
        {
            LastMoveTime = Time.time;
            LastPosition = transform.position;
        }
        if (LastMoveTime + CarvingTime < Time.time && !isMoving)
        {
            Agent.enabled = false;
            Obstacle.enabled = true;
        }

        if (Agent.enabled && Agent.remainingDistance > Agent.stoppingDistance)
        {
            Character.Move(Agent.desiredVelocity, false, false);
        }
        else
        {   
            Character.Move(Vector3.zero, false, false);
            isMoving = false;
        }
    }

    public void SetDestination(Vector3 Position)
    {   
        destination = Position;
        Obstacle.enabled = false;

        LastMoveTime = Time.time;
        LastPosition = transform.position;

        StartCoroutine(MoveAgent(Position));
    }

    private IEnumerator MoveAgent(Vector3 Position)
    {
        yield return null;
        Agent.enabled = true;
        Agent.SetDestination(Position);
        isMoving = true;
    }

    public void SetPriority(int Priority)
    {
        Agent.avoidancePriority = Priority;
        OriginalPriority = Priority;
    }

    private int GetMin(int a, int b)
    {
        if (a < b)
        {
            return a;
        }
        return b;
    }

    private int GetMax(int a, int b)
    {
        if (a > b)
        {
            return a;
        }
        return b;
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

    public Vector3 GetNavMeshDestination()
    {   
        if (Agent.enabled == false)
        {
            return Vector3.zero;
        }

        // Get the end point of the path
        NavMeshPath path = Agent.path;
        Vector3 destination = path.corners[path.corners.Length - 1];

        // Get the closest point on the NavMesh to the destination
        NavMeshHit hit;
        NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas);

        return hit.position;
    }

    public string CheckCrossing()
    {   
        // check if player crossing between A and B and if market is full
        int playerLoc = GetAreaCode(transform.position);
        int destLoc = GetAreaCode(Agent.destination);

        string result = string.Format("{0}-{1}", locDict[playerLoc], locDict[destLoc]);

        // crossing = result;
        return result;
    }

    public string GetCrossing()
    {
        return crossing;
    }

    public int GetAreaCode(Vector3 loc)
    {
        Vector3 topLeftA = new Vector3(-10f, 0f, 10f);
        Vector3 bottomRightA = new Vector3(10f, 0f, 5.5f);

        Vector3 topLeftB = new Vector3(-10f, 0f, 1.5f);
        Vector3 bottomRightB = new Vector3(10f, 0f, -10f);

        if (loc.x > topLeftA.x && loc.x < bottomRightA.x && loc.z < topLeftA.z && loc.z > bottomRightA.z)
        {
            // in A
            return 0;
        }
        else if (loc.x > topLeftB.x && loc.x < bottomRightB.x && loc.z < topLeftB.z && loc.z > bottomRightB.z)
        {
            // in B
            return 1;
        }
        else
        {
            if (loc.x < 0)
            {   
                // in alleyway
                return 2;
            }
            else
            {   
                // in market
                return 3;
            }
        }
    }

    public Vector3 GetRandomAtArea(int areaCode)
    {
        Vector3 topLeftA = new Vector3(-10f, 0f, 10f);
        Vector3 bottomRightA = new Vector3(10f, 0f, 5.5f);

        Vector3 topLeftB = new Vector3(-10f, 0f, 1.5f);
        Vector3 bottomRightB = new Vector3(10f, 0f, -10f);

        Vector3 topLeftAlley = new Vector3(-10f, 0f, 5.5f);
        Vector3 bottomRightAlley = new Vector3(-5f, 0f, 1.5f);

        Vector3 topLeftMarket = new Vector3(2.5f, 0f, 5.5f);
        Vector3 bottomRightMarket = new Vector3(6.5f, 0f, 1.5f);

        Vector3 fountain = GameObject.Find("World/Fountain").transform.position;
        fountain = new Vector3(fountain.x, 0f, fountain.z);

        switch (areaCode)
        {
            case 0:
                // area A
                return new Vector3(Random.Range(topLeftA.x, bottomRightA.x), 0f, Random.Range(bottomRightA.z, topLeftA.z));
            case 1:
                // area B
                return new Vector3(Random.Range(topLeftB.x, bottomRightB.x), 0f, Random.Range(bottomRightB.z, topLeftB.z));
            case 2:
                // alleyway
                return new Vector3(Random.Range(topLeftAlley.x, bottomRightAlley.x), 0f, Random.Range(bottomRightAlley.z, topLeftAlley.z));
            case 3:
                // market
                return new Vector3(Random.Range(topLeftMarket.x, bottomRightMarket.x), 0f, Random.Range(bottomRightMarket.z, topLeftMarket.z));
            case 4:
                // fountain
                return fountain;
            default:
                return Vector3.zero;
        }
    }

    public void SetSpeed(float speed)
    {
        Agent.speed = speed;
    }

    public float GetSpeed()
    {
        return Agent.speed;
    }


}