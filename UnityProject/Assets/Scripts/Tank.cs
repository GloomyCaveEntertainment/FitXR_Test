/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2018/12/20
 * @Brief: tank object class
 * @Description: provides basic functionality
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Tank : MonoBehaviour {

    #region Public Data
    public enum TSTATE { IDLE = 0, WAIT_FOR_INPUT, MOVING, AIMING, FIRING}
    #endregion

    #region Behaviour Methods
    // Use this for initialization
    void Start () {
		
	}
    
    // Update is called once per frame
    void Update () {
		switch (_state)
        {


            case TSTATE.MOVING:
                if (Vector3.Distance(_targetPos, transform.position) <= _minReachDist)
                {
                    GameMgr.Instance.TankOnPosition();
                    _state = TSTATE.IDLE;
                }
                break;
        }
	}
    #endregion

    #region Public Methods
    /// <summary>
    /// 
    /// </summary>
    public void Setup()
    {
        _state = TSTATE.IDLE;
        _agent = GetComponent<NavMeshAgent>();
        if (!_agent)
            Debug.LogError("No NavMeshAgent component found!");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    public void MoveToPosition(Vector3 position)
    {
        _targetPos = position;
        if (!_agent.SetDestination(_targetPos))
            Debug.LogError("Couldn't reach position: " + _targetPos + " -tank: " + name);
        else
            _state = TSTATE.MOVING;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Fire(bool isPlayerTank = false)
    {
        if (isPlayerTank)
        {
            //already calculated from player input
        }
        else
        {
            _currentFireAngle = Random.Range(_minFireAngle, _maxFireAngle);
            _currentFirePower = Random.Range(_minFirePower, _maxFirePower);
            _currentFireVector = new Vector3(_turretHelper.forward.x, _turretHelper.forward.z * Mathf.Tan(_currentFireAngle * (Mathf.PI / 180f)), _turretHelper.forward.z).normalized;

        }
        Debug.Log("Firing stats: " + _currentFireAngle + " - " + _currentFirePower);
        Fire(_currentFireVector, _currentFirePower);
    }

    /// <summary>
    /// Overloaded method with fixed values
    /// </summary>
    /// <param name="fireDir"></param>
    /// <param name="firePower"></param>
    public void Fire(Vector3 fireDir, float firePower)
    {
        Debug.Log(name+" firing with dir and power: " + fireDir + " - " + firePower);
        GameMgr.Instance.Projectile.transform.position = _turretHelper.transform.position;
        GameMgr.Instance.Projectile.SetActive(true);
        GameMgr.Instance.Projectile.GetComponent<Projectile>().Fire();
        GameMgr.Instance.Projectile.GetComponent<Rigidbody>().velocity = fireDir * firePower;
        _state = TSTATE.FIRING;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="height"></param>
    public void SetFireVector(float height)
    {
        _currentFireVector = new Vector3(_turretHelper.forward.x, _turretHelper.forward.z * Mathf.Tan(height * (Mathf.PI / 180f)), _turretHelper.forward.z).normalized;
        Debug.Log("Player fire vector: " +height + " - "+ _currentFireVector);
    }

    /// <summary>
    /// Improve aiming based on last firing values 
    /// </summary>
    public void RecalculateValues()
    {
        //TODO
        Debug.Log("Recalculating. . .");
    }
    #endregion

    #region Private Methods

    #endregion

    #region Properties
    public Vector3 FiringVector {  get { return _currentFireVector; } set { _currentFireVector = value; } }
    public float FiringPower {  get { return _currentFirePower; } set { _currentFirePower = value; } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private float _minReachDist;    //minimum distance to reach a target destination
    [SerializeField]
    private Transform _turretHelper;
    [SerializeField]
    private float _minFireAngle, _maxFireAngle;   //over the horizontal plane, in degrees
    [SerializeField]
    private float _minFirePower, _maxFirePower;


    //TOREMOVE
    #endregion

    #region Private Non-Serialized Fields
    private TSTATE _state;
    private NavMeshAgent _agent;
    private Vector3 _targetPos; //movement target position
    private float _currentFireAngle, _currentFirePower;
    private Vector3 _currentFireVector;
    #endregion
}
