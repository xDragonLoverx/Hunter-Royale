using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float xSensitivity;
    [SerializeField] float ySensitivity;

    [SerializeField] Transform Orientation;

    float xRotation;
    float yRotation;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update()
    {
        float MouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * xSensitivity;
        float MouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * ySensitivity;

        yRotation += MouseX;
        xRotation -= MouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        Orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
