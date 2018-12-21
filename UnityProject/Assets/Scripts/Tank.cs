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
        _randomShot = true;
        _missCounter = 0;
        _currentFireAngle = _minFireAngle;
        _currentFirePower = _minFirePower;

    }

    /// <summary>
    /// Moves to position using the navmesh
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
    /// <param name="isPlayerTank">player or AI</param>
    public void Fire(bool isPlayerTank = false)
    {
        if (isPlayerTank)
        {
            //height angle already calculated from player input
            _currentFireVector = new Vector3(_turretHelper.forward.x, Mathf.Abs(_turretHelper.forward.z) * Mathf.Tan(_currentFireAngle * (Mathf.PI / 180f)), _turretHelper.forward.z).normalized;
        }
        else  //AI
        {
            //First shot uses random values.
            if (_randomShot)
            {
                _randomShot = false;
                _currentFireAngle = Random.Range(_minFireAngle, _maxFireAngle);
                _currentFirePower = Random.Range(_minFirePower, _maxFirePower);

            }
            _currentFireVector = new Vector3(_turretHelper.forward.x, Mathf.Abs(_turretHelper.forward.z) * Mathf.Tan(_currentFireAngle * (Mathf.PI / 180f)), _turretHelper.forward.z).normalized;
        }
        Fire(_currentFireVector, _currentFirePower);
    }

    /// <summary>
    /// Overloaded method with fixed values
    /// </summary>
    /// <param name="fireDir"></param>
    /// <param name="firePower"></param>
    public void Fire(Vector3 fireDir, float firePower)
    {
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
    public void SetFitingAngle(float height)
    {
        _currentFireAngle = height;
        //_currentFireVector = new Vector3(_turretHelper.forward.x, Mathf.Abs(_turretHelper.forward.z) * Mathf.Tan(height * (Mathf.PI / 180f)), _turretHelper.forward.z).normalized;
    }

    /// <summary>
    /// Improve aiming based on last firing values 
    /// </summary>
    public void RecalculateValues(Vector3 missPosition, bool missedByObstacle)
    {
        Debug.Log("Recalculating. . .");
        //Too many attempts with same angle, change it
        if (_missCounter > _maxMissesSameAngle)
        {
            GetNewFiringAngle(true);
        }
        else
        {
            //Short shot
            if (Vector3.Distance(missPosition, transform.position) < Vector3.Distance(GameMgr.Instance.PlayerPos, transform.position))
            {   //raise angle, there was an obstacle 
                if (missedByObstacle)
                    GetNewFiringAngle(false);
                else
                {
                    ++_missCounter;
                    Debug.Log("**MIN New firing power is from: " + _currentFirePower);
                    _newMinPower_AI = _currentFirePower;    //adjust firing range
                    _currentFirePower = Mathf.Lerp(_currentFirePower, _maxFirePower, 0.5f);
                    Debug.Log("to: " + _currentFirePower);
                }
            }
            else //far shot
            {
                ++_missCounter;
                Debug.Log("**   FS - New firing power is from: " + _currentFirePower);
                _newMaxPower_AI = _maxFirePower;    //adjust firing range
                _currentFirePower = Mathf.Lerp(_minFirePower, _currentFirePower, 0.5f);
                Debug.Log("to: " + _currentFirePower);
            }
        }
        
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Calculates a new firing angle
    /// </summary>
    /// <param name="randomAngle">new random angle or raise current angle</param>
    private void GetNewFiringAngle(bool randomAngle)
    {
        if (randomAngle)
            _currentFireAngle = Random.Range(_minFireAngle, _maxFireAngle);
        else
            _currentFireAngle = Mathf.Lerp(_currentFireAngle, _maxFireAngle, 0.5f);// (_maxFireAngle - _currentFireAngle) * 0.5f;
                                                                               //reset power range if we tweak angle
        _newMinPower_AI = _minFirePower;
        _newMaxPower_AI = _maxFirePower;
        _missCounter = 0;   //reset counter, new angle
    }
    #endregion

    #region Properties
    public Vector3 FiringVector {  get { return _currentFireVector; } set { _currentFireVector = value; } }
    public float FiringPower {  get { return _currentFirePower; } set { _currentFirePower = value; } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private float _minReachDist;    //minimum distance to reach a target destination
    [SerializeField]
    private Transform _turretHelper;    //projectile spawn spot
    [SerializeField]
    private float _minFireAngle, _maxFireAngle;   //over the horizontal plane, in degrees
    [SerializeField]
    private float _minFirePower, _maxFirePower;
    [SerializeField]
    private int _maxMissesSameAngle;    //max allowed misses with the same angle

    #endregion

    #region Private Non-Serialized Fields
    private TSTATE _state;
    private NavMeshAgent _agent;
    private Vector3 _targetPos; //movement target position
    private float _currentFireAngle, _currentFirePower;
    private float  _newMaxPower_AI, _newMinPower_AI;
    private Vector3 _currentFireVector;

    private int _missCounter;   //counter to prevent "dead" aiming loops
    private bool _randomShot;   //flag used for first AI shot, taking init random values
    #endregion
}
