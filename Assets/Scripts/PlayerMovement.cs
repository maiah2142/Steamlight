using UnityEngine;

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
		relAngVel = trans.InverseTransformDirection(rb.angularVelocity);
		relTranVel = trans.InverseTransformDirection(rb.velocity);

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
				pitchAxis,
				Vector3.right,
				pitchForce,
				0,
				pitchClamp
			);
			//yaw
			SAS(
				yawAxis,
				Vector3.up,
				yawForce,
				1,
				yawClamp
			);
			//roll
			SAS(
				rollAxis,
				Vector3.forward,
				rollForce,
				2,
				rollClamp
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
			//if current velocity is within the positive and negative force
			if (relAngVel[relIndex] < force * Time.deltaTime &&
			    relAngVel[relIndex] > force * Time.deltaTime * -1){
				//set the velocity of current axis to 0
				Vector3 vel = relAngVel;
				vel[relIndex] = 0.0f;
				rb.angularVelocity = transform.TransformDirection(vel);
			}
			//if the current velocity of the axis is positive
			if (relAngVel[relIndex] > 0){
				//apply negative force to the rigid body
				rb.AddRelativeTorque(dirVector * force * -1, ForceMode.Force);
			//if the current velocity of the axis is negative
			} else if (relAngVel[relIndex] < 0){
				//apply positive force to the rigid body
				rb.AddRelativeTorque(dirVector * force, ForceMode.Force);
			}
		//if there is user input on a particular axis
		} else {
			//apply force to rigid body using the clamp method
			//limits the turn rate to a specified velocity

			float clamp = maxClamp * axis;
			//if the current velocity of the axis is positive
			if (relAngVel[relIndex] > clamp){
				//apply negative force to the rigid body
				rb.AddRelativeTorque(dirVector * force * -1, ForceMode.Force);
			//if the current velocity of the axis is negative
			} else if (relAngVel[relIndex] < clamp){
				//apply positive force to the rigid body
				rb.AddRelativeTorque(dirVector * force, ForceMode.Force);
			}
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
		//forward and back
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
			//if current velocity is within the positive and negative force
			if (relTranVel[relIndex] < posForce / rb.mass  * Time.deltaTime &&
			    relTranVel[relIndex] > negForce / rb.mass * Time.deltaTime * -1){
				//set the velocity of current axis to 0
				Vector3 vel = relTranVel;
				vel[relIndex] = 0.0f;
				rb.velocity = transform.TransformDirection(vel);
			}
			//if the current velocity of the axis is positive
			if (relTranVel[relIndex] > 0){
				//apply negative force to the rigid body
				rb.AddRelativeForce(dirVector * negForce * -1, ForceMode.Force);
			//if the current velocity of the axis is negative
			} else if (relTranVel[relIndex] < 0){
				//apply positive force to the rigid body
				rb.AddRelativeForce(dirVector * posForce, ForceMode.Force);
			}
		}
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