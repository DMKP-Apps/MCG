using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputController : MonoBehaviour {

	private GameController GameController;
    private CannonPlayerState playerState = null;

	public Camera sceneCamera;

    private CannonPlayerState GetPlayerState()
    {
        CannonPlayerState playerController = this.GetComponent<CannonPlayerState>();
        var parent = this.transform.parent;
        while (parent != null && playerController == null)
        {
            playerController = parent.GetComponent<CannonPlayerState>();
            if (playerController != null)
            {
                break;
            }
            parent = parent.parent;
        }
        return playerController;
    }

    private void Start()
	{
        
		m_OriginalRotation = transform.localRotation;
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
        playerState = GetPlayerState();

    }

	public Vector3 rotationRange = new Vector3(0, 0, 70);
	public float rotationSpeed = 10;
	public float dampingTime = 0.2f;

	private Vector3 m_TargetAngles = new Vector3();
	private Vector3 m_FollowAngles = new Vector3();
	private Vector3 m_FollowVelocity = new Vector3();
	private Quaternion m_OriginalRotation;

	public Vector2 InputPosition = new Vector3(0, 0);
	private string previousPosition = string.Empty;

	public void Reset() {
		m_TargetAngles = new Vector3();
		m_FollowAngles = new Vector3();
		m_FollowVelocity = new Vector3();
	}

    private bool isMoving = false;

    public bool AllowInput = true;

	public float perspectiveZoomSpeed = 0.5f;        // The rate of change of the field of view in perspective mode.
	public float orthoZoomSpeed = 0.5f;        // The rate of change of the orthographic size in orthographic mode.

    private List<Vector2> dragInput = new List<Vector2>();

	// Update is called once per frame
	void Update () {

		InputPosition = new Vector2(0, 0);
        bool hasTouch = false;

		var touches = Input.touches.ToList ();

		if (touches.Count > 0) {
			GameController.TouchDetected ();
			hasTouch = true;

		}

		var cameraController = sceneCamera.gameObject.GetComponent<MainCameraController> ();
		if (touches.Count == 2 && sceneCamera != null && !touches.Any(x => !(x.phase == TouchPhase.Moved || x.phase == TouchPhase.Stationary))) {

			if (!AllowInput) {
				return;
			}

			// Store both touches.
			Touch touchZero = touches [0];
			Touch touchOne = touches [1];

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // disable this function for now
            if (1 == 2)
            {
                if (deltaMagnitudeDiff > 0 && cameraController.Mode != MainCameraController.CameraMode.Explore)
                {
                    // zoom out.
                    cameraController.Mode = MainCameraController.CameraMode.Explore;
                    cameraController.canInputMovement = false;
                }
                else if (deltaMagnitudeDiff < 0 && cameraController.Mode == MainCameraController.CameraMode.Explore && cameraController.canInputMovement)
                {
                    // zoom in.
                    cameraController.Mode = MainCameraController.CameraMode.FollowCannon;


                }
            }

			
            
		
		} 

		if(cameraController.Mode == MainCameraController.CameraMode.Explore && cameraController.canInputMovement && touches.Count == 1)
		{
			touches.Where(x => x.phase == TouchPhase.Moved).Take(1).ToList().ForEach (touch => {


				if (touch.phase == TouchPhase.Moved) {
					if (!AllowInput) {
						return;
					}

					hasTouch = true;

					var followPosition = (new Vector3(touch.deltaPosition.x * 1.5f, 0f, touch.deltaPosition.y * 5f));

					cameraController.explorePosition = sceneCamera.transform.position + followPosition;

					/*var minStep = 2f * Time.deltaTime;
					var step = (Vector3.Distance (followPosition, camera.transform.position) * 0.8f) * Time.deltaTime;
					if (step < minStep) {
						step = minStep;
					}

					camera.transform.position = Vector3.MoveTowards (camera.transform.position, followPosition, step);*/



				}
			});
		}


        if (cameraController.Mode == MainCameraController.CameraMode.FollowCannon)
        {


            touches.Where(x => x.phase == TouchPhase.Moved).Take(1).ToList().ForEach(touch =>
            {


                if (touch.phase == TouchPhase.Moved)
                {
                    if (!AllowInput)
                    {
                        return;
                    }

                    hasTouch = true;

                    if (!playerState.isFiring())
                    {
                        transform.localRotation = SmoothRotator.Rotate(transform.localRotation, ref m_OriginalRotation,
                            ref m_TargetAngles, ref m_FollowAngles,
                            ref m_FollowVelocity, rotationRange, rotationSpeed,
                            dampingTime, touch.deltaPosition.x, touch.deltaPosition.y);


                        //m_OriginalRotation = transform.localRotation;

                        var currentRotation = string.Format("{0},{1},{2}",
                                                                transform.localRotation.x.ToString("0000.0000"),
                                                                transform.localRotation.y.ToString("0000.0000"),
                                                                transform.localRotation.z.ToString("0000.0000"));



                        if (previousPosition != currentRotation)
                        {
                            previousPosition = currentRotation;
                            InputPosition.x = touch.deltaPosition.x;
                            InputPosition.y = touch.deltaPosition.y;

                        }

                    }


                }
            });
        }

        if (cameraController.Mode == MainCameraController.CameraMode.FollowBullet)
        {
            touches.Where(x => x.phase == TouchPhase.Moved).Take(1).ToList().ForEach(touch =>
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    if (!AllowInput)
                    {
                        return;
                    }

                    hasTouch = true;

                    
                    dragInput.Add(touch.deltaPosition);

                }
            });

            if (touches.Count == 0 && dragInput.Count > 0)
            {
                //var maxForce = 30f;
                var currentBullet = GameController.GetCurrentShotBullet();
                if (currentBullet != null)
                {
                    Vector2 input = new Vector2();
                    input = dragInput.OrderByDescending(x => x.sqrMagnitude).FirstOrDefault();
                    //dragInput.ForEach(x => input += x);
                    dragInput = new List<Vector2>();
                    //if (input.sqrMagnitude > 100)
                    //{
                    //    input = input * (100 / input.sqrMagnitude);
                    //    input *= 5000f;
                    //}

                    //if(input.x > maxForce)
                    //input = input * 0.4f;

                    var bulletController = currentBullet.GetComponent<BulletHitController>();
                    if (bulletController != null)
                    {
                        bulletController.AddSpin(input);
                    }
                }
            }

        }

        if (!hasTouch)
        {
            if (Input.GetMouseButtonDown(0))
            {
                GameController.TouchDetected();
                isMoving = !isMoving;
            };

			if (!AllowInput) {
				return;
			}

            if (isMoving)
            {
                var y = Input.GetAxis("Mouse Y") * 10;
                var x = Input.GetAxis("Mouse X") * 10;
                //Debug.Log(string.Format("{0}.{1}", y, x));
                if (!playerState.isFiring())
                {
                    transform.localRotation = SmoothRotator.Rotate(transform.localRotation, ref m_OriginalRotation,
                                ref m_TargetAngles, ref m_FollowAngles,
                                ref m_FollowVelocity, rotationRange, rotationSpeed * 2,
                                dampingTime, x, y);
                    
                    var currentRotation = string.Format("{0},{1},{2}",
                        transform.localRotation.x.ToString("0000.0000"),
                        transform.localRotation.y.ToString("0000.0000"),
                        transform.localRotation.z.ToString("0000.0000"));

                    if (previousPosition != currentRotation)
                    {
                        previousPosition = currentRotation;
                        InputPosition.x = x;
                        InputPosition.y = y;

                    }

                }

            }
        }
    }
}
