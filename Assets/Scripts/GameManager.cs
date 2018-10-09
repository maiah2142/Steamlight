using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GameManager : MonoBehaviour {
	public int raceCtr, lapCtr;
	private GameObject[] racePosts;

	// Use this for initialization
	void Start () {
		//initialise the array of game objects
		//array is dynamically defined at start time depending on number of children
		racePosts = new GameObject[gameObject.transform.childCount];

		/*
		int init = 0;
		foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]){
			if (go.name.StartsWith("RacePost")){
				racePosts[init] = go;
				init++;
			}
		}
		*/
		foreach (Transform child in transform){
			int raceIndex = int.Parse(Regex.Match(child.gameObject.name, @"\d+").Value);
			racePosts[raceIndex] = child.gameObject;
		}

		for(int i = 0; i < racePosts.Length; i++)
			Debug.Log(racePosts[i].name);

		//Debug.Log("Number of race posts: " + racePosts.Length);
	}

	// Increment to the next active race post
	public void incrementRacePost(){
		//racePosts[raceCtr].SetActive(false);
		racePosts[raceCtr].GetComponent<MeshRenderer>().enabled = false;
		if(raceCtr == racePosts.Length - 1)
			raceCtr = 0;
		else
			raceCtr++;;
		racePosts[raceCtr].GetComponent<MeshRenderer>().enabled = true;
	}
}
