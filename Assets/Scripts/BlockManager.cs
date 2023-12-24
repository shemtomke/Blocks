using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class BlockManager : MonoBehaviour
{
    public GameObject smallerCubePrefab;

    public Material whiteMaterial;
    public Material blackMaterial;

    public float moveSpeed;
    public float padding;
    float splitFactor = 0.5f;

    public bool sameColorSplit = false;
    public bool isInvertedSwipe;
    int cubeNumber = 0;
    public int splitNumber = 0;

    // For the Block Used to Split
    Vector3 originalPosition;
    Vector3 originalScale;
    Block objBlock;

    private void Start()
    {
        isInvertedSwipe = false;
    }
    public void AllowInversion(bool isInvert)
    {
        isInvertedSwipe = isInvert;

        sameColorSplit = isInvertedSwipe;
    }
    #region Split
    public void Split(GameObject obj)
    {
        originalPosition = obj.transform.position;
        originalScale = obj.transform.localScale;
        objBlock = obj.GetComponent<Block>();

        splitNumber++;
        if (objBlock.isBlack) { objBlock.splitUI.color = UnityEngine.Color.white; }
        else if (objBlock.isWhite) { objBlock.splitUI.color = UnityEngine.Color.black; }
        objBlock.splitUI.gameObject.SetActive(true);
        objBlock.splitUI.text = splitNumber.ToString();

        // Wait for 1 second before starting the splitting process
        Invoke("StartSplitting", 0.5f);
    }
    void StartSplitting()
    {
        SplitCubes(originalPosition, originalScale, objBlock, splitFactor);
    }

    // SplitCubes method that takes parameters
    void SplitCubes(Vector3 position, Vector3 scale, Block block, float factor)
    {
        objBlock.splitUI.gameObject.SetActive(false);

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    Vector3 spawnPosition = position +
                        new Vector3((i - 0.5f) * (scale.x * factor + padding),
                                    (j - 0.5f) * (scale.y * factor + padding),
                                    (k - 0.5f) * (scale.z * factor + padding));

                    GameObject smallerCube = Instantiate(smallerCubePrefab, spawnPosition, Quaternion.identity);
                    smallerCube.name = "Cube " + cubeNumber;
                    Material cubeMaterial = null;

                    if (sameColorSplit)
                    {
                        // if white then split w, b
                        if (block.isWhite)
                        {
                            cubeMaterial = (i + j + k) % 2 == 0 ? blackMaterial : whiteMaterial;
                        }
                        // else black then b, w
                        else if (block.isBlack)
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
                    smallerCube.transform.localScale = scale * factor;

                    // If the cube number is not divisible by 2, disable or destroy the cube
                    if (cubeNumber % 2 != 0)
                    {
                        Destroy(smallerCube);
                    }

                    cubeNumber++;
                }
            }
        }

        Destroy(objBlock.gameObject, 0.5f);
    }
    #endregion
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
        if (!isInvertedSwipe)
            return;

        GameObject closestBlockObjectUp = null;
        GameObject closestBlockObjectDown = null;
        GameObject currentBlockObject = currentBlock.gameObject;

        // if you are missing all normal directions (Up, Down, Left, Right)
        if (currentBlock.closestBlockDown == null && currentBlock.closestBlockLeft == null 
            && currentBlock.closestBlockRight == null && currentBlock.closestBlockUp == null)
        {
            Debug.Log("Found a close Block!");
            // Check Diagonal Side
            if(currentBlock.closesBlockSouthEast != null || currentBlock.closesBlockNorthEast != null || 
                currentBlock.closesBlockNorthWest != null || currentBlock.closesBlockSouthWest != null)
            {
                if (currentBlock.closesBlockSouthEast != null)
                {
                    closestBlockObjectDown = currentBlock.closesBlockSouthEast;
                }
                else if (currentBlock.closesBlockNorthEast != null)
                {
                    closestBlockObjectUp = currentBlock.closesBlockNorthEast;
                }
                else if (currentBlock.closesBlockNorthWest != null)
                {
                    closestBlockObjectUp = currentBlock.closesBlockNorthWest;
                }
                else if (currentBlock.closesBlockSouthWest != null)
                {
                    closestBlockObjectDown = currentBlock.closesBlockSouthWest;
                }

                if(closestBlockObjectDown != null)
                {
                    Block closestBlockDown = closestBlockObjectDown.GetComponent<Block>();

                    if (closestBlockObjectDown.transform.localScale.x == currentBlock.transform.localScale.x)
                    {
                        float yValue = closestBlockDown.transform.position.y + closestBlockDown.transform.localScale.x;

                        currentBlock.target = new Vector3(closestBlockDown.transform.position.x, yValue, closestBlockDown.transform.position.z);
                        currentBlock.isMove = true;
                    }
                    else if (currentBlock.transform.localScale.x > closestBlockDown.transform.localScale.x)
                    {
                        var scaleDiff = currentBlockObject.gameObject.transform.localScale.x / closestBlockObjectDown.gameObject.transform.localScale.x;

                        var xInc = scaleDiff + (scaleDiff / 2);
                        var xValue = currentBlockObject.transform.localScale.x * xInc;
                        var yValue = currentBlock.transform.localScale.x * (scaleDiff / 2);

                        currentBlock.target = new Vector3(xValue, yValue, currentBlock.transform.position.z);
                        currentBlock.isMove = true;
                    }
                    else if (closestBlockDown.transform.localScale.x > currentBlock.transform.localScale.x)
                    {
                        float scaleDiff = closestBlockDown.transform.localScale.x / currentBlock.transform.localScale.x;
                        var xValue = currentBlock.initialPos.x - closestBlockDown.transform.localScale.x;
                        var yValue = closestBlockDown.transform.localScale.x / (scaleDiff * 2);

                        currentBlock.target = new Vector3(xValue, yValue, currentBlockObject.transform.position.z);
                        currentBlock.isMove = true;
                    }
                }
                else if(closestBlockObjectUp != null)
                {
                    Block closestBlockUp = closestBlockObjectUp.GetComponent<Block>();

                    if (closestBlockObjectUp.transform.localScale.x == currentBlock.transform.localScale.x)
                    {
                        // This is for bottom
                        float yValue = closestBlockUp.transform.position.y - closestBlockUp.transform.localScale.x;

                        currentBlock.target = new Vector3(closestBlockUp.transform.position.x, yValue, closestBlockUp.transform.position.z);
                        currentBlock.isMove = true;
                    }
                    else if (currentBlock.transform.localScale.x > closestBlockUp.transform.localScale.x)
                    {
                        Debug.Log("Current Block is Bigger Size! " + closestBlockUp.name);
                        var scaleDiff = currentBlockObject.gameObject.transform.localScale.x / closestBlockUp.gameObject.transform.localScale.x;

                        var xValue = - currentBlockObject.transform.localScale.x;
                        var yValue = - (currentBlockObject.transform.localScale.x * (scaleDiff / 2));

                        currentBlock.target = new Vector3(xValue, yValue, currentBlock.transform.position.z);
                        currentBlock.isMove = true;
                    }
                    else if (closestBlockUp.transform.localScale.x > currentBlock.transform.localScale.x)
                    {
                        Debug.Log("Closest Block is Bigger Size! " + closestBlockUp.name);
                        float scaleDiff = closestBlockUp.transform.localScale.x / currentBlock.transform.localScale.x;
                        var xValue = currentBlock.initialPos.x - closestBlockUp.transform.localScale.x;
                        var yValue = closestBlockUp.transform.localScale.x / (scaleDiff * 2);

                        currentBlock.target = new Vector3(xValue, yValue, currentBlockObject.transform.position.z);
                        currentBlock.isMove = true;
                    }
                }
            }
        }

        if(currentBlock.isMove)
        {
            Move(currentBlock, currentBlock.target);
        }
    }
    void Move(Block blck, Vector3 target)
    {
        GameObject obj = blck.gameObject;
        var pos = obj.transform.position;

        // Set the object's position to the new calculated position
        obj.transform.position = Vector3.MoveTowards(pos, target, moveSpeed * Time.deltaTime);

        if (Mathf.Approximately(pos.x, target.x) &&
            Mathf.Approximately(pos.y, target.y) &&
            Mathf.Approximately(pos.z, target.z))
        {
            blck.isMove = false;
        }
    }
    // Get Majority Of Blocks
    void FindMajority()
    {
        
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