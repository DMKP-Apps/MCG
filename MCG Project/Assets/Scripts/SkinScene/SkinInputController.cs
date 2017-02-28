using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class SkinInputController : MonoBehaviour {

    public string gameScene = "Course";

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
    bool isMoving = false;
    bool onSelection = false;
    float touchBegin;
    void Update()
    {

        bool hasTouch = false;
        var touches = Input.touches.ToList();


        touches.Where(x => x.phase == TouchPhase.Began).Take(1).ToList().ForEach(touch => {
            touchBegin = touch.position.x;
            direction = 0;
            hasTouch = true;
        });

        touches.Where(x => x.phase == TouchPhase.Ended).Take(1).ToList().ForEach(touch => {
            direction = (touchBegin - touch.position.x) * -1;
            hasTouch = true;
            OnMoveCamera();
        });

        //touches.Where(x => x.phase == TouchPhase.Moved).Take(1).ToList().ForEach(touch =>
        //{
        //    if (touch.phase == TouchPhase.Moved)
        //    {
        //        var x = touch.deltaPosition.x;

        //        hasTouch = true;

        //        direction += x;
                
        //    }
        //});
        
        if (!hasTouch)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMoving = !isMoving;
            };

            
            if (isMoving)
            {
                hasTouch = true;

                var y = Input.GetAxis("Mouse Y") * 10;
                var x = Input.GetAxis("Mouse X") * 10;

                direction += x;
               

            }
        }

        if (!hasTouch)
        {
            OnMoveCamera();
        }



    }

    private void OnMoveCamera()
    {
        onSelection = Vector3.Distance(previewItems[index].CameraPosition.position, mainCamera.transform.position) == 0f;
        Debug.Log(string.Format("IsMoving: {0}, Direction: {1}, Distance: {2}", onSelection, direction, Vector3.Distance(previewItems[index].CameraPosition.position, mainCamera.transform.position)));

        var moveDistance = direction;
        if (direction < 0)
        {
            moveDistance *= -1;
        }

        if (onSelection && moveDistance > 5)
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
