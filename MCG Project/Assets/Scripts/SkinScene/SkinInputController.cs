using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class SkinInputController : MonoBehaviour {

    public string gameScene = "Course";

    public Transform mainCamera;
    public TextMesh text;

    private CannonSkinPreview[] previewItems;
    private int index = 0;

    private float direction = 0;

    void Start()
    {
        previewItems = GameObject.FindObjectsOfType<CannonSkinPreview>().OrderBy(x => x.transform.position.x).ToArray();

        mainCamera.transform.position = previewItems[index].CameraPosition.position;

    }

    
    // Update is called once per frame
    bool isMoving = false;
    void Update()
    {

        
        bool hasTouch = false;

        var touches = Input.touches.ToList();
        
        
        touches.Where(x => x.phase == TouchPhase.Moved).Take(1).ToList().ForEach(touch =>
        {


            if (touch.phase == TouchPhase.Moved)
            {
                var y = touch.deltaPosition.y;
                var x = touch.deltaPosition.x;

                hasTouch = true;

                direction += x;
                
            }
        });
        
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
            if (direction > 0 && index < previewItems.Length - 1)
            {
                index++;
            }
            else if (direction < 0 && index > 0)
            {
                index--;
            }

            direction = 0;
        }


        var followPosition = previewItems[index].CameraPosition.position;
        
        var minStep = 10f * Time.deltaTime;
        var step = (Vector3.Distance(followPosition, mainCamera.transform.position) * 0.75f) * Time.deltaTime;
        if (step < minStep)
        {
            step = minStep;
        }

        mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, followPosition, step);
        text.text = previewItems[index].description;



    }

    public void OnContinueClick()
    {
        GameSettings.CannonBarrelMaterial = previewItems[index].CannonBarrelMaterial;
        GameSettings.CannonWheelMaterial = previewItems[index].CannonWheelMaterial;

        SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

}
