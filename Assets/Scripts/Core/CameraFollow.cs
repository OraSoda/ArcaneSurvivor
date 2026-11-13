
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField]
    private Vector3 offset =
        new Vector3(0f, 14f, -4f);

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(75f, 0f, 0f);
    }
}
