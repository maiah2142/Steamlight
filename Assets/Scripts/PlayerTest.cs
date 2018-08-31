using UnityEngine;

public class PlayerTest : MonoBehaviour
{
	private Rigidbody rb;
	private Transform trans;

	private const bool DEBUG = true;

	//defines for force
	private const float forwardForce = 8.0f;
	private const float backwardForce = 4.0f;

	private Vector3 relVel;

	private float startTime, deltaTime;
	private bool test;

	private void Start(){
		startTime = Time.time;
		rb = GetComponent<Rigidbody>();
		trans = GetComponent<Transform>();
		rb.AddRelativeTorque(Vector3.right * 6.2843f, ForceMode.Impulse);
		//rb.AddRelativeTorque(Vector3.up * 3.0f, ForceMode.Impulse);

	}

	// Update is called once per frame
	private void Update(){
	}

	// Update game at fixed intervals
	private void FixedUpdate(){
		//update relative rotation and translation velocities
		relVel = trans.InverseTransformDirection(rb.angularVelocity);

		DisplayDebug();

		//if current velocity is within the positive and negative force
		if (relVel[0] < 3 * Time.deltaTime &&
			relVel[0] > 3 * Time.deltaTime * -1){
			//set the velocity of current axis to 0
			Vector3 vel = relVel;
			vel[0] = 0.0f;
			rb.angularVelocity = transform.TransformDirection(vel);

		}
		//if the current velocity of the axis is positive
		if (relVel[0] > 0){
			//apply negative force to the rigid body
			rb.AddRelativeTorque(Vector3.right * 3 * -1, ForceMode.Force);
		//if the current velocity of the axis is negative
		} else if (relVel[0] < 0){
			//apply positive force to the rigid body
			rb.AddRelativeTorque(Vector3.right * 3, ForceMode.Force);
		}

	}

	// Collection of debug logs
	private void DisplayDebug(){
		Debug.Log("Speed = " + relVel.magnitude);
	}
}