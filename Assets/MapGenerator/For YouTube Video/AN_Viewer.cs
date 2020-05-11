using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_Viewer : MonoBehaviour
{
    public float Walk = 1f, Run = 5f;
    public int Sensitivity = 60;
    public bool HideCursor = false;

    Vector3 Move;
    Transform Cam;
    float xmouse, ymouse, yRotation, Speed;

    void Start()
    {
        Cam = Camera.main.GetComponent<Transform>();

        if (HideCursor)
        {
            Cursor.lockState = CursorLockMode.Locked; // freeze cursor on screen centre
            Cursor.visible = false; // invisible cursor
        }
    }

    void Update() // camera rotation
    {
        xmouse = Input.GetAxis("Mouse X") * Time.deltaTime * Sensitivity;
        ymouse = Input.GetAxis("Mouse Y") * Time.deltaTime * Sensitivity;

        transform.Rotate(Vector3.up * xmouse);
        yRotation -= ymouse;
        yRotation = Mathf.Clamp(yRotation, -85f, 60f);
        Cam.localRotation = Quaternion.Euler(yRotation, 0, 0);

        if (Input.GetKey(KeyCode.LeftShift)) Speed = Run;
        else Speed = Walk;

        Move = Cam.forward * Speed * Input.GetAxis("Vertical") +
            Cam.right * Speed * Input.GetAxis("Horizontal");
        Move *= Time.deltaTime;
        transform.position += Move;
    }
}
