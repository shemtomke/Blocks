using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Material whiteMaterial;
    public Material blackMaterial;

    private void Start()
    {
        this.GetComponent<Renderer>().material = blackMaterial;
    }
    void Split()
    {
        // Divide the Block into 4 (0, 1, 2, 3)
        // Odd Blocks take Black (1, 3)
        // Even Blocks take white (0, 2)

    }
    void Swipe()
    {

    }
    void InvertedSwipe()
    {

    }
    private void OnMouseDown()
    {
        Debug.Log("Split the Cube!");
    }
}
