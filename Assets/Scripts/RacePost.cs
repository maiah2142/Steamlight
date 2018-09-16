using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RacePost : MonoBehaviour {
	GameManager gm;

	void Start(){
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
	}

	void OnTriggerEnter(){
		gm.incrementRacePost();
	}
}
