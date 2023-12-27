using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BlockManager : MonoBehaviour
{
    public List<GameObject> allBlocks = new List<GameObject>();
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
    public int majorityBlocks;

    // For the Block Used to Split
    Vector3 originalPosition;
    Vector3 originalScale;
    Block objBlock;

    private void Start()
    {
        isInvertedSwipe = true;
    }
    private void Update()
    {
        UpdateAllBlocks();
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
                    allBlocks.Add(smallerCube.gameObject);
                    cubeNumber++;
                }
            }
        }

        Destroy(objBlock.gameObject);
    }
    #endregion

    #region Swipe
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
    #endregion

    #region Move Block
    // If during the game a block is away diagonally from everything move it closer to the closest neighbour.
    // This is an in game mechanic outside of the player's control
    public void CheckDiagonalSide(Block currentBlock, ref GameObject closestBlockObjectDown, ref GameObject closestBlockObjectUp)
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
    }
    public void CheckVerticalConnection(Block currentBlock, ref GameObject closestBlockObjectDown, ref GameObject closestBlockObjectUp)
    {
        if (closestBlockObjectDown != null)
        {
            Block closestBlockDown = closestBlockObjectDown.GetComponent<Block>();

            // Check if there is no block on top and it is not destroyed
            if (closestBlockDown.normalClosestBlockUp != null)
            {
                closestBlockObjectDown = currentBlock.closestBlockDown;
            }
        }
        else if (closestBlockObjectUp != null)
        {
            Block closestBlockUp = closestBlockObjectUp.GetComponent<Block>();

            // Check if there is no block in the bottom and it is not destroyed
            if (closestBlockUp.normalClosestBlockDown != null)
            {
                closestBlockObjectUp = currentBlock.closestBlockUp;
            }
        }
    }
    // When Diagonals are missing
    public void CheckAvailableBlocks(Block currentBlock, ref GameObject closestBlockObjectDown, 
        ref GameObject closestBlockObjectUp, ref GameObject closestBlockObjectLeft, ref GameObject closestBlockObjectRight)
    {
        if (!currentBlock.isAvailableDiagonals)
        {
            // CheckUp & Down Verticals
            if (currentBlock.closestBlockDown != null)
            {
                closestBlockObjectDown = currentBlock.closestBlockDown;
            }
            else if (currentBlock.closestBlockUp != null)
            {
                closestBlockObjectUp = currentBlock.closestBlockUp;
            }
            // What if we have both up and down?
            else if(currentBlock.closestBlockUp != null && currentBlock.closestBlockDown != null)
            {
                int randomIndex = UnityEngine.Random.Range(0, 2);
                if (randomIndex == 0)
                {
                    closestBlockObjectDown = currentBlock.closestBlockDown;
                }
                else
                {
                    closestBlockObjectUp = currentBlock.closestBlockUp;
                }
            }

            // If both closest object down and up are null
            if (closestBlockObjectDown == null && closestBlockObjectUp == null)
            {
                if (currentBlock.closestBlockRight != null)
                {
                    closestBlockObjectRight = currentBlock.closestBlockRight;
                }
                else if (currentBlock.closestBlockLeft != null)
                {
                    closestBlockObjectLeft = currentBlock.closestBlockLeft;
                }
                else if(currentBlock.closestBlockRight != null && currentBlock.closestBlockLeft != null)
                {
                    int randomIndex = UnityEngine.Random.Range(0, 2);
                    if (randomIndex == 0)
                    {
                        closestBlockObjectLeft = currentBlock.closestBlockLeft;
                    }
                    else
                    {
                        closestBlockObjectRight = currentBlock.closestBlockRight;
                    }
                }
            }
        }
    }
    void PositionBlockUp(Block currentBlock, ref GameObject closestBlockObjectDown)
    {
        if (closestBlockObjectDown != null)
        {
            Block closestBlockDown = closestBlockObjectDown.GetComponent<Block>();

            if (closestBlockObjectDown.transform.localScale.x == currentBlock.transform.localScale.x)
            {
                float yValue = closestBlockDown.transform.position.y + closestBlockDown.transform.localScale.x;

                currentBlock.target = new Vector3(closestBlockDown.transform.position.x, yValue + padding, closestBlockDown.transform.position.z);
                currentBlock.isMove = true;
            } //Solved
            else if (currentBlock.transform.localScale.x > closestBlockDown.transform.localScale.x)
            {
                var scaleDiff = currentBlock.gameObject.transform.localScale.x / closestBlockObjectDown.gameObject.transform.localScale.x;

                var xInc = scaleDiff + (scaleDiff / 2);
                var xValue = closestBlockDown.transform.localScale.x * xInc;
                var yValue = currentBlock.transform.localScale.x * (scaleDiff / 2);

                currentBlock.target = new Vector3(xValue, yValue + padding, currentBlock.transform.position.z);
                currentBlock.isMove = true;
            }
            else if (closestBlockDown.transform.localScale.x > currentBlock.transform.localScale.x)
            {
                float scaleDiffFloat = closestBlockDown.transform.localScale.x / currentBlock.transform.localScale.x;
                int scaleDiff = (int)scaleDiffFloat;

                float xValue = 0;
                if(closestBlockObjectDown == currentBlock.closesBlockSouthEast) //Right
                    xValue = XBlock(scaleDiff, closestBlockObjectDown, false);
                else if(closestBlockObjectDown == currentBlock.closesBlockSouthWest) //Left
                    xValue = XBlock(scaleDiff, closestBlockObjectDown, true);
                else
                    xValue = XBlock(scaleDiff, closestBlockObjectDown, true);

                var yValue = closestBlockDown.transform.localScale.x / (scaleDiff * 2);

                currentBlock.target = new Vector3(xValue, yValue + padding, currentBlock.transform.position.z);
                currentBlock.isMove = true;
            } //Solved
        }
    }
    void PositionBlockDown(Block currentBlock, ref GameObject closestBlockObjectUp)
    {
        if (closestBlockObjectUp != null)
        {
            Block closestBlockUp = closestBlockObjectUp.GetComponent<Block>();
            if (closestBlockObjectUp.transform.localScale.x == currentBlock.transform.localScale.x)
            {
                float yValue = closestBlockUp.transform.position.y - closestBlockUp.transform.localScale.x;

                currentBlock.target = new Vector3(closestBlockUp.transform.position.x, yValue - padding, closestBlockUp.transform.position.z);
                currentBlock.isMove = true;
            } //Solved
            else if (currentBlock.transform.localScale.x > closestBlockUp.transform.localScale.x)
            {
                var scaleDiff = currentBlock.gameObject.transform.localScale.x / closestBlockUp.gameObject.transform.localScale.x;

                var xInc = scaleDiff + (scaleDiff / 2);
                var xValue = closestBlockUp.transform.localScale.x * xInc;
                var yValue = -(currentBlock.transform.localScale.x * (scaleDiff / 2));

                currentBlock.target = new Vector3(xValue, yValue - padding, currentBlock.transform.position.z);
                currentBlock.isMove = true;
            }
            else if (closestBlockUp.transform.localScale.x > currentBlock.transform.localScale.x) // Solved
            {
                float scaleDiffFloat = closestBlockUp.transform.localScale.x / currentBlock.transform.localScale.x;
                int scaleDiff = (int)scaleDiffFloat;

                float xValue = 0;
                if (closestBlockObjectUp == currentBlock.closesBlockNorthEast) //Right
                    xValue = XBlock(scaleDiff, closestBlockObjectUp, false);
                else if (closestBlockObjectUp == currentBlock.closesBlockNorthWest) //Left
                    xValue = XBlock(scaleDiff, closestBlockObjectUp, true);
                else
                    xValue = XBlock(scaleDiff, closestBlockObjectUp, true);

                var yValue = closestBlockUp.transform.localScale.x / (scaleDiff * 2);

                currentBlock.target = new Vector3(xValue, yValue - padding, currentBlock.transform.position.z);
                currentBlock.isMove = true;
            } // Solved
        }
    }
    float XBlock(int diff, GameObject closestGameObject, bool isLeft)
    {
        Vector3 pos = closestGameObject.transform.position;
        Vector3 scale = closestGameObject.transform.localScale;

        List<GameObject> cubes = new List<GameObject>();
        GameObject leftCube = null;
        GameObject rightCube = null;

        for (int i = 0; i < diff; i++)
        {
            for (int j = 0; j < diff; j++)
            {
                for (int k = 0; k < diff; k++)
                {
                    Vector3 spawnPosition = pos +
                        new Vector3((i - 0.5f) * (scale.x * splitFactor + padding),
                                    (j - 0.5f) * (scale.y * splitFactor + padding),
                                    (k - 0.5f) * (scale.z * splitFactor + padding));

                    GameObject smallerCube = Instantiate(smallerCubePrefab, spawnPosition, Quaternion.identity);
                    smallerCube.SetActive(false);

                    cubes.Add(smallerCube);
                    if (i == 0 && j == 0 && k == 0)
                    {
                        leftCube = smallerCube;
                    }
                    else if (i == diff - 1 && j == 0 && k == 0)
                    {
                        rightCube = smallerCube;
                    }
                }
            }
        }

        float xValue = isLeft ? leftCube.transform.position.x : rightCube.transform.position.x;

        // Destroy all small cubes
        for (int i = 0; i < cubes.Count; i++)
        {
            Destroy(cubes[i]);
        }

        return xValue;
    }
    // from right
    void PositionBlockLeft(Block currentBlock, ref GameObject closestBlockObjectLeft)
    {
        if (closestBlockObjectLeft != null)
        {
            Block closestBlockLeft = closestBlockObjectLeft.GetComponent<Block>();

            float xValue = closestBlockLeft.transform.position.x + closestBlockLeft.transform.localScale.x;
            float yValue = closestBlockLeft.transform.position.y;

            currentBlock.target = new Vector3(xValue - padding, yValue, closestBlockLeft.transform.position.z);
            currentBlock.isMove = true;
        }
    }
    void PositionBlockRight(Block currentBlock, ref GameObject closestBlockObjectRight)
    {
        if (closestBlockObjectRight != null)
        {
            Block closestBlockRight = closestBlockObjectRight.GetComponent<Block>();

            float xValue = closestBlockRight.transform.position.x - closestBlockRight.transform.localScale.x;
            float yValue = closestBlockRight.transform.position.y;

            currentBlock.target = new Vector3(xValue + padding, yValue, closestBlockRight.transform.position.z);
            currentBlock.isMove = true;
        }
    }
    void PrepareMove(Block currentBlock)
    {
        if (currentBlock.isMove)
        {
            Move(currentBlock, currentBlock.target);
        }
    }
    public void MoveBlockCloser(Block currentBlock)
    {
        // This Move Closer only works when inverted swipe is enabled
        if (!isInvertedSwipe)
            return;

        // When the Block has a Ray Gap - Meaning the other blocks are far away
        if (!currentBlock.isRayGap)
            return;

        GameObject closestBlockObjectUp = null;
        GameObject closestBlockObjectDown = null;
        GameObject closestBlockObjectLeft = null;
        GameObject closestBlockObjectRight = null;

        CheckDiagonalSide(currentBlock, ref closestBlockObjectDown, ref closestBlockObjectUp);
        CheckAvailableBlocks(currentBlock, ref closestBlockObjectDown, ref closestBlockObjectUp, 
                                            ref closestBlockObjectLeft, ref closestBlockObjectRight);

        CheckVerticalConnection(currentBlock, ref closestBlockObjectDown, ref closestBlockObjectUp);

        // Position Blocks
        PositionBlockUp(currentBlock, ref closestBlockObjectDown);
        PositionBlockDown(currentBlock, ref closestBlockObjectUp);
        PositionBlockLeft(currentBlock, ref closestBlockObjectLeft);
        PositionBlockRight(currentBlock, ref closestBlockObjectRight);

        PrepareMove(currentBlock);
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
            blck.isRayGap = false;
        }
    }
    bool CannotMoveToNewPosition(Block block)
    {
        float positionTolerance = 0.001f;

        for (int i = 0; i < allBlocks.Count; i++)
        {
            // Check if the target has a block
            if (Vector3.Distance(allBlocks[i].transform.position, block.target) < positionTolerance)
            {
                return true;
            }
        }

        return false;
    }
    // Get Majority Of Blocks
    void FindMajority()
    {
        
    }
    void CheckRelatedMajorityBlocks()
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            
        }
    }
    #endregion

    void UpdateAllBlocks()
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            if (allBlocks[i] == null)
                allBlocks.Remove(allBlocks[i]);
        }
    }
    public void ResetGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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