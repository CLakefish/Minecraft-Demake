using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Orientation")]
    [SerializeField] public Transform playerObj;

    [Header("Mouse Stuff")]
    [SerializeField] public bool canLook = true;
    [SerializeField]
    public Vector2 Sensitivity
    {
        get { return sensitivity; }
        set { sensitivity = value; }
    }
    [SerializeField] Vector2 sensitivity;

    [SerializeField]
    public float FOV
    {
        get { return fieldOfView; }
        set { fieldOfView = value; }
    }
    [SerializeField] float fieldOfView;

    float shakePos;
    public bool invertY;
    public bool invertX;

    Vector2 mouseRotation;
    internal Vector2 mousePos;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // FOV Change based on FOV 
        Camera.main.fieldOfView = fieldOfView;
    }


    // Update is called once per frame
    void Update()
    {
        if (!canLook) return;

        // https://github.com/deadlykam/TutorialFPSRotation/tree/main/TutorialFPSRotation/Assets/TutorialFPSRotation/Scripts

        Camera.main.fieldOfView = Input.GetKey(KeyCode.LeftShift) ? fieldOfView * 1.15f : fieldOfView;

        mousePos = new Vector2(Input.GetAxis("Mouse X") * sensitivity.x, Input.GetAxis("Mouse Y") * sensitivity.y);

        mouseRotation.x -= mousePos.y;
        mouseRotation.y += mousePos.x;
        mouseRotation.x = Mathf.Clamp(mouseRotation.x, -89f, 89f);

        transform.eulerAngles = new Vector3(0f, mouseRotation.y, 0f);
        playerObj.localEulerAngles = new Vector3(mouseRotation.x, 0f, 0f);
    }
}
