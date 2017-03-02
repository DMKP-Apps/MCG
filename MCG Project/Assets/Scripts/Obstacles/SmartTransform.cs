using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


[System.Serializable]
public struct MoveVector
{
    public Vector3 position, rotation, scale;
    public float waitMillisec, positionSpeed, rotationSpeed, scaleSpeed;

    public MoveVector(Vector3 position, Vector3 rotation, Vector3 scale, float waitMillisec, float positionSpeed, float rotationSpeed, float scaleSpeed)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.waitMillisec = waitMillisec;
        this.positionSpeed = positionSpeed;
        this.rotationSpeed = rotationSpeed;
        this.scaleSpeed = scaleSpeed;
    }
}

public class SmartTransform : MonoBehaviour {

    public List<MoveVector> movement = new List<MoveVector>();


    private Vector3 _position_dest;
    private Vector3 _rotation_dest;
    private DateTime _timeStamp = DateTime.Now;
    private double _waitTime = 0;
    private float _rotationSpeed = 0;
    private float _positionSpeed = 0;
    private float _scaleSpeed = 0;
    private int _index = 0;

    // Use this for initialization
    void Start () {

        if (movement.Count < 2)
        {
            return;
        }
        transform.localRotation = Quaternion.Euler(movement[_index].rotation);
        _index++;
        _rotation_dest = movement[_index].rotation;
        _position_dest = movement[_index].position;
        _timeStamp = DateTime.Now;
        _waitTime = movement[_index].waitMillisec;
        _positionSpeed = movement[_index].positionSpeed;
        _rotationSpeed = movement[_index].rotationSpeed;
        _scaleSpeed = movement[_index].scaleSpeed;

    }

    // Update is called once per frame
    void Update () {

        if (movement.Count < 2)
        {
            return;
        }

        var quat = Quaternion.Euler(_rotation_dest);
        //v

        if (quat == transform.localRotation && _position_dest == transform.localPosition)
        {
            _index++;
            if (_index >= movement.Count)
            {
                _index = 0;
            }
            _rotation_dest = movement[_index].rotation;
            _position_dest = movement[_index].position;
            _timeStamp = DateTime.Now;
            _waitTime = movement[_index].waitMillisec;
            _positionSpeed = movement[_index].positionSpeed;
            _rotationSpeed = movement[_index].rotationSpeed;
            _scaleSpeed = movement[_index].scaleSpeed;

        }

        //if (quat == transform.localRotation && _direction == 0) {
        //    _direction = 1;
        //    _destination = startRotation;
        //    _timeStamp = DateTime.Now;
        //    _waitTime = waitAtEnd;
        //    _speed = speedToStart;
        //}
        //else if (quat == transform.localRotation && _direction == 1)
        //{
        //    _direction = 0;
        //    _destination = endRotation;
        //    _timeStamp = DateTime.Now;
        //    _waitTime = waitAtStart;
        //    _speed = speedToEnd;
        //}

        if (DateTime.Now.Subtract(_timeStamp).TotalMilliseconds > _waitTime)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _position_dest, Time.deltaTime * _positionSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, quat, Time.deltaTime * _rotationSpeed);
        }

    }
}
