using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputController : MonoBehaviour {

	private GameController GameController;

	private void Start()
	{
		m_OriginalRotation = transform.localRotation;
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
	}

	public Vector3 rotationRange = new Vector3(0, 0, 70);
	public float rotationSpeed = 10;
	public float dampingTime = 0.2f;

	private Vector3 m_TargetAngles;
	private Vector3 m_FollowAngles;
	private Vector3 m_FollowVelocity;
	private Quaternion m_OriginalRotation;

	public Vector2 InputPosition = new Vector3(0, 0);
	private string previousPosition = string.Empty;

	// Update is called once per frame
	void Update () {
		InputPosition = new Vector2(0, 0);
		Input.touches.ToList ().ForEach (touch => {
			if(touch.phase == TouchPhase.Began) {
				GameController.TouchDetected();

			}
			if(touch.phase == TouchPhase.Moved) {
				// Construct a ray from the current touch coordinates

				//var ray = Camera.main.ScreenPointToRay (touch.position);
				//Debug.Log(string.Format("x: {0}, y: {1}", ray.direction.x, ray.direction.y));
				//var hits = Physics.RaycastAll(ray);
				//Debug.Log(string.Format("hits: {0}", hits.Length));

				//hits.ToList().Where(hit => hit.collider != null).ToList()
				//	.ForEach(hit => {

				//		Debug.Log(string.Format("name: {0}", hit.collider.gameObject.name));
				//		if(hit.collider.gameObject.name == this.name) {

							//float step = Speed * Time.deltaTime;
							//transform.position = Vector3.MoveTowards(transform.position, 
							//	new Vector3(touch.deltaPosition.x,0.0f,touch.deltaPosition.y), step);
							//transform.Rotate(touch.deltaPosition.y,0.0f,0.0f);
							transform.localRotation = SmoothRotator.Rotate( transform.localRotation, ref m_OriginalRotation,
								ref m_TargetAngles, ref m_FollowAngles,
								ref m_FollowVelocity, rotationRange, rotationSpeed,
								dampingTime,touch.deltaPosition.x, touch.deltaPosition.y);



							var currentRotation = string.Format("{0},{1},{2}", 
								transform.localRotation.x.ToString("0000.0000"),
								transform.localRotation.y.ToString("0000.0000"),
								transform.localRotation.z.ToString("0000.0000"));



							if(previousPosition != currentRotation)
							{
								previousPosition = currentRotation;
								InputPosition.x = touch.deltaPosition.x;
								InputPosition.y = touch.deltaPosition.y;

							}




						//}
					//});
			}
		});
	}
}
