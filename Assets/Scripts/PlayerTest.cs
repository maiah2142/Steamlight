﻿using UnityEngine;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerRCS))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Transform))]

public class PlayerTest : MonoBehaviour {
	public Rigidbody rb;
	public Transform trans;
	PlayerRCS rcs;

	private const bool DEBUG = false;


	//defines for force
	private const float PI = (float)Math.PI;
	private float pitchForce = 3*PI/2;
	private float rollForce = 3*PI/2;
	private float yawForce = PI/2;
	private float forwardForce = 8.0f;
	private float backwardForce = 4.0f;
	private float transUpForce = 6.0f;
	private float transDownForce = 4.0f;
	private float transLeftForce = 4.0f;
	private float transRightForce = 4.0f;

	private float pitchClamp = 3*PI/4;
	private float rollClamp = 3*PI/4;
	private float yawClamp = PI/4;

	//Define input axes
	private float pitchAxis, rollAxis, yawAxis, surgeAxis, swayAxis, heaveAxis;

	private bool keySAS, keyABS;

	private Vector3 relAngVel, relTranVel;

	void Start(){
		rb = GetComponent<Rigidbody>();
		rb.inertiaTensorRotation = Quaternion.identity;
		rb.centerOfMass = Vector3.zero;
		Debug.Log(rb.mass);


		trans = GetComponent<Transform>();
		rcs = gameObject.AddComponent<PlayerRCS>() as PlayerRCS;

		keySAS = true; 
		keyABS = true;
	}

	// Update is called once per frame
	void Update(){
		//grab axis of controller
		pitchAxis = Input.GetAxis("Pitch");
		rollAxis = Input.GetAxis("Roll");
		yawAxis = Input.GetAxis("Yaw");
		surgeAxis = Input.GetAxis("Surge");
		swayAxis = Input.GetAxis("Sway");
		heaveAxis = Input.GetAxis("Heave");

		//grab button input
		//TODO: Fix this
		if (Input.GetButton("SAS"))
			keySAS = !keySAS;
		if (Input.GetButton("ABS"))
			keyABS = !keyABS;
	}

	// Update game at fixed intervals
	void FixedUpdate(){
		//update relative rotation and translation velocities
		updateRelVel();

		rotateShip();
		moveShip();

		if (DEBUG) displayDebug();
	}

	// Collective method for all ship rotation
	private void rotateShip(){
		//pitch
		rotateAxis(
			pitchAxis, //x axis
			Vector3.right, //direction vector
			pitchForce, //force
			0, //index for x
			pitchClamp //max force for SAS
		);
		//yaw
		rotateAxis(
			yawAxis, //y axis
			Vector3.up, //direction vector
			yawForce, //force
			1, //index for y
			yawClamp //max force for SAS
		);
		//roll
		rotateAxis(
			rollAxis, //z axis
			Vector3.forward, //direction vector
			rollForce, //force
			2, //index for z
			rollClamp //max force for SAS
		);

	}

