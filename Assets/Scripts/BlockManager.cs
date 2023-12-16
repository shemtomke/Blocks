using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject smallerCubePrefab;

    public Material whiteMaterial;
    public Material blackMaterial;

    bool isInvertedSwipe = false;

    //public float splitDistance = 1.0f;
    public void Split(GameObject obj)
    {
        Vector3 originalPosition = obj.transform.position; 

        // When increasing the Scale for More cubes ensure it is divisible by 3 to allow easier split
        float splitDistance = obj.transform.localScale.x;

        if (obj.transform.localScale.x == 1)
            splitDistance *= 0.5f;
        else
            splitDistance -= 1;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawnPosition = originalPosition + new Vector3((i - 0.5f) * splitDistance, (j - 0.5f) * splitDistance, 0f);
                GameObject smallerCube = Instantiate(smallerCubePrefab, spawnPosition, Quaternion.identity);
                Material cubeMaterial = (i + j) % 2 == 0 ? blackMaterial : whiteMaterial;
                smallerCube.GetComponent<Renderer>().material = cubeMaterial;
                smallerCube.transform.localScale = obj.transform.localScale.x == 1 ? obj.transform.localScale * 0.5f : obj.transform.localScale - new Vector3(1, 1, 1);
            }
        }
        
        Destroy(obj);
    }
    public void Swipe(GameObject currentBlock, GameObject closestBlock)
    {
        if (closestBlock == null)
            return;

        var curBlck = currentBlock.GetComponent<Block>();
        var curMat = currentBlock.GetComponent<Renderer>().material;

        //Neighbouring Block
        var closestBlockMaterial = closestBlock.gameObject.GetComponent<Renderer>().material;

        // swiping to block should merge with the current block color
        if (closestBlock.transform.localScale.x > currentBlock.transform.localScale.x)
        {
            if (isInvertedSwipe)
            {
                // The next cube must have an opposite color to allow merge
                if (curMat == whiteMaterial)
                {
                    closestBlockMaterial = blackMaterial;
                }
                else if (curMat == blackMaterial)
                {
                    closestBlockMaterial = whiteMaterial;
                }
            }
            else
            {
                // Next Cube takes the same material color if the currentblock has the same color as the next block
                if(curMat == closestBlockMaterial)
                {
                    closestBlockMaterial = curMat;
                    Debug.Log("Same Color!");
                }
                else
                {
                    Debug.Log("Not same Color!");
                }
            }

            // destroy the currentBlock
            Destroy(currentBlock);
        }
        else
        {
            Debug.Log("Small Size!");
        }
    }
}
