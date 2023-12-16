using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject smallerCubePrefab;

    public Material whiteMaterial;
    public Material blackMaterial;

    bool isInvertedSwipe;
    int cubeNumber = 0;

    private void Start()
    {
        isInvertedSwipe = false;
    }
    public void AllowInversion(bool isInvert)
    {
        isInvertedSwipe = isInvert;
    }
    public void Split(GameObject obj)
    {
        Vector3 originalPosition = obj.transform.position;
        Vector3 originalScale = obj.transform.localScale;

        // When increasing the Scale for More cubes, ensure it is divisible by 3 to allow easier split
        float splitFactor = 0.5f;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Vector3 spawnPosition = originalPosition +
                new Vector3((i - 0.5f) * originalScale.x * splitFactor,
                            (j - 0.5f) * originalScale.y * splitFactor,
                            0f);

                GameObject smallerCube = Instantiate(smallerCubePrefab, originalPosition, Quaternion.identity);
                smallerCube.name = "Cube " + cubeNumber;
                Material cubeMaterial = (i + j) % 2 == 0 ? blackMaterial : whiteMaterial;
                Block blockComponent = smallerCube.GetComponent<Block>();
                blockComponent.isBlack = cubeMaterial == blackMaterial;
                blockComponent.isWhite = cubeMaterial == whiteMaterial;
                smallerCube.GetComponent<Renderer>().material = cubeMaterial;
                smallerCube.transform.localScale = originalScale * splitFactor;
                cubeNumber++;
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
        var clstBlck = closestBlock.GetComponent<Block>();
        var closestBlockMaterial = closestBlock.gameObject.GetComponent<Renderer>().material;

        // Check if the blocks are of the same type (color)
        bool sameColor = curBlck.isWhite == clstBlck.isWhite || curBlck.isBlack == clstBlck.isBlack;

        // swiping to block should merge with the current block color
        if (closestBlock.transform.localScale.x > currentBlock.transform.localScale.x)
        {
            if (isInvertedSwipe)
            {
                Debug.Log(closestBlockMaterial.name);
                // Inverted swipe logic
                if (sameColor)
                {
                    // The next cube must have an opposite color to allow merge
                    closestBlockMaterial = curBlck.isWhite 
                        ? closestBlock.gameObject.GetComponent<Renderer>().material = blackMaterial
                        : closestBlock.gameObject.GetComponent<Renderer>().material = whiteMaterial;
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (!sameColor)
                    return;
            }

            // destroy the currentBlock
            Destroy(currentBlock);
        }
        else
        {
            Debug.Log("Can't Swipe, Big in Size!");
        }
    }
}
