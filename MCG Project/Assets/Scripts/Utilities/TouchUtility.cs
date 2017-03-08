using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class TouchDetail
{
    public int touchId;
    public Vector2? startPosition;
    public Vector2? endPosition;
    public Vector2 deltaPosition = new Vector2();
    //public List<Vector2> deltaPositions = new List<Vector2>();
    public List<GameObject> beginCollidingObject = new List<GameObject>();
    public List<GameObject> moveCollidingObjects = new List<GameObject>();
    public List<GameObject> endCollidingObjects = new List<GameObject>();

    private List<Vector2> _deltaPositions = new List<Vector2>();
    private List<Vector2> _positions = new List<Vector2>();

    public List<Vector2> AllDeltaPositions()
    {
        return _deltaPositions != null ? _deltaPositions : new List<Vector2>();
    }

    public int NumberOfPosition { get { return _positions != null ? _positions.Count : 0; } }

    public void AddPosition(Vector2 point)
    {
        if (_positions == null)
        {
            _positions = new List<Vector2>();
        }

        if (_positions.Count == 0 && startPosition.HasValue)
        {
            _positions.Add(startPosition.Value);
        }

        _positions.Add(point);

        if (_deltaPositions == null)
        {
            _deltaPositions = new List<Vector2>();
        }

        if (_positions.Count > 1)
        {
            var previousPositions = _positions[_positions.Count - 2];
            var currentPositions = _positions[_positions.Count - 1];

            var maxScreenSize = Screen.width;
            if (Screen.height > maxScreenSize) {
                maxScreenSize = Screen.height;
            }

            var delta = currentPositions - previousPositions;
            delta.x = delta.x / maxScreenSize * 100;
            delta.y = delta.y / maxScreenSize * 100;
            deltaPosition = delta;
            _deltaPositions.Add(delta);
        }
        
    }

    public List<GameObject> AllCollidingObject
    {
        get
        {
            return beginCollidingObject.Union(moveCollidingObjects).Union(endCollidingObjects)
                .GroupBy(x => x.GetInstanceID())
                .Select(x => x.FirstOrDefault())
                .Where(x => x != null)
                .ToList();
        }
    }

    public Vector2 GetInputDistance()
    {
        if (!startPosition.HasValue)
        {
            return new Vector2();
        }

        var start = startPosition.Value;
        var end = _positions.LastOrDefault();

        if (endPosition.HasValue)
        {
            end = endPosition.Value;
        }

        var maxScreenSize = Screen.width;
        if (Screen.height > maxScreenSize)
        {
            maxScreenSize = Screen.height;
        }

        var difference = end - start;
        difference.x = difference.x / maxScreenSize * 100;
        difference.y = difference.y / maxScreenSize * 100;

        return difference;
    }

    public List<TouchEventType> CollisionDetectedEvent(GameObject gameObject)
    {
        List<TouchEventType> types = new List<TouchEventType>();
        if (beginCollidingObject.FindIndex(x => x.GetInstanceID() == gameObject.GetInstanceID()) > -1)
        {
            types.Add(TouchEventType.Began);
        }
        if (moveCollidingObjects.FindIndex(x => x.GetInstanceID() == gameObject.GetInstanceID()) > -1)
        {
            types.Add(TouchEventType.Moved);
        }
        if (endCollidingObjects.FindIndex(x => x.GetInstanceID() == gameObject.GetInstanceID()) > -1)
        {
            types.Add(TouchEventType.Ended);
        }
        return types;
    }

    public TouchEventType EventType
    {
        get
        {
            return startPosition != null && endPosition == null && _deltaPositions.Count == 0 ? TouchEventType.Began :
                startPosition != null && endPosition == null && _deltaPositions.Count > 0 ? TouchEventType.Moved :
                startPosition != null && endPosition != null ? TouchEventType.Ended : TouchEventType.Unknown;
        }
    }

    // KAP - Save code for later
    /*
     * 
     * // Store both touches.
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
     * */
}

public enum TouchEventType
{
    Began,
    Moved,
    Ended,
    Unknown
}

public class TouchUtility : MonoBehaviour
{
    public class TouchEventInfo
    {
        public Action<IEnumerable<TouchDetail>> onAction;
        public MonoBehaviour parent;
    }

    private List<TouchDetail> touchDetails = new List<TouchDetail>();
    private Dictionary<TouchEventType, List<TouchEventInfo>> subscribers = new Dictionary<TouchEventType, List<TouchEventInfo>>();

    public bool detectMouseInput = false;
    public bool detectKeyboardInput = true;
    private bool trackMouseInput = false;

    private const int keyboardId = 99;
    private RectTransform rectTranform = null;

    public float keyboardSpeed = 5;
    public float mouseMoveSpeed = 12f;


