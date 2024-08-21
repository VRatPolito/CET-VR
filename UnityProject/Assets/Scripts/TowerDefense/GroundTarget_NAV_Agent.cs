using UnityEngine;
using UnityEngine.AI;

public class GroundTarget_NAV_Agent : MonoBehaviour
{
    [SerializeField]
    private Transform target; //target to reach
    private float speedLimit= 20f; //to correct problems with agents that go the wrong way
    [SerializeField]
    private float minSpeed = 3f;
    [SerializeField]
    private float maxSpeed = 8f;
    NavMeshAgent agent;   
    void Start()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);
        if(agent == null)
            Debug.Log("agent = null");
        if(this.gameObject.GetComponent<Rigidbody>().velocity.magnitude >= speedLimit)
        {
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void setSpeed(float startTime, float endTime, float curTime)
    {
        float perc = Mathf.InverseLerp(startTime, endTime, curTime);
        agent = this.gameObject.GetComponent<NavMeshAgent>();
 
        if (agent != null)
        {   
            agent.speed = Mathf.Lerp(minSpeed, maxSpeed, perc); 
        }
            
        else
            Debug.Log("Agent = null");

    }
}
