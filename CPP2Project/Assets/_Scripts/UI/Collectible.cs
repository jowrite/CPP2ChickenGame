using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Points Properties")]
    [SerializeField] private int points = 10;
    //[SerializeField] private GameObject collectEffect;


    public int GetPoints() => points;

    public void Collect()
    {
        //if (collectEffect != null)
          //  Instantiate(collectEffect, transform.position, Quaternion.identity);

       // gameObject.SetActive(false);
    }
}
