using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

using UnityEngine.UI; //temporary

public class GameManager : MonoBehaviour {
	public int raceCtr, lapCtr;
	private GameObject[] racePosts;
	public Text raceTimerText;

	private float startTime, lapTime;

	// Use this for initialization
	void Start () {
		//initialise the array of game objects
		//array is dynamically defined at start time depending on number of children
		racePosts = new GameObject[gameObject.transform.childCount];

		foreach (Transform child in transform){
			int raceIndex = int.Parse(Regex.Match(child.gameObject.name, @"\d+").Value);
			racePosts[raceIndex] = child.gameObject;
		}

		raceTimerText.text = "0.00 s";
	}

	void FixedUpdate(){
		if(raceCtr != 0)
			lapTime = Time.time - startTime;
	}

	void Update(){
		if(raceCtr != 0)
			raceTimerText.text = lapTime.ToString("F2") + " s";
	}

	// Increment to the next active race post
	public void incrementRacePost(){
		if(raceCtr == 0)
			startTime = Time.time;

		racePosts[raceCtr].GetComponent<MeshRenderer>().enabled = false;
		if(raceCtr == racePosts.Length - 1)
			raceCtr = 0;
		else
			raceCtr++;;
		racePosts[raceCtr].GetComponent<MeshRenderer>().enabled = true;
	}
}
