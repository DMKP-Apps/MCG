using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonSkinPreview : MonoBehaviour {

    public Transform CameraPosition;
    public Transform MoveToPosition;
    public string description = string.Empty;

    public Material CannonBarrelMaterial;
    public Material CannonWheelMaterial;

    public GameObject Cannon;

    public GameObject WheelRight;
    public GameObject WheelLeft;

    public Renderer WheelRightRenderer;
    public Renderer WheelLeftRenderer;
    public Renderer BarrelRenderer;

    public Vector3 startRotation = new Vector3(0,-57.5f, 0);
    public Vector3 endRotation = new Vector3(0, -140f, 0);

    public float rotateInSpeed = 5f;
    public float rotateOutSpeed = 5f;

    public float moveInSpeed = 5f;
    public float moveOutSpeed = 5f;

    public bool isSelected = false;

    private Vector3 _rotation;
    private Vector3 _position;
    private Vector3 _originalPostion;
    private float _rotateSpeed = 0;
    private float _moveSpeed = 0;
    private float _wheelRotateMultiplier = 1;

    void Start()
    {
        if (CannonWheelMaterial != null && WheelRightRenderer != null && WheelLeftRenderer != null)
        {
            WheelRightRenderer.material = CannonWheelMaterial;
            WheelLeftRenderer.material = CannonWheelMaterial;
        }
        if (CannonBarrelMaterial != null && BarrelRenderer != null)
        {
            BarrelRenderer.material = CannonBarrelMaterial;
        }

        if (Cannon != null)
        {
            Cannon.transform.localRotation = Quaternion.Euler(startRotation);
            _originalPostion = Cannon.transform.localPosition;
        }



    }



    void Update()
    {

        if (isSelected)
        {
            _rotation = endRotation;
            _rotateSpeed = rotateInSpeed;
            _position = MoveToPosition.localPosition;
            _moveSpeed = moveInSpeed;
            _wheelRotateMultiplier = -1;
        }
        else
        {
            _rotation = startRotation;
            _rotateSpeed = rotateOutSpeed;
            _position = _originalPostion;
            _moveSpeed = moveOutSpeed;
            _wheelRotateMultiplier = 1;
        }

        var quat = Quaternion.Euler(_rotation);
        Cannon.transform.localRotation = Quaternion.Slerp(Cannon.transform.localRotation, quat, Time.deltaTime * _rotateSpeed);

        var followPosition = _position;

        var step = _moveSpeed * Time.deltaTime;
        
        Cannon.transform.localPosition = Vector3.MoveTowards(Cannon.transform.localPosition, followPosition, step);

        if (WheelLeft != null && WheelRight != null)
        {
            if (Vector3.Distance(Cannon.transform.localPosition, followPosition) != 0)
            {
                WheelLeft.transform.Rotate(new Vector3((_moveSpeed * 0.8f) * _wheelRotateMultiplier, 0, 0));
                WheelRight.transform.Rotate(new Vector3((_moveSpeed * 0.8f) * _wheelRotateMultiplier, 0, 0));
            }
            else if (quat != Cannon.transform.localRotation)
            {
                WheelLeft.transform.Rotate(new Vector3(((Time.deltaTime * _rotateSpeed) + 0.1f) * _wheelRotateMultiplier, 0, 0));
                WheelRight.transform.Rotate(new Vector3(((Time.deltaTime * _rotateSpeed) + 0.1f) * (_wheelRotateMultiplier * -1), 0, 0));
            }
        }


    }

}
