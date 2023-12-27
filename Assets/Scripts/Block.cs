using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public Text splitUI;
    public Vector3 initialPos;

    [Space(10)]
    public GameObject closestBlockUp;
    public GameObject closestBlockDown;
    public GameObject closestBlockLeft;
    public GameObject closestBlockRight;

    [Space(20)]
    public GameObject normalClosestBlockUp;
    public GameObject normalClosestBlockDown;
    public GameObject normalClosestBlockLeft;
    public GameObject normalClosestBlockRight;

    [Space(20)]

    public GameObject closesBlockNorthWest;
    public GameObject closesBlockNorthEast;
    public GameObject closesBlockSouthEast;
    public GameObject closesBlockSouthWest;

    [Space(10)]
    public float sphereRadius = 0.2f;
    public float rayDistance = 2f;
    public float rayGap;
    private float dragThreshold = 10f; // Adjust this value to set the drag sensitivity

    public Color rayColor = Color.red;

    public Vector3 target;
    private Vector3 dragStartPos;
    private Vector3 dragEndPos;

    public Vector3 northeastDiagonal;
    public Vector3 northwestDiagonal;
    public Vector3 southeastDiagonal;
    public Vector3 southwestDiagonal;

    public bool isMove = false;
    public bool isRayGap = false;
    public bool isAvailableDiagonals = false;
    private bool isDragging = false;
    public bool isWhite, isBlack;

    BlockManager blockManager;

    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();

        rayGap = transform.localScale.x;

        isDragging = false;
        initialPos = transform.position;
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
        if(!isMove)
        {
            CastRay(northeastDiagonal, ref closesBlockNorthEast, rayDistance);
            CastRay(northwestDiagonal, ref closesBlockNorthWest, rayDistance);
            CastRay(southeastDiagonal, ref closesBlockSouthEast, rayDistance);
            CastRay(southwestDiagonal, ref closesBlockSouthWest, rayDistance);

            // Cast rays in four directions from the center of the object
            CastRay(Vector3.up, ref closestBlockUp, rayDistance);
            CastRay(Vector3.down, ref closestBlockDown, rayDistance);
            CastRay(Vector3.left, ref closestBlockLeft, rayDistance);
            CastRay(Vector3.right, ref closestBlockRight, rayDistance);

            // This will show if we have a gap with the next cube if null
            BoxCastRay(Vector3.up, ref normalClosestBlockUp, rayGap);
            BoxCastRay(Vector3.down, ref normalClosestBlockDown, rayGap);
            BoxCastRay(Vector3.left, ref normalClosestBlockLeft, rayGap);
            BoxCastRay(Vector3.right, ref normalClosestBlockRight, rayGap);

            AllRaysGapEmpty();
            NoDiagonals();
            AvailableDiagonals();
        }

        blockManager.MoveBlockCloser(this);
    }
    void OnDrawGizmos()
    {
        //Vector3 northeastDiagonal = new Vector3(1f, 1f, 0f).normalized;
        //Vector3 northwestDiagonal = new Vector3(-1f, 1f, 0f).normalized;
        //Vector3 southeastDiagonal = new Vector3(1f, -1f, 0f).normalized;
        //Vector3 southwestDiagonal = new Vector3(-1f, -1f, 0f).normalized;

        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(transform.position - northeastDiagonal * rayDistance, sphereRadius);
        //Gizmos.DrawSphere(transform.position - northwestDiagonal * rayDistance, sphereRadius);
        //Gizmos.DrawSphere(transform.position - southeastDiagonal * rayDistance, sphereRadius);
        //Gizmos.DrawSphere(transform.position - southwestDiagonal * rayDistance, sphereRadius);
    }
    public void CastRay(Vector3 direction, ref GameObject closestBlock, float rayDist)
    {
        // Calculate the ray origin at the center of the object
        Vector3 rayOrigin = transform.position;

        // Cast a ray in the specified direction
        Ray ray = new Ray(rayOrigin, direction);

        // Draw the ray in the scene view for visualization
        Debug.DrawRay(ray.origin, ray.direction * rayDist, rayColor);

        // Perform a raycast and update closestBlock if hit
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDist))
        {
            if (hit.collider.CompareTag("Block"))
            {
                closestBlock = hit.collider.gameObject;
            }
        }
    }
    public void BoxCastRay(Vector3 direction, ref GameObject closestBlock, float rayDist)
    {
        // Calculate the ray origin at the center of the object
        Vector3 rayOrigin = transform.position;

        // Perform a raycast and update closestBlock if hit
        RaycastHit hit;
        if (Physics.BoxCast(rayOrigin, transform.lossyScale * 0.5f, direction, out hit, Quaternion.identity, rayDist))
        {
            Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
            // Check if the hit object has the "Block" tag
            if (hit.collider.CompareTag("Block"))
            {
                closestBlock = hit.collider.gameObject;
            }
        }
    }
    public void SphereCastRay(Vector3 direction, ref GameObject closestBlock, float radius, float rayDist)
    {
        Vector3 rayOrigin = transform.position;
        RaycastHit hit;
        if (Physics.SphereCast(rayOrigin, radius, direction, out hit, rayDist))
        {
            // Your code for handling the hit
            if (hit.collider.CompareTag("Block"))
            {
                closestBlock = hit.collider.gameObject;
            }
        }
    }
    // if the ray gap is empty then there is a gap between, meaning it should move to the closest neighbour
    public void AllRaysGapEmpty()
    {
        if (normalClosestBlockUp == null && normalClosestBlockDown == null &&
            normalClosestBlockLeft == null && normalClosestBlockRight == null)
            isRayGap = true;
    }
    // Check when we can't detect the diagonals 
    public void NoDiagonals()
    {
        if (closesBlockNorthEast == null && closesBlockNorthWest == null &&
            closesBlockSouthEast == null && closesBlockSouthWest == null)
            isAvailableDiagonals = false;
    }
    public void AvailableDiagonals()
    {
        if (closesBlockNorthEast != null || closesBlockNorthWest != null ||
            closesBlockSouthEast != null || closesBlockSouthWest != null)
            isAvailableDiagonals = true;
    }
}
