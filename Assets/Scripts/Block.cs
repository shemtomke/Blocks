using UnityEngine;

public class Block : MonoBehaviour
{
    public GameObject closestBlockUp;
    public GameObject closestBlockDown;
    public GameObject closestBlockLeft;
    public GameObject closestBlockRight;

    [Space(20)]

    public GameObject closesBlockNorthWest;
    public GameObject closesBlockNorthEast;
    public GameObject closesBlockSouthEast;
    public GameObject closesBlockSouthWest;

    public float rayDistance = 2f;
    public Color rayColor = Color.red;

    private Vector3 dragStartPos;
    private Vector3 dragEndPos;

    private float dragThreshold = 10f; // Adjust this value to set the drag sensitivity

    private bool isDragging = false;
    public bool isWhite, isBlack;

    BlockManager blockManager;

    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();
        //rayDistance = transform.localScale.x * 0.5f;
        isDragging = false;
    }
    private void OnMouseDown()
    {
        // Record the start position for both clicks and potential drags
        dragStartPos = Input.mousePosition;
    }
    private void OnMouseUp()
    {
        if (!isDragging)
        {
            // If it's not a drag (within the threshold), consider it a click
            Debug.Log("Split the Cube!");
            blockManager.Split(this.gameObject);
        }

        isDragging = false; // Reset the dragging flag
    }
    private void OnMouseDrag()
    {
        // Check for dragging only if the mouse is moving
        if (Vector3.Distance(Input.mousePosition, dragStartPos) > dragThreshold)
        {
            isDragging = true;

            dragEndPos = Input.mousePosition;

            // Calculate the drag delta
            Vector3 dragDelta = dragEndPos - dragStartPos;

            // Check for horizontal drag (left or right)
            if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y))
            {
                if (dragDelta.x < 0)
                {
                    // Drag left
                    blockManager.Swipe(this.gameObject, closestBlockLeft);
                }
                else
                {
                    // Drag right
                    blockManager.Swipe(this.gameObject, closestBlockRight);
                }
            }
            // Check for vertical drag (up or down)
            else if (Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x))
            {
                if (dragDelta.y < 0)
                {
                    // Drag down
                    blockManager.Swipe(this.gameObject, closestBlockDown);
                }
                else
                {
                    // Drag up
                    blockManager.Swipe(this.gameObject, closestBlockUp);
                }
            }
        }
    }
    private void Update()
    {
        // Cast rays in four directions from the center of the object
        CastRay(Vector3.up, ref closestBlockUp);
        CastRay(Vector3.down, ref closestBlockDown);
        CastRay(Vector3.left, ref closestBlockLeft);
        CastRay(Vector3.right, ref closestBlockRight);

        Vector3 northeastDiagonal = new Vector3(1f, 1f, 0f).normalized;
        Vector3 northwestDiagonal = new Vector3(-1f, 1f, 0f).normalized;
        Vector3 southeastDiagonal = new Vector3(1f, -1f, 0f).normalized;
        Vector3 southwestDiagonal = new Vector3(-1f, -1f, 0f).normalized;

        CastRay(northeastDiagonal, ref closesBlockNorthEast);
        CastRay(northwestDiagonal, ref closesBlockNorthWest);
        CastRay(southeastDiagonal, ref closesBlockSouthEast);
        CastRay(southwestDiagonal, ref closesBlockSouthWest);
    }
    public void CastRay(Vector3 direction, ref GameObject closestBlock)
    {
        // Calculate the ray origin at the center of the object
        Vector3 rayOrigin = transform.position;

        // Cast a ray in the specified direction
        Ray ray = new Ray(rayOrigin, direction);

        // Draw the ray in the scene view for visualization
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, rayColor);

        // Perform a raycast and update closestBlock if hit
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            // Check if the hit object has the "Block" tag
            if (hit.collider.CompareTag("Block"))
            {
                closestBlock = hit.collider.gameObject;
            }
        }
    }
}
