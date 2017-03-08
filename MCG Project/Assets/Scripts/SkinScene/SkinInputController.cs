using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class SkinInputController : MonoBehaviour {

    public string gameScene = "Course";

    private const string disableTouchTag = "DisableTouch";

    public GameObject previewCannonPrefab;

    public List<Material> barrelMaterials;
    public List<Material> wheelMaterials;

    public Transform mainCamera;
    public TextMesh text;

    private CannonSkinPreview[] previewItems;
    private int index = 0;

    private float direction = 0;

    void Start()
    {
        BuildPreviews();

        previewItems = GameObject.FindObjectsOfType<CannonSkinPreview>().OrderBy(x => x.transform.position.x).ToArray();

        index = GameSettings.SkinIndex;
        if (index < 0 || index >= previewItems.Length)
        {
            index = 0;
        }

        mainCamera.transform.position = previewItems[index].CameraPosition.position;

        var touchUtility = GameObject.FindObjectOfType<TouchUtility>();
        if (touchUtility != null)
        {
            touchUtility.Subscribe(this, OnTouchEnded, TouchEventType.Ended);
        }


    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            var touchUtility = GameObject.FindObjectOfType<TouchUtility>();
            if (!touchUtility.HasSubscription(this))
            {
                touchUtility.Subscribe(this, OnTouchEnded, TouchEventType.Ended);
            }
        }

    }

    void OnDestroy()
    {
        var touchUtility = GameObject.FindObjectOfType<TouchUtility>();
        if (touchUtility != null)
        {
            touchUtility.Unsubscribe(this);
        }
    }

    void BuildPreviews()
    {
        var positionOffset = new Vector3(-24f, 0, 0);
        float offsetIncrement = 6f;
        int wheelIndex = 0;
        barrelMaterials.ForEach(material => {

            var previewItem = (GameObject)Instantiate(previewCannonPrefab, positionOffset, Quaternion.Euler(new Vector3(0, 0, 0)));
            var skinPreview = previewItem.GetComponent<CannonSkinPreview>();
            if (skinPreview != null)
            {
                var wheelMarterial = wheelMaterials.Count > wheelIndex ? wheelMaterials[wheelIndex] : null;
                skinPreview.CannonBarrelMaterial = material;
                skinPreview.CannonWheelMaterial = wheelMarterial;
                skinPreview.description = material.name;
            }

            positionOffset.x += offsetIncrement;
            wheelIndex++;
        });

        
    }

    
    // Update is called once per frame
    bool onSelection = false;
   

    void OnTouchEnded(IEnumerable<TouchDetail> touches)
    {
        direction = 0;
        touches.ToList()
            .ForEach(touch => {

                //Debug.Log(string.Format("Touched: {0}", string.Join(",", touch.AllCollidingObject.Select(x => x.name).ToArray())));

                if (!(touch.endCollidingObjects.Any(x => x.tag == disableTouchTag) && touch.beginCollidingObject.Any(x => x.tag == disableTouchTag)))
                {
                    direction = (touch.startPosition.Value.x - touch.endPosition.Value.x) * -1;
                }
                
            });
    }

    private bool isContinueKey = false;

    void Update()
    {
        OnMoveCamera();


        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift)) && !isContinueKey)
        {
            isContinueKey = true;
            OnContinueClick();
        }
        else if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftShift)))
        {
            isContinueKey = false;
        }


    }

       

    private void OnMoveCamera()
    {
        onSelection = Vector3.Distance(previewItems[index].CameraPosition.position, mainCamera.transform.position) == 0f;

        var moveDistance = direction;
        if (direction < 0)
        {
            moveDistance *= -1;
        }

        if (onSelection && moveDistance > 0)
        {
            if (direction < 0 && index < previewItems.Length - 1)
            {
                previewItems[index].isSelected = false;
                index++;
            }
            else if (direction > 0 && index > 0)
            {
                previewItems[index].isSelected = false;
                index--;
            }

            direction = 0;
        }
        else if(!onSelection)
        {
            direction = 0;
        }


        var followPosition = previewItems[index].CameraPosition.position;

        var minStep = 8f * Time.deltaTime;
        var step = (Vector3.Distance(followPosition, mainCamera.transform.position) * 0.75f) * Time.deltaTime;
        if (step < minStep)
        {
            step = minStep;
        }

        mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, followPosition, step);
        text.text = previewItems[index].description;

        previewItems[index].isSelected = true;

    }

    public void OnContinueClick()
    {
        GameSettings.SkinIndex = index;
        GameSettings.CannonBarrelMaterial = previewItems[index].CannonBarrelMaterial;
        GameSettings.CannonWheelMaterial = previewItems[index].CannonWheelMaterial;

        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

}
