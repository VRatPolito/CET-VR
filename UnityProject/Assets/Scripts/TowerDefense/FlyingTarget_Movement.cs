using UnityEngine;

public class FlyingTarget_Movement : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject _tower; //flying targets will fall towards the tower

    [SerializeField]
    private float startSpeed; //magnitude of the initial velocity
    private Vector3 _startVelocity; //initial velocity (with y = 0) -> to direct target towards tower in the fall
    private Vector3 _direction;

    private Vector2 _towerXZPos;
    [SerializeField]
    private float _fallDist = 10f; //the airdrops will fall 10m from the tower
    private float _startDist;
    private Vector2 _startXZPos;

    [SerializeField]
    float minStartSpeed = 1f, maxStartSpeed = 5f;

    private void Awake()
    {
        GameObject _terrain = GameObject.FindGameObjectWithTag("Ground");
        //flying targets will not collide with trees
        _terrain.GetComponent<CustomTreeColliders>().IgnoreCollisionWithTrees(this.gameObject);
    }
    void Start()
    {
        _tower = GameObject.FindGameObjectWithTag("Tower");

        _direction = calculateDirection();
        _startVelocity = _direction * startSpeed;

        this.gameObject.GetComponent<Rigidbody>().velocity = _startVelocity;

        _towerXZPos = new Vector2(_tower.transform.position.x, _tower.transform.position.z);
        _startXZPos = new Vector2(transform.position.x, transform.position.z); //initial pos
        _startDist = Mathf.Abs(Vector2.Distance(_towerXZPos, _startXZPos));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 _velocity = new Vector3();
        _velocity.y = _startVelocity.y; //unchanged component

        Vector2 _xzpos = new Vector2(transform.position.x, transform.position.z);

        float distFromTower = Vector2.Distance(_towerXZPos, _xzpos); //airdrop will fall a default _falldist distance from the tower
        
                distFromTower = Mathf.Abs(distFromTower);
        float t = (distFromTower - _fallDist) / _startDist;

        float _xvel = Mathf.Lerp(0f, _startVelocity.x, t);
        float _zvel = Mathf.Lerp(0f, _startVelocity.z, t);

        _velocity.x = _xvel;
        _velocity.z = _zvel;

        this.gameObject.GetComponent<Rigidbody>().velocity = _velocity;
        
    }

    Vector3 calculateDirection()
    {
        Vector3 _d = -(this.transform.position - _tower.transform.position).normalized; //direction it will move up (as it falls)
        return _d;
    }
    public void setStartSpeed(float startTime, float endTime, float curTime)
    {
        float perc = Mathf.InverseLerp(startTime, endTime, curTime);
        startSpeed = Mathf.Lerp(minStartSpeed, maxStartSpeed, perc);
    }
    
}
