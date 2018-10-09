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

		foreach (Transform child in transform){
			int raceIndex = int.Parse(Regex.Match(child.gameObject.name, @"\d+").Value);
			racePosts[raceIndex] = child.gameObject;
		}
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
