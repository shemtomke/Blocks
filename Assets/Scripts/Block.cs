using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    BlockManager blockManager;
    private void Start()
    {
        blockManager = FindObjectOfType<BlockManager>();
    }
    // Split Block
    private void OnMouseDown()
    {
        Debug.Log("Split the Cube!");
        blockManager.Split(this.gameObject);
    }
    // Swipe

}
