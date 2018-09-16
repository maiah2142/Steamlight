using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	private int raceCtr, lapCtr;
	private GameObject[] racePosts;

	// Use this for initialization
	void Start () {
		//TODO: find better way of initiating into array
		int i = 0;
		foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]){
			if (go.name.StartsWith("RacePost"))
				i++;
		}
		racePosts = new GameObject[i];
		int init = 0;
		foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]){
			if (go.name.StartsWith("RacePost")){
				racePosts[init] = go;
				init++;
			}
		}

		Debug.Log(racePosts.Length);
	}

	// Increment to the next active race post
	public void incrementRacePost(){
		racePosts[raceCtr].SetActive(false);
		if(raceCtr == racePosts.Length - 1)
			raceCtr = 0;
		else
			raceCtr++;
		racePosts[raceCtr].SetActive(true);
	}
}
