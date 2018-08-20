using UnityEngine;

public class MouseLook : MonoBehaviour {
	GameObject player;
	Vector2 mouseLook;
	public float sensitivity = 3.0f;
	private const float xClamp = 140.0f;
	private const float yClamp = 80.0f;
	
	void Start(){
		//removes cursor from game
		Cursor.lockState = CursorLockMode.Locked;
		//get parent object transform info
		player = this.transform.parent.gameObject;
	}
	
	void Update(){
		//get mouse input
		Vector2 deltaMouse = new Vector2(Input.GetAxisRaw("Head Look X"), Input.GetAxisRaw("Head Look Y"));
		//normalise input by time passed
		mouseLook += deltaMouse * sensitivity * Time.deltaTime;

		//restrict freelook range
		mouseLook.x = Mathf.Clamp(mouseLook.x, -xClamp, xClamp);
		mouseLook.y = Mathf.Clamp(mouseLook.y, -yClamp, yClamp);

		//calc rotation
		this.transform.rotation = player.transform.rotation;
		this.transform.localRotation = Quaternion.Euler(-mouseLook.y, mouseLook.x, 0.0f);
	}
}