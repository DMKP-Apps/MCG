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
    public List<Vector2> deltaPositions = new List<Vector2>();
    public List<GameObject> beginCollidingObject = new List<GameObject>();
    public List<GameObject> moveCollidingObjects = new List<GameObject>();
    public List<GameObject> endCollidingObjects = new List<GameObject>();

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
            return startPosition != null && endPosition == null && deltaPositions.Count == 0 ? TouchEventType.Began :
                startPosition != null && endPosition == null && deltaPositions.Count > 0 ? TouchEventType.Moved :
                startPosition != null && endPosition != null ? TouchEventType.Ended : TouchEventType.Unknown;
        }
    }
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

    private List<TouchDetail> touchDetails = new List<TouchDetail>();
    private Dictionary<TouchEventType, List<Action<IEnumerable<TouchDetail>>>> subscribers = new Dictionary<TouchEventType, List<Action<IEnumerable<TouchDetail>>>>();

    public bool detectMouseInput = false;
    private bool trackMouseInput = false;

    private const int keyboardId = 99;
    private RectTransform rectTranform = null;

    void Start()
    {
        init(TouchEventType.Began, TouchEventType.Moved, TouchEventType.Ended);

        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            rectTranform = canvas.GetComponent<RectTransform>();
            GameSettings.DisplaySize = rectTranform.rect.size;
        }
    }

    void init(params TouchEventType[] eventTypes)
    {
        if (subscribers == null)
        {
            subscribers = new Dictionary<TouchEventType, List<Action<IEnumerable<TouchDetail>>>>();
        }

        if (eventTypes == null)
        {
            return;
        }

        eventTypes.ToList().ForEach(type => {
            if (!subscribers.ContainsKey(type))
            {
                subscribers.Add(type, new List<Action<IEnumerable<TouchDetail>>>());
            }
            else if (subscribers[type] == null)
            {
                subscribers[type] = new List<Action<IEnumerable<TouchDetail>>>();
            }
        });
        
    }

    public void Subscribe(Action<IEnumerable<TouchDetail>> onUpdate, params TouchEventType[] eventTypes)
    {
        if (eventTypes == null || onUpdate == null)
        {   // nothing to do... just exit.
            return;
        }

        // init just incase the sequence of events happens before the items start up.
        init(eventTypes);

        eventTypes.ToList().ForEach(type => {
            subscribers[type].Add(onUpdate);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (GameSettings.DisplaySize == null || GameSettings.DisplaySize.Value != rectTranform.rect.size)
        {
            GameSettings.DisplaySize = rectTranform.rect.size;
        }

        if (!OnTouch(Input.touches.ToList()) && detectMouseInput)
        {
            var x = Input.GetAxis("Mouse X");
            var y = Input.GetAxis("Mouse Y");

            if (Input.GetMouseButtonDown(0))
            {
                trackMouseInput = !trackMouseInput;
            };

            if (!(x == 0 && y == 0))
            {
                OnMouseMove(new Vector2(Input.mousePosition.x, Input.mousePosition.y), new Vector2(x, y));
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
                    touchDetails[index].deltaPositions.Add(touch.deltaPosition);
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
            touchDetails[touchIndex].deltaPositions.Add(movement);
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

        // update the subscribing objects.
        subscribers[TouchEventType.Began].ForEach(action =>
        {
            action(touchDetails.Where(x => x.EventType == TouchEventType.Began));
        });
        subscribers[TouchEventType.Moved].ForEach(action =>
        {
            action(touchDetails.Where(x => x.EventType == TouchEventType.Moved));
        });
        subscribers[TouchEventType.Ended].ForEach(action =>
        {
            action(touchDetails.Where(x => x.EventType == TouchEventType.Ended));
        });

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
