using UnityEngine;

public class PlayerTest : MonoBehaviour
{
	private Rigidbody rb;
	private Transform trans;

	private const bool DEBUG = true;

	//defines for force
	private const float forwardForce = 8.0f;
	private const float backwardForce = 4.0f;

	private Vector3 relTranVel;

	private float startTime, deltaTime;
	private bool test;

	private void Start(){
		startTime = Time.time;
		rb = GetComponent<Rigidbody>();
		trans = GetComponent<Transform>();
		rb.AddRelativeForce(Vector3.forward * 16.0f, ForceMode.Impulse);
	}

	// Update is called once per frame
	private void Update(){
	}

	// Update game at fixed intervals
	private void FixedUpdate(){
		//update relative rotation and translation velocities
		relTranVel = trans.InverseTransformDirection(rb.velocity);

		rb.AddRelativeForce(Vector3.forward * backwardForce * -1, ForceMode.Force);

		DisplayDebug();

		Vector3 orgVel = new Vector3(1.0f, 1.0f, 2.0f);
		Vector3 testVel, refVel;
		testVel = Vector3.forward;


		if (relTranVel.z < 0 && test == false){
			test = true;
			deltaTime = Time.time - startTime;
		}
	}

	// Collection of debug logs
	private void DisplayDebug(){
		/*
		Debug.Log("Speed = " + relTranVel);
		Debug.Log("Time = " + deltaTime);
		*/
	}
}