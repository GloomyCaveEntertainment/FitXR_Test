/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2018/12/20
 * @Brief: Game logic manager
 * @Description: Controls game main loop
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{

    #region Public Data
    public static GameMgr Instance;

    public enum STATE { INIT = 0, INSTRUCTIONS, MOVING_AI, WAIT_PLAYER_INPUT, MOVING_PLAYER, AIMING_AI, AIMING_PLAYER, FIRING_AI, FIRING_PLAYER, SHOWING_RESULTS, FINISHED}
    #endregion


    #region Behaviour Methods
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }

    void Start()
    {
        Setup();
        SetState(STATE.INSTRUCTIONS);
    }

    void Update()
    {
        //ESC - Quit
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        //R - Restart
        else if (Input.GetKeyDown(KeyCode.R))
            ResetGame();

        switch (_state)
        {
            case STATE.INIT:

                break;

            case STATE.INSTRUCTIONS:
                _timer += Time.deltaTime;            
                if (Input.GetKeyDown(KeyCode.Space) || _timer >= _instructionsTime)
                {
                    _timer = 0f;
                    ++_instructionIndex;
                    //last instruction shown, game starts
                    if (_instructionIndex >= _gameInstructions.Count)
                    {
                        SetState(STATE.MOVING_AI);
                    }
                    else
                    {
                        _uiMgr.ShowInstruction(_gameInstructions[_instructionIndex]);                    
                    }
                }

                break;

            case STATE.WAIT_PLAYER_INPUT:
                if (Input.GetMouseButtonDown(0))
                {
                    _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    Debug.DrawRay(_ray.origin, _ray.direction*200f, Color.red, 2f);

                    if (Physics.Raycast(_ray, out _movementHitPos, Mathf.Infinity, _groundLayerMask | _obstacleLayerMask))
                    {
                        Debug.Log("Raycast");
                        if (_movementHitPos.collider != null && _movementHitPos.collider.gameObject.layer != _obstacleLayerMask)
                        {
                            Debug.Log("Successful, ground hit");
                            _playerTank.MoveToPosition(_movementHitPos.point);
                            SetState(STATE.MOVING_PLAYER);
                        }
                    }
                }
                break;

            case STATE.SHOWING_RESULTS:
                _timer += Time.deltaTime;
                if (_timer >= _resultsFbTime)
                {
                    SetState(_nextState);
                    _uiMgr.ShowFeedbackPanel(false);
                }
                break;

                
                
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Method called when any tank reached the moving target position
    /// </summary>
    public void TankOnPosition()
    {
        switch (_state)
        {
            case STATE.MOVING_AI:
                SetState(STATE.WAIT_PLAYER_INPUT);
                break;

            case STATE.MOVING_PLAYER:
                SetState(STATE.FIRING_AI);
                break;

                default:
                Debug.LogError("Wrong transition: "+_state);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void SetPlayerFireHeight(float val)
    {
        _playerTank.SetFitingAngle(val);       
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void SetPlayerFirePower(float val)
    {
        _playerTank.FiringPower = val;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ProjectileMiss(Vector3 position, bool byObstacle = false)
    {
        switch (_state)
        {
            case STATE.FIRING_AI:
                _uiMgr.ShowFeedbackText("Enemy misses!");
                _enemyTank.RecalculateValues(position, byObstacle);     //improve aiming for next round
                SetState(STATE.SHOWING_RESULTS);
                _nextState = STATE.AIMING_PLAYER;
                //SetState(STATE.FIRING_PLAYER);
                break;

            case STATE.FIRING_PLAYER:
                _uiMgr.ShowFeedbackText("You miss!");
                SetState(STATE.SHOWING_RESULTS);
                _nextState = STATE.FIRING_AI;
                //SetState(STATE.FIRING_AI);
                break;
        }
        _projectile.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnemyHit()
    {
        //TODO
        _uiMgr.ShowFeedbackText("Enemy destroyed, you won!");
        SetState(STATE.SHOWING_RESULTS);
        _nextState = STATE.FINISHED;
        Debug.Log("---------         Enemy hit        -------------");
        //_uiMgr.ShowFeedbackPanel(false);
        //_uiMgr.ShowGameFinishedPanel(true);
        _win = true;
        _projectile.SetActive(false);
        _explosionPs.transform.position = _enemyTank.transform.position;
        _explosionPs.gameObject.SetActive(true);
        _explosionPs.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayerHit()
    {
        //TODO
        _uiMgr.ShowFeedbackText("You got destroyed, you lose!");
        SetState(STATE.SHOWING_RESULTS);
        _nextState = STATE.FINISHED;
        Debug.Log("-----------      Player hit!         ----------------");
        //_uiMgr.ShowFeedbackPanel(false);
        //_uiMgr.ShowGameFinishedPanel(false);
        _win = false;
        _projectile.SetActive(false);
        _explosionPs.transform.position = _playerTank.transform.position;
        _explosionPs.gameObject.SetActive(true);
        _explosionPs.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void FirePlayerTank()
    {
        if (_state != STATE.AIMING_PLAYER)
            return;
        SetState(STATE.FIRING_PLAYER);
        _playerTank.Fire(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetGame()
    {
        Setup();
        SetState(STATE.INSTRUCTIONS);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="impact"></param>
    /// <param name="pos"></param>
    public void PlayImpactSfx(bool impact, Vector3 pos)
    {
        _audioSource.transform.position = pos;
        _audioSource.PlayOneShot(impact ? _impactSfx : _missFx);
    }
    #endregion



    #region Private Methods
    /// <summary>
    /// Initi setup
    /// </summary>
    private void Setup()
    {
        _state = STATE.INIT;
        if (_uiMgr == null)
            _uiMgr = UIMgr.Instance;
        _instructionIndex = 0;
        _uiMgr.ClearAll();
        _timer = 0f;
        _playerTank.Setup();
        _enemyTank.Setup();
        _projectile.SetActive(false);
        _explosionPs.gameObject.SetActive(false);
        StartTanks();
    }

    /// <summary>
    /// 
    /// </summary>
    private void StartGame()
    {
        _state = STATE.INIT;
        //TODO
    }
    /// <summary>
    /// Set tanks on start position + init setup
    /// </summary>
    private void StartTanks()
    {
        _playerTank.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        _enemyTank.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        _playerTank.transform.position = _playerStartPos.position;
        _playerTank.transform.rotation = _playerStartPos.rotation;
        _enemyTank.transform.position = _enemyStartPos.position;
        _enemyTank.transform.rotation = _enemyStartPos.rotation;
        _playerTank.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
        _enemyTank.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newState"></param>
    private void SetState(STATE newState)
    {
        _state = newState;
        _uiMgr.SetFeedbackText(_state);
        switch (_state)
        {
            case STATE.INSTRUCTIONS:
                _instructionIndex = 0;
                _timer = 0f;
                _uiMgr.ShowInstruction(_gameInstructions[_instructionIndex]);
                break;

            case STATE.MOVING_AI:
                _enemyTank.MoveToPosition(GetRandomPosition());
                break;

            case STATE.WAIT_PLAYER_INPUT:
                _uiMgr.ShowInstruction("Click on the terrain to move");
                break;

            case STATE.MOVING_PLAYER:
                
                break;

            case STATE.FIRING_AI:
                
                _enemyTank.Fire();
                break;

            case STATE.AIMING_PLAYER:
                _uiMgr.ShowAimingPanel(true);
                break;

            case STATE.FIRING_PLAYER:
                
                break;

            case STATE.SHOWING_RESULTS:
                _uiMgr.ShowFeedbackPanel(true);
                _uiMgr.ShowAimingPanel(false);
                _timer = 0f;
                break;
            case STATE.FINISHED:
                _uiMgr.ShowFeedbackPanel(false);
                _uiMgr.ShowAimingPanel(false);
                _uiMgr.ShowGameFinishedPanel(_win);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomPosition()
    {
        bool validPos = false;
        RaycastHit _hit;

        do 
        {
            Physics.Raycast(GetRandomFloorPos() + Vector3.up * 100f, Vector3.down, out _hit, Mathf.Infinity, _groundLayerMask | _obstacleLayerMask);
            if (_hit.collider != null && _hit.collider.gameObject.layer != _obstacleLayerMask)
            {
                Debug.Log("Hit layer: " + _hit.collider.gameObject.layer + "and point is: "+_hit.point);
                return _hit.point;
            }

        } while (_hit.collider == null);

        return Vector3.zero;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomFloorPos()
    {
        return new Vector3(_navigableGround.transform.position.x + Random.Range(-_navigableGround.transform.localScale.x*0.5f, _navigableGround.transform.localScale.x * 0.5f),
                            transform.position.y,
                            _navigableGround.transform.position.z + Random.Range(-_navigableGround.transform.localScale.z*0.5f, _navigableGround.transform.localScale.z * 0.5f));
    }
    #endregion


    #region Properties
    public GameObject Projectile {  get { return _projectile; } private set { } }
    public Vector3 PlayerPos {  get { return _playerTank.transform.position; } private set { } }
    #endregion

    #region Private Serialized Fields
    [SerializeField]
    private Tank _playerTank, _enemyTank;
    [SerializeField]
    private Transform _playerStartPos, _enemyStartPos;  //starting positions
    [SerializeField]
    private List<string> _gameInstructions;

    [SerializeField]
    private float _instructionsTime;    //showing time
    [SerializeField]
    private float _resultsFbTime;       //fire result feedback

    [SerializeField]
    private GameObject _navigableGround;    //object which determines the navigable area size
    [SerializeField]
    private LayerMask _groundLayerMask, _obstacleLayerMask;
    [SerializeField]
    private GameObject _projectile;     //reusing one projectile for both tanks 
    [SerializeField]
    private ParticleSystem _explosionPs;
    
    //Audio
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _impactSfx, _missFx;  //fire SFX
    #endregion

    #region Private Non-serialized Fields
    private STATE _state, _nextState;
    private UIMgr _uiMgr;
    
    //aux vars used to raycast selected mov position over terrain
    private Ray _ray; 
    private RaycastHit _movementHitPos;

    private float _timer;
    private int _instructionIndex;  //current instruction from the list to show
    private bool _win;
    #endregion

}