    void Start()
    {
        init(TouchEventType.Began, TouchEventType.Moved, TouchEventType.Ended);

        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            rectTranform = canvas.GetComponent<RectTransform>();
            GameSettings.DisplaySize = rectTranform.rect.size;
        }

        Cursor.visible = false;
    }

    void init(params TouchEventType[] eventTypes)
    {
        if (subscribers == null)
        {
            subscribers = new Dictionary<TouchEventType, List<TouchUtility.TouchEventInfo>>();
        }

        if (eventTypes == null)
        {
            return;
        }

        eventTypes.ToList().ForEach(type => {
            if (!subscribers.ContainsKey(type))
            {
                subscribers.Add(type, new List<TouchUtility.TouchEventInfo>());
            }
            else if (subscribers[type] == null)
            {
                subscribers[type] = new List<TouchUtility.TouchEventInfo>();
            }
        });
        
    }

    public void Subscribe(MonoBehaviour parent, Action<IEnumerable<TouchDetail>> onUpdate, params TouchEventType[] eventTypes)
    {
        if (eventTypes == null || onUpdate == null)
        {   // nothing to do... just exit.
            return;
        }

        // init just incase the sequence of events happens before the items start up.
        init(eventTypes);

        eventTypes.ToList().ForEach(type => {
            subscribers[type].Add(new TouchEventInfo() {
                parent = parent,
                onAction = onUpdate
            });
        });
    }

    public void Unsubscribe(MonoBehaviour parent)
    {
        if (parent == null )
        {   // nothing to do... just exit.
            return;
        }

        subscribers.Select(x => x.Key).ToList().ForEach(type => {
            subscribers[type] = subscribers[type].Where(x => x.parent != parent).ToList();
        });
    }

    public bool HasSubscription(MonoBehaviour parent)
    {
        if (parent == null)
        {   // nothing to do... just exit.
            return false;
        }

        return subscribers.Any(x => subscribers[x.Key].Any(y => y.parent == parent));
    }

    private Vector2 keyBoardMove;
    private Vector2 mouseMove;
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp(KeyCode.M))
        {
            detectMouseInput = !detectMouseInput;
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            detectKeyboardInput = !detectKeyboardInput;
        }

        if (GameSettings.DisplaySize == null || GameSettings.DisplaySize.Value != rectTranform.rect.size)
        {
            GameSettings.DisplaySize = rectTranform.rect.size;
        }

        if (!OnTouch(Input.touches.ToList()) && (detectMouseInput || detectKeyboardInput))
        {
            var x = detectMouseInput ? Input.GetAxis("Mouse X") * mouseMoveSpeed : 0;
            var y = detectMouseInput ? Input.GetAxis("Mouse Y") * mouseMoveSpeed : 0;

            mouseMove += detectMouseInput ? new Vector2(x, y) : new Vector2(0, 0);

            var point = mouseMove; //detectMouseInput ? new Vector2(Input.mousePosition.x, Input.mousePosition.y) : new Vector2(0,0);
           
            if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) && detectKeyboardInput)
            {
                var speed = keyboardSpeed;
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    keyBoardMove.x += speed * -1;
                    x = speed * -1;
                }

                if (Input.GetKey(KeyCode.RightArrow))
                {
                    keyBoardMove.x += speed;
                    x = speed;
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    keyBoardMove.y += speed;
                    y = speed;
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    keyBoardMove.y += speed * -1;
                    y = speed * -1;
                }

                point = keyBoardMove;

            }
            else
            {
                keyBoardMove = new Vector2(0, 0);
            }

            if (!(x == 0 && y == 0))
            {
                trackMouseInput = true;
                OnMouseMove(point, new Vector2(x, y));
            }
            else if(trackMouseInput)
            {
                trackMouseInput = false;
                int touchIndex = touchDetails.FindIndex(k => k.touchId == keyboardId);
                if (touchIndex > -1)
                {
                    //var endpoint = touchDetails[touchIndex].startPosition.GetValueOrDefault() + touchDetails[touchIndex].deltaPosition;
                    //Debug.Log(string.Format("OnMouseMove([{0}], [{1}])", point, touchDetails[touchIndex].deltaPosition));
                    OnMouseMove(point, touchDetails[touchIndex].deltaPosition);
                }

                mouseMove = new Vector2(0, 0);
                keyBoardMove = new Vector2(0, 0);


            }
        }
    }

    
    bool OnTouch(List<Touch> touches) {
        		
		if (touches.Count == 0) {
            return false;
		}

        touchDetails.AddRange(touches.Where(touch => touch.phase == TouchPhase.Began).Select(touch => new TouchDetail
        {
            touchId = touch.fingerId,
            startPosition = touch.position,
            beginCollidingObject = checkForCollision(touch)
        }));

        touches.Where(touch => touch.phase == TouchPhase.Moved)
            .ToList()
            .ForEach(touch =>
            {
                var index = touchDetails.FindIndex(x => x.touchId == touch.fingerId);
                if (index > -1)
                {
                    touchDetails[index].AddPosition(touch.position);
                    touchDetails[index].moveCollidingObjects = checkForCollision(touch);
                }
            });

        List<int> completedTouchIndexes = new List<int>();
        touches.Where(touch => touch.phase == TouchPhase.Ended)
            .ToList()
            .ForEach(touch =>
            {
                var index = touchDetails.FindIndex(x => x.touchId == touch.fingerId);
                if (index > -1)
                {
                    touchDetails[index].endPosition = touch.position;
                    touchDetails[index].endCollidingObjects = checkForCollision(touch);
                    completedTouchIndexes.Add(index);
                }
            });

        touches.Where(touch => touch.phase == TouchPhase.Canceled)
            .ToList()
            .ForEach(touch =>
            {
                var index = touchDetails.FindIndex(x => x.touchId == touch.fingerId);
                if (index > -1)
                {
                    completedTouchIndexes.Add(index);
                }
            });

        updateSubscribers(completedTouchIndexes);

        return true;
        
    }

    void OnMouseMove(Vector2 point, Vector2 movement)
    {
        if (movement == new Vector2(0, 0))
        {
            return;
        }


        int touchIndex = touchDetails.FindIndex(x => x.touchId == keyboardId);
        List<int> completedTouchIndexes = new List<int>();

        // determine if this is a start or movement
        if (trackMouseInput && touchIndex == -1)
        { // Begin
            var touchDetail = new TouchDetail() {
                touchId = keyboardId,
                startPosition = point,
                beginCollidingObject = checkForCollision(point)
            };
            touchDetails.Add(touchDetail);
        }
        else if (trackMouseInput && touchIndex > -1)
        { // Move
            touchDetails[touchIndex].AddPosition(point);
            touchDetails[touchIndex].moveCollidingObjects = checkForCollision(point);
        }
        else if (!trackMouseInput && touchIndex > -1)
        { // End
            touchDetails[touchIndex].endPosition = point;
            touchDetails[touchIndex].endCollidingObjects = checkForCollision(point);

            completedTouchIndexes.Add(touchIndex);
        }

        updateSubscribers(completedTouchIndexes);

    }

    void updateSubscribers(List<int> completedTouchIndexes)
    {
        // ensure that this list is unique;
        completedTouchIndexes = completedTouchIndexes.Distinct().ToList();

        var tbegan = touchDetails.Where(x => x.EventType == TouchEventType.Began).ToList();
        if (tbegan.Count > 0)
        {
            // update the subscribing objects.
            subscribers[TouchEventType.Began].ForEach(action =>
            {
                action.onAction(tbegan);
            });
        }

        var mbegan = touchDetails.Where(x => x.EventType == TouchEventType.Moved).ToList();
        if (mbegan.Count > 0)
        {
            // update the subscribing objects.
            subscribers[TouchEventType.Moved].ForEach(action =>
            {
                action.onAction(mbegan);
            });
        }

        var ebegan = touchDetails.Where(x => x.EventType == TouchEventType.Ended).ToList();
        if (ebegan.Count > 0)
        {
            // update the subscribing objects.
            subscribers[TouchEventType.Ended].ForEach(action =>
            {
                action.onAction(ebegan);
            });
        }

        // remove the completed items
        completedTouchIndexes.ForEach(x => touchDetails.RemoveAt(x));
    }

    List<GameObject> checkForCollision(Touch touch)
    {
        return checkForCollision(touch.position);
    }
    List<GameObject> checkForCollision(Vector2 point)
    {
        List<GameObject> collidingObjects = new List<GameObject>();

        var touchPosWorld = Camera.main.ScreenToWorldPoint(point);

        Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);

        Ray ray = Camera.main.ScreenPointToRay(point);
        RaycastHit2D hit2D = Physics2D.Raycast(touchPosWorld2D, Camera.main.transform.forward);
        if (hit2D.collider != null && hit2D.collider.transform != null)
        {
            collidingObjects.Add(hit2D.collider.transform.gameObject);
        }

        RaycastHit hit3D;
        if (Physics.Raycast(ray, out hit3D, 1000.0f))
        {
            collidingObjects.Add(hit3D.collider.transform.gameObject);
        }

        // only get the unique items.
        collidingObjects = collidingObjects.GroupBy(x => x.GetInstanceID()).Select(x => x.FirstOrDefault()).Where(x => x != null).ToList();

        return collidingObjects;
    }

}
