using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(_target); 
	}

    [SerializeField]
    private Transform _target;
}
