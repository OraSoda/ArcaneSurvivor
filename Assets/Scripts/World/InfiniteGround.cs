
using UnityEngine;

public class InfiniteGround : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followStep = 10f;

    private float originalY;

    private void Awake()
    {
        originalY = transform.position.y;

        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float step = Mathf.Max(1f, followStep);

        Vector3 position = transform.position;

        position.x = Mathf.Round(
            player.position.x / step) * step;

        position.z = Mathf.Round(
            player.position.z / step) * step;

        position.y = originalY;

        transform.position = position;
    }
}
