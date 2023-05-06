using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ObstacleAgent)), RequireComponent(typeof(ThirdPersonCharacter))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    public int CamMode = 0;
    public GameObject ThirdPersonCam;
    public GameObject OverheadCam;

    public ObstacleAgent Agent;
    public FlowMap flow;
    public GameObject MarkerSphere;
    public ThirdPersonCharacter Character;
    public string crossing = "";

    private float OriginalY;

    private Vector3 DesiredPosition;    

    private Vector3 WASDMovement = Vector3.zero;

    void Start()
    {
        OriginalY = transform.position.y;
        cam = OverheadCam.GetComponent<Camera>();
        Agent.SetPriority(50);
        DesiredPosition = Agent.destination;
        flow = GameObject.Find("FlowMap").GetComponent<FlowMap>();
    }

    private void Update()
    {   
        if (transform.position.y != OriginalY)
        {
            transform.position = new Vector3(transform.position.x, OriginalY, transform.position.z);
        }

        if (Input.GetMouseButtonDown(0))
        {   
            // print("Mouse Down");
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit))
            {
                Agent.SetDestination(hit.point);
                DesiredPosition = hit.point;
            }
        }

        // if rightclick, switch camera
        if (Input.GetMouseButtonDown(1))
        {   
            if (CamMode == 1)
            {   
                CamMode = 0;
                OverheadCam.SetActive(true);
                ThirdPersonCam.SetActive(false);
                cam = OverheadCam.GetComponent<Camera>();
            }
            else
            {   
                CamMode = 1;
                OverheadCam.SetActive(false);
                ThirdPersonCam.SetActive(true);
                cam = ThirdPersonCam.GetComponent<Camera>();
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {   
            float fastSpeed = Agent.OriginalSpeed * 2;
            float acceleration = 0.7f;
            float curSpeed = Agent.GetSpeed();

            // accelerate to speed
            if (curSpeed < fastSpeed)
            {   
                curSpeed += acceleration;
                Agent.SetSpeed(curSpeed);
            } else {
                Agent.SetSpeed(fastSpeed);
            }
        }
        else {
            Agent.SetSpeed(Agent.OriginalSpeed);
        }
    }
}
