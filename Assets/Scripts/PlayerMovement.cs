using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	private Rigidbody m_rb;
	private Transform m_trans;

	private const bool DEBUG = true;

	//defines for force
	private float pitchForce = 240.0f;
	private float rollForce = 240.0f;
	private float yawForce = 120.0f;
	private float forwardForce = 400.0f;
	private float transForce = 200.0f;

	private float pitchClamp = 3.0f;
	private float rollClamp = 3.0f;
	private float yawClamp = 1.5f;
	//Define
	private float pitchAxis, rollAxis, yawAxis, surgeAxis, swayAxis, heaveAxis;

	private bool keySpaceBrake, keySAS, keyABS;

	private Vector3 relativeAngularVelocity, relativeTranslationalVelocity;

	private void Start(){
		m_rb = GetComponent<Rigidbody>();
		m_trans = GetComponent<Transform>();
	}

	// Update is called once per frame
	private void Update(){
		//grab axis of controller
		pitchAxis = Input.GetAxisRaw("Pitch");
		rollAxis = Input.GetAxisRaw("Roll");
		yawAxis = Input.GetAxisRaw("Yaw");
		surgeAxis = Input.GetAxisRaw("Surge");
		swayAxis = Input.GetAxisRaw("Sway");
		heaveAxis = Input.GetAxisRaw("Heave");

		//grab button input
		if (Input.GetButton("SAS"))
			keySAS = !keySAS;
		if (Input.GetButton("ABS"))
			keyABS = !keyABS;

		//keySpaceBrake = Input.GetButton("Space Brake");
	}

	// Update game at fixed intervals
	private void FixedUpdate(){
		//update angular vel
		relativeAngularVelocity = m_trans.InverseTransformDirection(m_rb.angularVelocity);
		relativeTranslationalVelocity = m_trans.InverseTransformDirection(m_rb.velocity);

		ShipRotation(keySAS);
		ShipThrust(keyABS);

		DisplayDebug();
	}

	// Rotates ship based on assist mode
	private void ShipRotation(bool assist){
		//if SAS mode is on
		if (assist){
			//use SAS "smart" counterthrust
			/*
				Vector3 temp = Vector3.left * DeltaForce(pitchForce) * pitchAxis;
				Vector3.up * DeltaForce(yawForce) * yawAxis);
			*/
			//pitch
			SAS(pitchAxis,
				Vector3.right * DeltaForce(CounterForce(0.0f, relativeAngularVelocity.x, pitchForce)),
				Vector3.left * DeltaForce(SASClamp(pitchClamp, relativeAngularVelocity.x, pitchForce)) * pitchAxis);
			//roll
			SAS(rollAxis,
				Vector3.forward * DeltaForce(CounterForce(0.0f, relativeAngularVelocity.z, rollForce)),
				Vector3.back * DeltaForce(SASClamp(rollClamp, relativeAngularVelocity.z, rollForce)) * rollAxis);
			//yaw
			SAS(yawAxis,
				Vector3.up * DeltaForce(CounterForce(0.0f, relativeAngularVelocity.y, yawForce)),
				Vector3.up * DeltaForce(SASClamp(yawClamp, relativeAngularVelocity.y, yawForce)) * yawAxis);
		//if SAS mode is off
		} else {
			//no automatic counter thrust
			m_rb.AddRelativeTorque(Vector3.left * DeltaForce(pitchForce) * pitchAxis);
			m_rb.AddRelativeTorque(Vector3.back * DeltaForce(rollForce) * rollAxis);
			m_rb.AddRelativeTorque(Vector3.up * DeltaForce(yawForce) * yawAxis);
		}
	}
	// Stability Assist System slowdown
	private void SAS(float axes, Vector3 retrograde, Vector3 prograde){
		//if no input detected
		if (axes == 0)
			//activate retrograde thrusters
			m_rb.AddRelativeTorque(retrograde);
		//if input detected
		else
			//activate prograde thrusters
			m_rb.AddRelativeTorque(prograde);
	}
	private float SASClamp(float clamp, float currVel, float force){
		if (-clamp < currVel && currVel < clamp)
			return force;
		return 0.0f;
	}

	// Translate ship based on assist mode
	private void ShipThrust(bool assist){
		//if ABS mode is on
		if (assist){
			//use ABS "smart" counterthrust
			//surge
			ABS(surgeAxis,
				Vector3.forward * DeltaForce(CounterForce(0.0f, relativeTranslationalVelocity.z, forwardForce)),
				Vector3.forward * DeltaForce(forwardForce) * surgeAxis);
			//sway
			ABS(swayAxis,
				Vector3.right * DeltaForce(CounterForce(0.0f, relativeTranslationalVelocity.x, transForce)),
				Vector3.right * DeltaForce(transForce) * swayAxis);
			//heave
			ABS(heaveAxis,
				Vector3.up * DeltaForce(CounterForce(0.0f, relativeTranslationalVelocity.y, transForce)),
				Vector3.up * DeltaForce(transForce) * heaveAxis);
		}
		//if ABS mode is off
		else
		{
			//no automatic counter thrust
			m_rb.AddRelativeForce(Vector3.forward * DeltaForce(forwardForce) * surgeAxis, ForceMode.Force);
			m_rb.AddRelativeForce(Vector3.right * DeltaForce(transForce) * swayAxis, ForceMode.Force);
			m_rb.AddRelativeForce(Vector3.up * DeltaForce(transForce) * heaveAxis, ForceMode.Force);
		}
	}
	// Automatic Braking System
	private void ABS(float axes, Vector3 retrograde, Vector3 prograde){
		//if no input detected
		if (axes == 0)
			//activate retrograde thrusters
			m_rb.AddRelativeForce(retrograde, ForceMode.Force);
		//if input detected
		else
			//activate prograde thrusters
			m_rb.AddRelativeForce(prograde, ForceMode.Force);
	}

	// WIP Space brake system
	private void SpaceBrake(){
		m_rb.AddRelativeForce(Vector3.right
			* DeltaForce(CounterForce(0.0f, relativeTranslationalVelocity.x, transForce)), ForceMode.Force);
		m_rb.AddRelativeForce(Vector3.up
			* DeltaForce(CounterForce(0.0f, relativeTranslationalVelocity.y, transForce)), ForceMode.Force);
	}

	// Helper function for calculating counter force 
	private float CounterForce(float target, float vel, float force){
		//if velocity is negative
		if (vel < target)
			//return positive force
			return force;
		//if velocity is positve
		else if (vel > target)
			//return negative force
			return -force;
		//if velocity is 0, don't do anything
		return 0.0f;
	}

	// Helper function to get force over time
	private float DeltaForce(float force){
		return force * Time.deltaTime;
	}

	// Collection of debug logs
	private void DisplayDebug(){
		if (DEBUG) Debug.Log("Speed = " + m_rb.velocity.magnitude + " m/s");
		if (DEBUG) Debug.Log("SAS = " + keySAS + "\nrelativeTranslationalVelocity = " + relativeTranslationalVelocity);
		if (DEBUG) Debug.Log("ABS = " + keyABS + "\nrelativeAngularVelocity = " + relativeAngularVelocity);
	}
}
 