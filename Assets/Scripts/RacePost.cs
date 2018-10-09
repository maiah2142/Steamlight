using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class RacePost : MonoBehaviour {
	GameManager gm;
	public MeshRenderer mr;
	public int raceIndex;


	void Start(){
		gm = gameObject.GetComponentInParent<GameManager>();
		mr = gameObject.GetComponent<MeshRenderer>();
		raceIndex = int.Parse(Regex.Match(gameObject.name, @"\d+").Value);
	}

	void OnTriggerEnter(){
		if(gm.raceCtr == raceIndex){
			gm.incrementRacePost();
		}
	}
}