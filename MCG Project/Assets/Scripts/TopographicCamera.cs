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

    // Use this for initialization
    void Start () {
        tcamera = this.GetComponent<Camera>();
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            rect = canvas.GetComponent<RectTransform>();
        }

        //originalRect = tcamera.rect;

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
                newRect.height = 0.3f;
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

        var step = speed * Time.deltaTime;
        var difference = (GameSettings.EstimatedShotLocation + GameSettings.CurrentCannonLocation) / 2;
        var followPosition = new Vector3(difference.x, transform.position.y, difference.z);
        transform.position = Vector3.MoveTowards(transform.position, followPosition, step);

    }
}
