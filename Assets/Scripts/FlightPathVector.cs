using UnityEngine;
using System;

public class FlightPathVector : MonoBehaviour {
	private Rigidbody rb;
	private Transform trans;
	private Vector3 relTranVel;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		trans = GetComponent<Transform>();
	}
	
	// Update game at fixed intervals
	void FixedUpdate () {
		//update relative translation velocity
		relTranVel = transform.InverseTransformDirection(rb.velocity);

		//debug output in BSG format
		Debug.Log("Bearing: " + String.Format("{0,3}", CalcBearing()) +
			  ", Carom: " + String.Format("{0,3}", CalcMark()));
	}
	
	// Calculate the closest angle in degrees from ship to current horizontal vector
	private float CalcRawBearing(){
		return Mathf.Atan2(relTranVel.x, relTranVel.z) * Mathf.Rad2Deg;
	}
	// Rounds the horizontal angle to integer, angle outputted as unit circle in degrees  
	private int CalcBearing(){
		int hAngle = (int)Math.Round(CalcRawBearing(), 0);
		if (hAngle < 0)
			hAngle = hAngle + 360;
		return hAngle;
	}

	// Calculate the closest angle in degrees from ship to current vertical vector
	private float CalcRawMark(){
		float hPy = Mathf.Sqrt(Mathf.Pow(relTranVel.x, 2) + Mathf.Pow(relTranVel.z, 2));
		return Mathf.Atan2(relTranVel.y, hPy) * Mathf.Rad2Deg;
	}
	// Rounds the vertical angle to integer, angle outputted as unit circle in degrees  
	private int CalcMark(){
		int vAngle = (int)Math.Round(CalcRawMark(), 0);
		if (vAngle < 0)
			vAngle = vAngle + 360;
		return vAngle;
	}
}
