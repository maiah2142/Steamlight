using UnityEngine;
using UnityEngine.UI;

public class SpeedHUD : MonoBehaviour {
	public Rigidbody rb;
	public Text speedText;

	// Update is called once per frame
	void Update () {
		speedText.text = rb.velocity.magnitude.ToString("F2") + " m/s";
	}
}
