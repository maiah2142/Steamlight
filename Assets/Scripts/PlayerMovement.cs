﻿using UnityEngine;

public class PlayerMovement : MonoBehaviour {
	private Rigidbody rb;
	private Transform trans;

	private const bool DEBUG = true;

	//defines for force
	private const float pitchForce = 6.0f;
	private const float rollForce = 6.0f;
	private const float yawForce = 3.0f;
	private const float forwardForce = 8.0f;
	private const float backwardForce = 4.0f;
	private const float transUpForce = 6.0f;
	private const float transDownForce = 4.0f;
	private const float transLeftForce = 4.0f;
	private const float transRightForce = 4.0f;

	private const float pitchClamp = 3.0f;
	private const float rollClamp = 3.0f;
	private const float yawClamp = 1.5f;

	//Define
	private float pitchAxis, rollAxis, yawAxis, surgeAxis, swayAxis,
		heaveAxis;

	private bool keySAS, keyABS;

	private Vector3 relAngVel, relTranVel;

	private void Start(){
		rb = GetComponent<Rigidbody>();
		trans = GetComponent<Transform>();

		keySAS = true;
		keyABS = true;
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
		//TODO: Fix this
		if (Input.GetButton("SAS"))
			keySAS = !keySAS;
		if (Input.GetButton("ABS"))
			keyABS = !keyABS;
	}

	// Update game at fixed intervals
	private void FixedUpdate(){
		//update relative rotation and translation velocities
		relAngVel = trans.InverseTransformDirection(rb.angularVelocity);
		relTranVel = trans.InverseTransformDirection(rb.velocity);

		ShipRotation();
		ShipTranslation();

		DisplayDebug();
	}

	// Rotates ship based on SAS mode
	private void ShipRotation(){
		//if SAS mode is on
		if (keySAS){
			//use SAS "smart" counterthrust
			/*
				Vector3 temp = Vector3.left * pitchForce
					* pitchAxis;
				Vector3.up * yawForce * yawAxis);
			*/
			//pitch
			SAS(pitchAxis,
				Vector3.right * CounterForce(0.0f,
					relAngVel.x, pitchForce),
				Vector3.left * SASClamp(pitchClamp,
					relAngVel.x, pitchForce) * pitchAxis);
			//roll
			SAS(rollAxis,
				Vector3.forward * CounterForce(0.0f, relAngVel.z, rollForce),
				Vector3.back * SASClamp(rollClamp, relAngVel.z, rollForce) * rollAxis);
			//yaw
			SAS(yawAxis,
				Vector3.up * CounterForce(0.0f, relAngVel.y, yawForce),
				Vector3.up * SASClamp(yawClamp, relAngVel.y, yawForce) * yawAxis);
		//if SAS mode is off
		} else {
			//no automatic counter thrust
			rb.AddRelativeTorque(Vector3.left * pitchForce * pitchAxis, ForceMode.Force);
			rb.AddRelativeTorque(Vector3.back * rollForce * rollAxis, ForceMode.Force);
			rb.AddRelativeTorque(Vector3.up * yawForce * yawAxis, ForceMode.Force);
		}
	}
	// Stability Assist System slowdown
	private void SAS(float axis, Vector3 retrograde, Vector3 prograde){
		//if no input detected
		if (axis == 0)
			rb.AddRelativeTorque(retrograde, ForceMode.Force);
		//if input detected
		else
			rb.AddRelativeTorque(prograde, ForceMode.Force);
	}
	private float SASClamp(float clamp, float currVel, float force){
		if (-clamp < currVel && currVel < clamp)
			return force;
		return 0.0f;
	}

	private void ShipRCS(float axis, Vector3 dirVector, float posForce,
			float negForce, float relativeDir){
		//if axis is positive
		if (axis > 0.0f){
			rb.AddRelativeTorque(dirVector * posForce * axis,
				ForceMode.Force);
		//if axis is negative
		} else if (axis < 0.0f){
			rb.AddRelativeForce(dirVector * negForce * axis,
				ForceMode.Force);
		}
	}

	// Translate whole ship with the ShipThrust function
	private void ShipTranslation(){
		//forward and back
		ShipThrust(
			surgeAxis, //z axis
			Vector3.forward, //direction vector
			forwardForce, //positive force
			backwardForce, //negative force
			2 //index for z
		);
		//right and left
		ShipThrust(
			swayAxis, //x axis
			Vector3.right, //direction vector
			transRightForce, //positive force
			transLeftForce, //negative force
			0 //index for x
		);
		//up and down
		ShipThrust(
			heaveAxis, //y axis
			Vector3.up, //direction vector
			transUpForce, //positive force
			transDownForce, //negative force
			1 //index for y
		);
	}
	// Ship thrust for individual axis
	private void ShipThrust(float axis, Vector3 dirVector, float posForce,
			float negForce, int relIndex){
		//if axis is positive
		if (axis > 0.0f){
			rb.AddRelativeForce(dirVector * posForce * axis,
				ForceMode.Force);
		//if axis is negative
		} else if (axis < 0.0f){
			rb.AddRelativeForce(dirVector * negForce * axis,
				ForceMode.Force);
		//if no input and ABS system is on
		} else if (keyABS){
			//Provide artificial drag to the ship when a thrust 
			//vector is not used
			if (relTranVel[relIndex] < posForce / rb.mass 
					* Time.deltaTime &&
				relTranVel[relIndex] > negForce / rb.mass 
					* Time.deltaTime){	
				Vector3 vel = relTranVel;
				vel[relIndex] = 0.0f;
				rb.velocity = transform.TransformDirection(vel);
			}
			if (relTranVel[relIndex] > 0){
				rb.AddRelativeForce(dirVector * negForce * -1,
					ForceMode.Force);
			} else if (relTranVel[relIndex] < 0){
				rb.AddRelativeForce(dirVector * posForce,
					ForceMode.Force);
			}
		}
	}

	// Helper function for calculating counter force 
	private float CounterForce(float target, float vel, float force){
		//if velocity is negative
		if (vel < target)
			return force; //positive value
		//if velocity is positve
		else if (vel > target)
			return -force; //negative value
		//if velocity is 0, don't do anything
		return 0.0f;
	}
	
	// Collection of debug logs
	private void DisplayDebug(){
		Debug.Log("Speed = " + rb.velocity.magnitude + " m/s");
		Debug.Log("ABS = " + keyABS);
		Debug.Log("relTranVel = " + relTranVel);
		Debug.Log("SAS = " + keySAS);
		Debug.Log("relAngVel = " + relAngVel);

		float hAngle = Mathf.Atan2(relTranVel.x, relTranVel.z) * Mathf.Rad2Deg;
		float hPy = Mathf.Sqrt(Mathf.Pow(relTranVel.x, 2) + Mathf.Pow(relTranVel.z, 2));
		float vAngle = Mathf.Atan2(relTranVel.y, hPy) * Mathf.Rad2Deg;

		Debug.Log("hAngle = " + hAngle);
		Debug.Log("vAngle = " + vAngle);
	}
}