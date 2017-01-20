using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleControls : MonoBehaviour {

    // Use this for initialization
    private RectTransform rect = null;
    private Vector2 size;

    public float compareScaleTo = 1920f;
    private Vector2 originalPosOffset;
    private Vector3 originalScale;

    void Start () {
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null) {
            rect = canvas.GetComponent<RectTransform>();
        }
        var rtrans = this.GetComponent<RectTransform>();
        originalPosOffset = new Vector2(rtrans.anchoredPosition.x, rtrans.anchoredPosition.y);
        originalScale = transform.localScale;

    }
	
	// Update is called once per frame
	void Update () {
        var currentSize = new Vector2(rect.rect.width, rect.rect.height);
        if (size != currentSize)
        {
            size = currentSize;

            // determine the scale percentage
            float maxValue = size.x;
            if (size.y > maxValue) {
                maxValue = size.y;
            }

            var scalePercentage = maxValue / compareScaleTo;

            //Debug.Log(scalePercentage);
            transform.localScale = new Vector3(originalScale.x * scalePercentage, originalScale.y * scalePercentage, originalScale.z * scalePercentage);

            var rtrans = this.GetComponent<RectTransform>();
            rtrans.anchoredPosition = new Vector2(originalPosOffset.x * scalePercentage, originalPosOffset.y * scalePercentage);

        }
    }
}
