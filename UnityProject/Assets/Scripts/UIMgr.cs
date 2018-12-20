/************************************************************************/
/* @Author: Rodrigo Ribeiro-Pinto Carvajal
 * @Date: 2018/12/20
 * @Brief: User Interface helper
 * @Description: Manages all UI elements
 * ***********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMgr : MonoBehaviour
{

    #region Public Data
    public static UIMgr Instance;
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
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Clear all texts
    /// </summary>
    public void ClearAll()
    {
        _instructionsPanel.gameObject.SetActive(false);
        _turnPanel.gameObject.SetActive(false);
        _actionPanel.gameObject.SetActive(false);
        _shellResultPanel.gameObject.SetActive(false);
        _gameFinishedPanel.gameObject.SetActive(false);
        _instructionsText.text = _turnText.text = _actionText.text = _shellResultText.text = "";
        _firingPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Show game instruction
    /// </summary>
    /// <param name="text"></param>
    public void ShowInstruction(string text)
    {
        if (!_instructionsPanel.gameObject.activeSelf)
            _instructionsPanel.gameObject.SetActive(true);
        _instructionsText.text = text;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameState"></param>
    public void SetFeedbackText(GameMgr.STATE gameState)
    {
        //panel object activation / deactivation
        if (gameState != GameMgr.STATE.INSTRUCTIONS)
        {
            if (!_turnPanel.gameObject.activeSelf)
                _turnPanel.gameObject.SetActive(true);
            if (!_actionPanel.gameObject.activeSelf)
                _actionPanel.gameObject.SetActive(true);
            _instructionsPanel.gameObject.SetActive(false);
        }

        switch (gameState)
        {
            case GameMgr.STATE.MOVING_AI:
                _turnText.text = "Enemy turn";
                _actionText.text = "Moving...";
                break;
            case GameMgr.STATE.WAIT_PLAYER_INPUT:
                _turnText.text = "Player turn";
                _actionText.text = "Selecting movement position...";
                break;
            case GameMgr.STATE.MOVING_PLAYER:
                _turnText.text = "Player turn";
                _actionText.text = "Moving...";
                break;
            case GameMgr.STATE.AIMING_AI:
                _turnText.text = "Enemy turn";
                _actionText.text = "Aiming...";
                break;

            case GameMgr.STATE.AIMING_PLAYER:
                _turnText.text = "Player turn";
                _actionText.text = "Aiming...";
                break;

            case GameMgr.STATE.FIRING_AI:
                _turnText.text = "Enemy turn";
                _actionText.text = "Firing...";
                break;

            case GameMgr.STATE.FIRING_PLAYER:
                _turnText.text = "Player turn";
                _actionText.text = "Firing...";
                break;
        }
    }

    /// <summary>
    /// Getsfiring height value from UI slider
    /// </summary>
    /// <param name="val"></param>
    public void SetFiringHeight(float val)
    {
        GameMgr.Instance.SetPlayerFireHeight(val);
    }

    /// <summary>
    /// Getsfiring power value from UI slider
    /// </summary>
    /// <param name="val"></param>
    public void SetFiringPower(float val)
    {
        GameMgr.Instance.SetPlayerFirePower(val);
    }

    /// <summary>
    /// Shows / hides player input aiming panel
    /// </summary>
    /// <param name="enabled"></param>
    public void ShowAimingPanel(bool enabled)
    {
        _firingPanel.gameObject.SetActive(enabled);
    }

    /// <summary>
    /// Shows feedback about the current shot
    /// </summary>
    /// <param name="text"></param>
    public void ShowFeedbackText(string text)
    {
        if (!_shellResultPanel.gameObject.activeInHierarchy)
            _shellResultPanel.gameObject.SetActive(true);
        _shellResultText.text = text;
    }

    public void PlayerFire()
    {
        GameMgr.Instance.FirePlayerTank();
    }

    public void ShowFeedbackPanel(bool show)
    {
        _shellResultPanel.gameObject.SetActive(show);
    }

    public void ShowGameFinishedPanel(bool gameWon)
    {
        _gameFinishedPanel.gameObject.SetActive(true);
        if (gameWon)
            _gameFinishedText.text = "You won!";
        else
            _gameFinishedText.text = "You lost!";
    }

    public void RestartGame()
    {
        GameMgr.Instance.ResetGame();
    }
    #endregion



    #region Private Methods

    #endregion


    #region Properties

    #endregion

    #region Private Serialized Fields

    #endregion

    #region Private Non-serialized Fields
    [SerializeField]
    private RectTransform _instructionsPanel;
    [SerializeField]
    private Text _instructionsText;
    [SerializeField]
    private RectTransform _turnPanel;
    [SerializeField]
    private Text _turnText;
    [SerializeField]
    private RectTransform _actionPanel;
    [SerializeField]
    private Text _actionText;
    [SerializeField]
    private RectTransform _shellResultPanel;
    [SerializeField]
    private RectTransform _firingPanel;
    [SerializeField]
    private Text _shellResultText;

    [SerializeField]
    private GameObject _gameFinishedPanel;
    [SerializeField]
    private Text _gameFinishedText;
    #endregion

}
