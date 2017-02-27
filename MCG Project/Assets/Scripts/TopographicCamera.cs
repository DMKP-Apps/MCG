using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopographicCamera : MonoBehaviour {

    private Camera tcamera;
    public float speed = 2.5f;

    private RectTransform rect = null;
    private Vector2 size;
    public float compareScaleTo = 1280f;
    private Rect originalRect;
    private float scalePercentage = -1f;
	private Vector3 originalPosition;
	private GameController GameController;

    // Use this for initialization
    void Start () {
        tcamera = this.GetComponent<Camera>();
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            rect = canvas.GetComponent<RectTransform>();
        }

		originalPosition = transform.position;
		GameController = GameObject.Find ("GameController").GetComponent<GameController> ();

    }
	
	// Update is called once per frame
	void Update () {
        if (scalePercentage == -1) {
            originalRect = tcamera.rect;
            scalePercentage = 1;
        }
        //Debug.Log(originalRect);
        var currentSize = new Vector2(rect.rect.width, rect.rect.height);
        if (size != currentSize)
        {
            size = currentSize;

            // determine the scale percentage
            //float maxValue = size.x;
            if (size.x > size.y)
            {
                var newRect = originalRect;
                newRect.width = 0.15f;
                newRect.height = 0.35f;
                tcamera.rect = newRect;
            }
            else
            {
                var newRect = originalRect;
                newRect.width = 0.2f;
                newRect.height = 0.2f;
                tcamera.rect = newRect;
            }

            //scalePercentage = maxValue / compareScaleTo;

            ////Debug.Log(scalePercentage);
            //transform.localScale = new Vector3(originalScale.x * scalePercentage, originalScale.y * scalePercentage, originalScale.z * scalePercentage);

            //var rtrans = this.GetComponent<RectTransform>();
            //rtrans.anchoredPosition = new Vector2(originalPosOffset.x * scalePercentage, originalPosOffset.y * scalePercentage);

        }

        

        //var step = speed * Time.deltaTime;
		var cameraZoom = originalPosition.y * (GameSettings.ShotPower.HasValue && GameSettings.ShotPower.Value < 0.6f ? 0.6f : GameSettings.ShotPower.HasValue ? GameSettings.ShotPower.Value : 1);
		var difference = (GameSettings.EstimatedShotLocation + ((GameSettings.EstimatedShotLocation + GameSettings.CurrentCannonLocation) / 2)) / 2; //
  //      if (!GameSettings.ShotPower.HasValue) {
		//	var distance1 = Vector3.Distance (GameController.GetHolePinPosition(), GameSettings.CurrentCannonLocation);
		//	var distance2 = Vector3.Distance (GameSettings.EstimatedShotLocation, GameSettings.CurrentCannonLocation);

		//	if (distance1 < distance2) {
  //              var percentage = distance1 / distance2;
  //              if (percentage < 0.6f) {
  //                  percentage = 0.6f;
  //              }
		//		cameraZoom = originalPosition.y * percentage;
		//		//difference = GameController.GetHolePinPosition();
		//	}
		//}
		//
		var followPosition = new Vector3(difference.x, cameraZoom, difference.z);

        var minStep = 10f * Time.deltaTime;
        var step = (Vector3.Distance(followPosition, transform.position) * 0.8f) * Time.deltaTime;
        if (step < minStep)
        {
            step = minStep;
        }

        transform.position = Vector3.MoveTowards(transform.position, followPosition, step);
        //transform.LookAt(GameSettings.CurrentCannonLocation);

    }
}
