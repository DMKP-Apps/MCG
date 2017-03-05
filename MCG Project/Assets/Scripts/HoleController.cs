using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HoleController : MonoBehaviour {

	public int Par = 3;

	private List<Transform> raceTeePositions = null;
	private Transform teePosition = null;
    private List<Transform> checkPointPositions;
    private Transform holePosition = null;

	public bool allPlayersComplete = false;	
	public GameObject StartCamera;
	public GameObject EndCamera;
	public CameraPathAnimator EndCameraAnimator;
	public GameController GameController;

	public string HoleTitle = string.Empty;

	public int showPlayerHoleCompleteFor = 5000; 
	private System.DateTime startTime = System.DateTime.MaxValue;

	void Start() {
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();
		if (string.IsNullOrEmpty (HoleTitle)) {
			HoleTitle = gameObject.name;
		}
	}

	public Transform tee {
		get { 
			if (teePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Tee") {
						teePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return teePosition;
		}
	}

    public List<Transform> CheckPoints
    {
        get
        {
            if (checkPointPositions == null)
            {
                checkPointPositions = getAllByTag("CheckPoint", this.transform);
                if (tee != null)
                {
                    checkPointPositions = checkPointPositions.OrderBy(x => Vector3.Distance(x.position, tee.position)).ToList();
                }
                if (hole != null)
                {
                    checkPointPositions.Add(hole);
                }
            }
            return checkPointPositions;
        }
    }

    public Transform GetNextCheckPointLocation(Transform currentLocation)
    {
        float maxShotDistance = 240f;
        Transform checkPoint = null;
        int i = 0;
        checkPoint = CheckPoints.Select(x => new
        {
            Magnitude = currentLocation.position.magnitude - x.position.magnitude,
            Distance = Vector3.Distance(currentLocation.position, x.position),
            Name = x.gameObject.name,
            Transform = x,
            index = i++
        })
        .ToList()
        .Where(x => x.Distance < maxShotDistance)
        .OrderByDescending(x => x.index)
        .Select(x => x.Transform)
        .FirstOrDefault();//.ForEach(x => Debug.Log(string.Format("Mag: {0}, Distance: {1} - {2}", x.Magnitude, x.Distance, x.Name)));

        return checkPoint;
    }

    public float GetDistanceToPin(Transform currentLocation)
    {
        float maxShotDistance = 240f;
        int i = 0;
        int index = CheckPoints.Select(x => new
        {
            Magnitude = currentLocation.position.magnitude - x.position.magnitude,
            Distance = Vector3.Distance(currentLocation.position, x.position),
            Name = x.gameObject.name,
            Transform = x,
            index = i++
        })
        .ToList()
        .Where(x => x.Distance < maxShotDistance)
        .OrderByDescending(x => x.index)
        .Select(x => x.index)
        .FirstOrDefault();//.ForEach(x => Debug.Log(string.Format("Mag: {0}, Distance: {1} - {2}", x.Magnitude, x.Distance, x.Name)));

        var previousLocation = currentLocation.position;
        float distance = 0f;
        CheckPoints.Skip(index).ToList().ForEach(x => {
            distance += Vector3.Distance(previousLocation, x.position);
            previousLocation = x.position;
        });

        return distance;
    }

    private List<Transform> getAllByTag(string tag, Transform parent)
    {
        List<Transform> items = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child.tag == tag)
            {
                items.Add(child.gameObject.transform);
            }
            else if(child.childCount > 0)
            {
                items.AddRange(getAllByTag(tag, child));
            }
        }

        return items;
    }

    public List<Transform> raceTees {
		get { 
			if (raceTeePositions == null) {
				List<Transform> positions = new List<Transform> ();
				foreach (Transform child in this.transform) {
					if (child.tag == "TeeRace") {
						positions.Add(child.gameObject.transform);
					}
				}
				raceTeePositions = positions.OrderBy (x => x.gameObject.name).ToList ();
			}
			return raceTeePositions;
		}
	}

	public void HoleCompleted()
	{
		EndCamera.SetActive (true);
		EndCameraAnimator.startPercent = 0;
		EndCameraAnimator.Play ();


		if (!allPlayersComplete || GameSettings.playerMode == PlayerMode.ServerMultiplayer) {
			startTime = System.DateTime.Now;
		} else {
			startTime = System.DateTime.MaxValue;
		}

	}

	public void Update() {
		if (startTime != System.DateTime.MaxValue && EndCamera.activeInHierarchy) {
			if (System.DateTime.Now.Subtract (startTime).TotalMilliseconds > showPlayerHoleCompleteFor) {
				startTime = System.DateTime.MaxValue;
				DeactivateEndCamera ();
			}
		}
	}

	public Transform hole {
		get { 
			if (holePosition == null) {
				foreach (Transform child in this.transform) {
					if (child.tag == "Hole") {
						holePosition = child.gameObject.transform;
						break;
					}
				}
			}
			return holePosition;
		}
	}

	public void DeactivateHoleCameras() {
	
		if (StartCamera != null && StartCamera.activeInHierarchy) {
			DeactivateStartCamera ();
		}
		if (EndCamera != null && EndCamera.activeInHierarchy) {
			DeactivateEndCamera ();
		}

	
	}

	public void DeactivateStartCamera()
	{
		if (StartCamera != null) {
			StartCamera.SetActive (false);
		}
	}

	public void DeactivateEndCamera()
	{
		if (EndCamera != null) {
			EndCamera.SetActive (false);
			GameController.NextHole ();
			EndCameraAnimator.Stop ();
		}
	}


	public bool IsHoleCameraActive()
	{
		return StartCamera.activeInHierarchy || EndCamera.activeInHierarchy;
	}
}
