using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public CollisionManager collisionManager;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Camera")]
    public Transform playerCamera;

    private float xRotation = 0f;
    public Rigidbody bullet;
    public float speedLaunch = 5f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();

        if (Input.GetKeyDown(KeyCode.Space))
            Launch();
    }

    void HandleMovement()
    {
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        Vector3 move = (transform.forward * moveZ + transform.right * moveX).normalized;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void Launch()
    {
        bullet.linearVelocity = playerCamera.forward * speedLaunch;
        bullet.GetComponent<Bullet>().player = this;
        GameObject bulletObj = Instantiate(bullet.transform.gameObject, transform.position + (playerCamera.forward * 5), Quaternion.identity);

        collisionManager.RegisterObject(bulletObj.GetComponent<Rigidbody>());
    }
}