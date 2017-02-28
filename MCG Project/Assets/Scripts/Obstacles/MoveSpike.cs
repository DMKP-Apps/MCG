using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MoveSpike : MonoBehaviour {

    public Vector3 startRotation = new Vector3(0, 0, -75f);
    public Vector3 endRotation = new Vector3(0, 0, 0);
    public double waitAtEnd = 1000;
    public double waitAtStart = 1000;
    public double waitAtAwake = 1000;
    public float speedToEnd = 5f;
    public float speedToStart = 2f;

    private Vector3 _destination;
    private int _direction = 0;
    private DateTime _timeStamp = DateTime.Now;
    private double _waitTime = 0;
    private float _speed = 0;

    // Use this for initialization
    void Start () {
        transform.localRotation = Quaternion.Euler(startRotation);
        _destination = endRotation;
        _timeStamp = DateTime.Now;
        _waitTime = waitAtAwake;
        _speed = speedToEnd;

    }
	
	// Update is called once per frame
	void Update () {

        var quat = Quaternion.Euler(_destination);
        //v

        if (quat == transform.localRotation && _direction == 0) {
            _direction = 1;
            _destination = startRotation;
            _timeStamp = DateTime.Now;
            _waitTime = waitAtEnd;
            _speed = speedToStart;
        }
        else if (quat == transform.localRotation && _direction == 1)
        {
            _direction = 0;
            _destination = endRotation;
            _timeStamp = DateTime.Now;
            _waitTime = waitAtStart;
            _speed = speedToEnd;
        }

        if (DateTime.Now.Subtract(_timeStamp).TotalMilliseconds > _waitTime)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, quat, Time.deltaTime * _speed);
        }

    }
}
