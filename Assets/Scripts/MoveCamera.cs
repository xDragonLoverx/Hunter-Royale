using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform CameraPos;
    private void Update()
    {
        transform.position = CameraPos.position;
    }
}
