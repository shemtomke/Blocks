using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class BlockManager : MonoBehaviour
{
    public GameObject smallerCubePrefab;

    public Material whiteMaterial;
    public Material blackMaterial;

    public bool sameColorSplit = false;
    bool isInvertedSwipe;
    int cubeNumber = 0;
    public int splitNumber = 0;

    public Text splitUI;

    private void Start()
    {
        isInvertedSwipe = false;
    }
    public void AllowInversion(bool isInvert)
    {
        isInvertedSwipe = isInvert;
    }
    public void Split(GameObject obj) // White Split (Normal Split)
    {
        Vector3 originalPosition = obj.transform.position;
        Vector3 originalScale = obj.transform.localScale;
        Block objBlock = obj.GetComponent<Block>();

        // When increasing the Scale for More cubes, ensure it is divisible by 3 to allow easier split
        float splitFactor = 0.5f;

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++) // Added this loop for the z axis
                {
                    Vector3 spawnPosition = originalPosition +
                    new Vector3((i - 0.5f) * originalScale.x * splitFactor,
                              (j - 0.5f) * originalScale.y * splitFactor,
                              (k - 0.5f) * originalScale.z * splitFactor); // Adjusted this line to include the z axis

                    GameObject smallerCube = Instantiate(smallerCubePrefab, spawnPosition, Quaternion.identity);
                    smallerCube.name = "Cube " + cubeNumber;
                    Material cubeMaterial = null;
                    if(sameColorSplit)
                    {
                        // if white then split w, b
                        if (objBlock.isWhite)
                        {
                            cubeMaterial = (i + j + k) % 2 == 0 ? blackMaterial : whiteMaterial;
                        }
                        // else black then b, w
                        else if(objBlock.isBlack)
                        {
                            cubeMaterial = (i + j + k) % 2 == 0 ? whiteMaterial : blackMaterial;
                        }
                    }
                    else
                    {
                        cubeMaterial = (i + j + k) % 2 == 0 ? blackMaterial : whiteMaterial;
                    }
                    Block blockComponent = smallerCube.GetComponent<Block>();
                    blockComponent.isBlack = cubeMaterial == blackMaterial;
                    blockComponent.isWhite = cubeMaterial == whiteMaterial;
                    smallerCube.GetComponent<Renderer>().material = cubeMaterial;
                    smallerCube.transform.localScale = originalScale * splitFactor;

                    // If the cube number is not divisible by 2, disable or destroy the cube
                    if (cubeNumber % 2 != 0)
                    {
                        Destroy(smallerCube);
                    }

                    cubeNumber++;
                }
            }
        }
        splitNumber++;
        splitUI.text = splitNumber.ToString();
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

        // The merging block has to be the same or smaller size than the block it merges into.
        if (closestBlock.transform.localScale.x >= currentBlock.transform.localScale.x)
        {
            if (isInvertedSwipe)
            {
                // Inverted swipe logic
                if (sameColor)
                {
                    // The next cube must have an opposite color to allow merge
                    if(curBlck.isWhite)
                    {
                        closestBlock.gameObject.GetComponent<Renderer>().material = blackMaterial;
                        clstBlck.isBlack = true;
                        clstBlck.isWhite = false;
                    }
                    else
                    {
                        closestBlock.gameObject.GetComponent<Renderer>().material = whiteMaterial;
                        clstBlck.isBlack = false;
                        clstBlck.isWhite = true;
                    }
                }
                else
                {
                    Debug.Log("Not Merging On Inverted!");
                    return;
                }
            }
            else
            {
                if (!sameColor)
                {
                    Debug.Log("Not Merging Normal!");
                    return;
                } 
            }

            // destroy the currentBlock
            Destroy(currentBlock);
        }
        else
        {
            Debug.Log("Can't Swipe, Big in Size!");
        }
    }
    // If during the game a block is away diagonally from everything move it closer to the closest neighbour.
    // This is an in game mechanic outside of the player's control

    public void QuitGame()
    {
        Application.Quit();
    }
}
public enum Colors
{
    White, Black
}