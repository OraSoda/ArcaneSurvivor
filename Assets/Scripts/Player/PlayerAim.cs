
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float rotationSpeed = 20f;

    public Vector3 AimDirection { get; private set; } = Vector3.forward;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null || Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = targetCamera.ScreenPointToRay(mousePosition);

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 targetPoint = ray.GetPoint(distance);
        Vector3 direction = targetPoint - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        AimDirection = direction.normalized;

        Quaternion rotation = Quaternion.LookRotation(AimDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
