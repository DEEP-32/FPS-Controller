using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : MonoBehaviour
{
    [SerializeField] private Transform fpsCam;
    bool enemyInRange;
    // Start is called before the first frame update
    void Start()
    {
          
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        enemyInRange = Physics.Raycast(fpsCam.position, fpsCam.forward, 2f);
        //Debug.Log($"Enemy in Range: {enemyInRange}");
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collided");
        if (enemyInRange)
        {
            if (other.collider.tag=="Enemy")
            {
                Destroy(other.gameObject);
            }
        }
    }
}
