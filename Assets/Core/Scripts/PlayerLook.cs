using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{

    [SerializeField] Transform cameraRoot;
    [SerializeField] Transform orientation;

    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    float sensitivityX = 10f;
    float sensitivityY = 10f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public float xRot;
    public float yRot;

    // Update is called once per frame
    void Update()
    {
        xRot -= playerInput.mouseY * sensitivityX * 0.1f;
        yRot += playerInput.mouseX * sensitivityY * 0.1f;

        xRot = Mathf.Clamp(xRot, -90f, 90f);

        cameraRoot.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        orientation.transform.rotation = Quaternion.Euler(0, yRot, 0);
    }
}
