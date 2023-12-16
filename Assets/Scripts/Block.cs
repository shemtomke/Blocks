using UnityEngine;

public class Block : MonoBehaviour
{
    private Vector3 dragStartPos;
    private Vector3 dragEndPos;
    private float dragThreshold = 10f; // Adjust this value to set the drag sensitivity
    private bool isDragging = false;

    BlockManager blockManager;

    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();
    }

    // Split Block
    private void OnMouseDown()
    {
        // Record the start position for both clicks and potential drags
        dragStartPos = Input.mousePosition;
    }

    // Swipe / Drag
    private void OnMouseUp()
    {
        // If it's not a drag (within the threshold), consider it a click
        if (!isDragging)
        {
            Debug.Log("Split the Cube!");
            blockManager.Split(this.gameObject);
        }

        isDragging = false; // Reset the dragging flag
    }

    private void OnMouseDrag()
    {
        isDragging = true;

        dragEndPos = Input.mousePosition;

        // Calculate the drag delta
        Vector3 dragDelta = dragEndPos - dragStartPos;

        // Check for horizontal drag (left or right)
        if (Mathf.Abs(dragDelta.x) > Mathf.Abs(dragDelta.y) && Mathf.Abs(dragDelta.x) > dragThreshold)
        {
            if (dragDelta.x < 0)
            {
                // Drag left
                Debug.Log("Drag left!");
            }
            else
            {
                // Drag right
                Debug.Log("Drag right!");
            }
        }
        // Check for vertical drag (up or down)
        else if (Mathf.Abs(dragDelta.y) > Mathf.Abs(dragDelta.x) && Mathf.Abs(dragDelta.y) > dragThreshold)
        {
            if (dragDelta.y < 0)
            {
                // Drag down
                Debug.Log("Drag down!");
            }
            else
            {
                // Drag up
                Debug.Log("Drag up!");
            }
        }
    }
}
