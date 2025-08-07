using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayer : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Transform cam;
    private float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.Rotate(Vector3.up * mouseX);
        cam.localEulerAngles = new Vector3(pitch, 0, 0);

        // Movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move * speed * Time.deltaTime);
    }
}