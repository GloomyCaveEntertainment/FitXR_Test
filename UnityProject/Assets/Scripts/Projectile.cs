using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        _deployed = false;

    }
	
	// Update is called once per frame
	void Update () {
		if (_deployed)
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifeTime)
            {
                _deployed = false;
                GameMgr.Instance.ProjectileMiss(transform.position);
            }
        }
	}

    /// <summary>
    /// 
    /// </summary>
    public void Fire()
    {
        _timer = 0f;
        _deployed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //avoid self collisions
        if (_timer <= _minColTime)
            return;
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                GameMgr.Instance.EnemyHit();
                GameMgr.Instance.PlayImpactSfx(true, transform.position);
                break;

            case "Player":
                GameMgr.Instance.PlayerHit();
                GameMgr.Instance.PlayImpactSfx(true, transform.position);
                break;

            case "Obstacle":
                GameMgr.Instance.ProjectileMiss(transform.position, true);
                GameMgr.Instance.PlayImpactSfx(false, transform.position);
                break;
            case "Ground":
                GameMgr.Instance.ProjectileMiss(transform.position, false);
                GameMgr.Instance.PlayImpactSfx(false, transform.position);
                break;
            default:
                Debug.LogError("Wrong collision tag! - " + collision.gameObject + " - " + collision.gameObject.tag);
                break;
        }         
    }

    [SerializeField]
    private float _lifeTime;
    [SerializeField]
    private float _minColTime;

    private bool _deployed;
    private float _timer;
}
