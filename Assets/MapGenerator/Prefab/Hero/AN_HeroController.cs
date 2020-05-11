using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_HeroController : MonoBehaviour
{
    [Tooltip("Character settings (rigid body)")]
    public float MoveSpeed = 30f, JumpForce = 200f, Sensitivity = 70f;
    public bool HideCursor = true, Immoveble = false;
    [Space]
    public bool AndroidControl;

    bool jumpFlag = true; // to jump from surface only
    float xmouse, ymouse;

    Rigidbody rb;
    Vector3 moveVector, startVector;

    Transform Cam;
    float yRotation;

    void Start()
    {
        startVector = gameObject.transform.position;

        rb = GetComponent<Rigidbody>();
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

        if (Input.GetButtonDown("Jump") && jumpFlag) rb.AddForce(transform.up * JumpForce);
    }

    void FixedUpdate() // body moving
    {
        if (Immoveble)
        {
            gameObject.transform.position = startVector;
            rb.velocity = Vector3.zero;
        }

        moveVector = transform.forward * MoveSpeed * Input.GetAxis("Vertical") +
            transform.right * MoveSpeed * Input.GetAxis("Horizontal") +
            transform.up * rb.velocity.y;
        rb.velocity = moveVector;
    }
    
    private void OnTriggerStay(Collider other)
    {
        jumpFlag = true; // hero can jump
    }

    private void OnTriggerExit(Collider other)
    {
        jumpFlag = false;
    }

    public void JumpForAndroid()
    {
        if (jumpFlag) rb.AddForce(transform.up * JumpForce);
    }

    public void Moveble()
    {
        Immoveble = false;
        Cursor.lockState = CursorLockMode.Locked; // freeze cursor on screen centre
        Cursor.visible = false; // invisible cursor
    }
}