	// Logic for rotating an individual axis
	private void rotateAxis(float axis, Vector3 dirVector, float force, int relIndex, float maxClamp){
		//if SAS is off (manual mode)
		if (!keySAS){
			//if axis is positive
			if (axis > 0.0f){
				rb.AddRelativeTorque(dirVector * force * axis, ForceMode.Force);
				rcs.PlayParticle(relIndex, true, axis);
			//if axis is negative
			} else if (axis < 0.0f){
				rb.AddRelativeTorque(dirVector * force * axis, ForceMode.Force);
				rcs.PlayParticle(relIndex, false, axis);
			//if no input
			} else {
				rcs.StopParticle(relIndex, true);
				rcs.StopParticle(relIndex, false);
			}
		//if SAS is on (automatic drag mode) 
		} else {
			if (axis == 0){
				zeroOutVel(rcs.StopParticle, setAngVel, ref relAngVel, force, force, relIndex, 0.0f);
				matchVel(rcs.PlayParticle, rcs.StopParticle, rb.AddRelativeTorque, relAngVel, dirVector,
				force, force, relIndex, 0.0f);
			//if there is user input on a particular axis
			} else {
				//apply force to rigid body using the clamp method
				//limits the turn rate to a specified velocity
				float clamp = maxClamp * axis;
				zeroOutVel(rcs.StopParticle, setAngVel, ref relAngVel, force, force, relIndex, clamp);
				matchVel(rcs.PlayParticle, rcs.StopParticle, rb.AddRelativeTorque, relAngVel, dirVector,
				force, force, relIndex, clamp);
			}
		}
	}
	// Collective method for all ship translations
	private void moveShip(){
		//starboard and port
		moveOnAxis(
			swayAxis, //x axis
			Vector3.right, //direction vector
			transRightForce, //positive force
			transLeftForce, //negative force
			0 //index for x
		);
		//overhead and deck
		moveOnAxis(
			heaveAxis, //y axis
			Vector3.up, //direction vector
			transUpForce, //positive force
			transDownForce, //negative force
			1 //index for y
		);
		//bow and aft
		moveOnAxis(
			surgeAxis, //z axis
			Vector3.forward, //direction vector
			forwardForce, //positive force
			backwardForce, //negative force
			2 //index for z
		);
	}
	// Ship thrust for individual axis
	private void moveOnAxis(float axis, Vector3 dirVector, float posForce, float negForce, int relIndex){
		//if axis is positive
		if (axis > 0.0f){
			rb.AddRelativeForce(dirVector * posForce * axis, ForceMode.Force);
		//if axis is negative
		} else if (axis < 0.0f){
			rb.AddRelativeForce(dirVector * negForce * axis, ForceMode.Force);
		//if no input and ABS system is on
		} else if (keyABS){
			//if vel is not forward on relative z axis
			if (relIndex != 2 || relTranVel[2] < 0){
				zeroOutVel(placeholderAnimation2, setVel, ref relTranVel, posForce/rb.mass, negForce/rb.mass, relIndex, 0.0f);
				matchVel(placeholderAnimation, placeholderAnimation2, rb.AddRelativeForce, relTranVel,
				dirVector, posForce/rb.mass, negForce/rb.mass,
					relIndex, 0.0f);
			}
		}
	}
	
	// Level off the velocity of an axis to a target velocity if it is near enough to target
	private void zeroOutVel(Action<int, bool> stopAnimation, Action<Vector3> changeVel, ref Vector3 relVel,
			float posForce, float negForce, int relIndex, float target){
		//if current velocity is within the positive and negative force of target
		if (relVel[relIndex] < target + (negForce * Time.deltaTime) &&
		    relVel[relIndex] > target - (posForce * Time.deltaTime)){
			//set local velocity to target
			Vector3 vel = relVel;
			vel[relIndex] = target;
			//update global Vector3 relative velocity to new velocity
			relVel = vel;
			//convert local vector to world vector
			changeVel(transform.TransformDirection(vel));
			stopAnimation(relIndex, false);
			stopAnimation(relIndex, true);
		}
	}

	// Apply full counter force to rigid body to match a velocity
	private void matchVel(Action<int, bool, float> playAnimation, Action<int, bool> stopAnimation,
			Action<Vector3, ForceMode> applyForce, Vector3 relVel, Vector3 dirVector, float posForce,
			float negForce, int relIndex, float target){
		//if the current velocity of the axis is greater than the target
		if (relVel[relIndex] > target){
			//apply negative force to the rigid body
			applyForce(dirVector * -negForce, ForceMode.Force);
			stopAnimation(relIndex, true);
			playAnimation(relIndex, false, 1);
		//if the current velocity of the axis is less than the target
		} else if (relVel[relIndex] < target){
			//apply positive force to the rigid body
			applyForce(dirVector * posForce, ForceMode.Force);
			stopAnimation(relIndex, false);
			playAnimation(relIndex, true, 1);
		} else {
			stopAnimation(relIndex, true);
			stopAnimation(relIndex, false);
		}
	}

	// Update relative velocities of both angular and translation
	private void updateRelVel(){
		relAngVel = transform.InverseTransformDirection(rb.angularVelocity);
		relTranVel = transform.InverseTransformDirection(rb.velocity);
	}

	// set rb's velocity
	private void setVel(Vector3 tranVel){
		rb.velocity = tranVel;
	}

	// set rb's angular velocity
	private void setAngVel(Vector3 angVel){
		rb.angularVelocity = angVel;
	}

	// Collection of debug logs
	private void displayDebug(){
		Debug.Log("Speed = " + rb.velocity.magnitude + " m/s");
		Debug.Log("ABS = " + keyABS);
		Debug.Log("relTranVel = " + relTranVel);
		Debug.Log("SAS = " + keySAS);
		Debug.Log("relAngVel = " + relAngVel);
	}

	private void placeholderAnimation(int axisIndex, bool pos, float axis){
	}
	private void placeholderAnimation2(int axisIndex, bool pos){
	}
}