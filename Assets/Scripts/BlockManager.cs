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

    public float moveSpeed;

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
                    return;
                }
            }
            else
            {
                if (!sameColor)
                {
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
    public void MoveBlockCloser(Block currentBlock)
    {
        GameObject closestBlockObject = null;
        GameObject currentBlockObject = currentBlock.gameObject;

        // if you are missing all normal directions (Up, Down, Left, Right)
        if (currentBlock.closestBlockDown == null && currentBlock.closestBlockLeft == null 
            && currentBlock.closestBlockRight == null && currentBlock.closestBlockUp == null)
        {
            // Check Diagonal Side
            if(currentBlock.closesBlockSouthEast != null || currentBlock.closesBlockNorthEast != null || 
                currentBlock.closesBlockNorthWest != null || currentBlock.closesBlockSouthWest != null)
            {
                if (currentBlock.closesBlockSouthEast != null)
                {
                    closestBlockObject = currentBlock.closesBlockSouthEast;
                }
                else if (currentBlock.closesBlockNorthEast != null)
                {
                    closestBlockObject = currentBlock.closesBlockNorthEast;
                }
                else if (currentBlock.closesBlockNorthWest != null)
                {
                    closestBlockObject = currentBlock.closesBlockNorthWest;
                }
                else if (currentBlock.closesBlockSouthWest != null)
                {
                    closestBlockObject = currentBlock.closesBlockSouthWest;
                }

                Block closestBlock = closestBlockObject.GetComponent<Block>();

                if (closestBlockObject.transform.localScale.x == currentBlock.transform.localScale.x)
                {
                    Debug.Log("Same Size!");
                    Vector3 target = new Vector3(closestBlockObject.transform.position.x, currentBlockObject.transform.position.y, 
                        currentBlockObject.transform.position.z);

                    Move(currentBlockObject, target);
                }
                else if(currentBlock.transform.localScale.x > closestBlockObject.transform.localScale.x)
                {
                    Debug.Log("Current Block is Bigger Size!");
                    var xValue = closestBlockObject.transform.position.x * 
                        (currentBlockObject.transform.localScale.x / closestBlockObject.transform.localScale.x);

                    Vector3 target = new Vector3(xValue, closestBlockObject.transform.position.y,
                        closestBlockObject.transform.position.z);

                    Move(currentBlockObject, target);
                }
                else if(closestBlockObject.transform.localScale.x > currentBlock.transform.localScale.x)
                {
                    Debug.Log("Closest Block is Bigger Size!");
                    var xValue = currentBlockObject.transform.position.x - currentBlockObject.transform.localScale.x;

                    Vector3 target = new Vector3(xValue, closestBlockObject.transform.position.y,
                        closestBlockObject.transform.position.z);

                    Move(currentBlockObject, target);
                }
            }
        }
    }
    void Move(GameObject obj, Vector3 target)
    {
        // Calculate the new position using Vector3.Lerp
        Vector3 newPosition = Vector3.Lerp(obj.transform.position, target, moveSpeed * Time.deltaTime);

        // Set the object's position to the new calculated position
        obj.transform.position = newPosition;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
public enum Colors
{
    White, Black
}