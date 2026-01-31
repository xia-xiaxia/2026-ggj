using UnityEngine;
using UnityEngine.InputSystem;
public class cameraControl : MonoBehaviour
{

    [Header("Move")]
    public float moveSpeed = 20f;
    public float moveSmooth = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minHeight = 100f;
    public float maxHeight = 600f;

    [Header("Rotate")]
    public float rotateSpeed = 180f; // 鼠标拖动旋转速度

    [Header("Bounds")]
    public Vector2 minXZ = new Vector2(-200f, -200f);
    public Vector2 maxXZ = new Vector2(200f, 200f);

    [Header("Isometric Angle")]
    public float pitch = 45f;

    private Vector3 targetPos;
    private float targetYaw;

    void Start()
    {
        targetPos = transform.position;
        targetYaw = transform.eulerAngles.y;
        ApplyPitch();
    }

    void LateUpdate()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // 移动（WASD）
        Vector2 move = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) move.y += 1;
        if (Keyboard.current.sKey.isPressed) move.y -= 1;
        if (Keyboard.current.aKey.isPressed) move.x -= 1;
        if (Keyboard.current.dKey.isPressed) move.x += 1;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 moveDir = (forward * move.y + right * move.x).normalized;

        targetPos += moveDir * moveSpeed * Time.deltaTime;

        // 旋转（按住右键拖动）
        if (Mouse.current.rightButton.isPressed)
        {
            float deltaX = Mouse.current.delta.ReadValue().x;
            targetYaw += deltaX * rotateSpeed * Time.deltaTime;
        }

        // 缩放（滚轮）
        float scroll = Mouse.current.scroll.ReadValue().y;
        targetPos += Vector3.up * (-scroll * zoomSpeed * 0.01f);

        // 限制高度
        targetPos.y = Mathf.Clamp(targetPos.y, minHeight, maxHeight);

        // 限制边界
        targetPos.x = Mathf.Clamp(targetPos.x, minXZ.x, maxXZ.x);
        targetPos.z = Mathf.Clamp(targetPos.z, minXZ.y, maxXZ.y);

        // 平滑移动
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmooth * Time.deltaTime);

        // 旋转（保持斜角）
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(pitch, targetYaw, 0f),
            moveSmooth * Time.deltaTime
        );
    }

    private void ApplyPitch()
    {
        transform.rotation = Quaternion.Euler(pitch, targetYaw, 0f);
    }
}
