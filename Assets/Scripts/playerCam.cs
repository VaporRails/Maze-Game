using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour{
    public float Xsens;
    public float Ysens;

    public Transform orientation;

    public float xRotation;
    public float yRotation;
    // Start is called before the first frame update
    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update() {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * Xsens;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * Ysens;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
