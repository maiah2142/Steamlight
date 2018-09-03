using UnityEngine;
using System;

public class PlayerMovement : MonoBehaviour {
	private Rigidbody rb;
	private Transform trans;
	ParticleSystem[] pitchUpRCS;
	ParticleSystem[] pitchDownRCS;

	private const bool DEBUG = true;	

	//defines for force
	private const float PI = (float)Math.PI;
	private const float pitchForce = 3*PI/2;
	private const float rollForce = 3*PI/2;
	private const float yawForce = PI/2;
	private const float forwardForce = 8.0f;
	private const float backwardForce = 4.0f;
	private const float transUpForce = 6.0f;
	private const float transDownForce = 4.0f;
	private const float transLeftForce = 4.0f;
	private const float transRightForce = 4.0f;

	private const float pitchClamp = pitchForce/2;
	private const float rollClamp = rollForce/2;
	private const float yawClamp = yawForce/2;

	//Define input axes
	private float pitchAxis, rollAxis, yawAxis, surgeAxis, swayAxis, heaveAxis;

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
		updateRelVel();
		ShipRotation();
		ShipTranslation();

		if (DEBUG) DisplayDebug();
	}

	// Rotates ship based on SAS mode
	private void ShipRotation(){
		//if SAS mode is on
		if (keySAS){
			//pitch
			SAS(
				pitchAxis, //x axis
				Vector3.right, //direction vector
				pitchForce, //force
				0, //index for x
				pitchClamp //max force for SAS
			);
			//yaw
			SAS(
				yawAxis, //y axis
				Vector3.up, //direction vector
				yawForce, //force
				1, //index for y
				yawClamp //max force for SAS
			);
			//roll
			SAS(
				rollAxis, //z axis
				Vector3.forward, //direction vector
				rollForce, //force
				2, //index for z
				rollClamp //max force for SAS
			);
		//if SAS mode is off
		} else {
			//no automatic counter thrust
			rb.AddRelativeTorque(Vector3.right * pitchForce * pitchAxis, ForceMode.Force);
			rb.AddRelativeTorque(Vector3.forward * rollForce * rollAxis, ForceMode.Force);
			rb.AddRelativeTorque(Vector3.up * yawForce * yawAxis, ForceMode.Force);
		}
	}
	// Stability Assist System
	private void SAS(float axis, Vector3 dirVector, float force, int relIndex, float maxClamp){
		//if no input
		if (axis == 0){
			VelLevelOff(setAngVel, ref relAngVel, force, force, relIndex, 0.0f);
			VelMatch(rb.AddRelativeTorque, relAngVel, dirVector, force, force, relIndex, 0.0f);
		//if there is user input on a particular axis
		} else {
			//apply force to rigid body using the clamp method
			//limits the turn rate to a specified velocity
			float clamp = maxClamp * axis;
			VelLevelOff(setAngVel, ref relAngVel, force, force, relIndex, clamp);
			VelMatch(rb.AddRelativeTorque, relAngVel, dirVector, force, force, relIndex, clamp);
		}
	}

	/*
	private void ShipRCS(float axis, Vector3 dirVector, float force, float relativeDir){
		//if axis is positive
		if (axis > 0.0f){
			rb.AddRelativeTorque(dirVector * force * axis, ForceMode.Force);
		//if axis is negative
		} else if (axis < 0.0f){
			rb.AddRelativeForce(dirVector * force * axis, ForceMode.Force);
		}
	}
	*/

	// Translate whole ship with the ShipThrust function
	private void ShipTranslation(){
		//starboard and port
		ShipThrust(
			swayAxis, //x axis
			Vector3.right, //direction vector
			transRightForce, //positive force
			transLeftForce, //negative force
			0 //index for x
		);
		//overhead and deck
		ShipThrust(
			heaveAxis, //y axis
			Vector3.up, //direction vector
			transUpForce, //positive force
			transDownForce, //negative force
			1 //index for y
		);
		//bow and aft
		ShipThrust(
			surgeAxis, //z axis
			Vector3.forward, //direction vector
			forwardForce, //positive force
			backwardForce, //negative force
			2 //index for z
		);
	}
	// Ship thrust for individual axis
	private void ShipThrust(float axis, Vector3 dirVector, float posForce, float negForce, int relIndex){
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
				VelLevelOff(setVel, ref relTranVel, posForce/rb.mass, negForce/rb.mass, relIndex, 0.0f);
				VelMatch(rb.AddRelativeForce, relTranVel, dirVector, posForce/rb.mass, negForce/rb.mass,
					relIndex, 0.0f);
			}
		}
	}
	
	// Level off the velocity to a target velocity if it is near enough to target
	private void VelLevelOff(Action<Vector3> changeVel, ref Vector3 relVel, float posForce, float negForce,
			int relIndex, float target){
		//if current velocity is within the positive and negative force of target
		if (relVel[relIndex] < target + (negForce * Time.deltaTime) &&
		    relVel[relIndex] > target - (posForce * Time.deltaTime)){
			//set local velocity to target
			Vector3 vel = relVel;
			vel[relIndex] = target;
			relVel = vel;
			//convert local vector to world vector
			changeVel(transform.TransformDirection(vel));
		}
	}

	// Apply full counter force to rigid body to match a velocity
	private void VelMatch(Action<Vector3, ForceMode> applyForce, Vector3 relVel, Vector3 dirVector,
			float posForce, float negForce, int relIndex, float target){
		//if the current velocity of the axis is greater than the target
		if (relVel[relIndex] > target){
			//apply negative force to the rigid body
			applyForce(dirVector * -negForce, ForceMode.Force);
		//if the current velocity of the axis is less than the target
		} else if (relVel[relIndex] < target){
			//apply positive force to the rigid body
			applyForce(dirVector * posForce, ForceMode.Force);
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
	private void DisplayDebug(){
		Debug.Log("Speed = " + rb.velocity.magnitude + " m/s");
		Debug.Log("ABS = " + keyABS);
		Debug.Log("relTranVel = " + relTranVel);
		Debug.Log("SAS = " + keySAS);
		Debug.Log("relAngVel = " + relAngVel);
	}
}