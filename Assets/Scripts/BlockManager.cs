using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject smallerCubePrefab;

    public Material whiteMaterial;
    public Material blackMaterial;

    public float splitDistance = 1.0f;
    public void Split(GameObject obj)
    {
        Vector3 originalPosition = obj.transform.position;

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
    public void Swipe()
    {

    }
    public void InvertedSwipe()
    {

    }
}
