using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public List<GameObject> pooledObjects;
    public int AmountToPooled;
    public static ObjectPooler instance = null;
    public GameObject objectToPool;
    GameObject temp;
    private void Awake()
    {
        if(instance == null)
            instance = this;    
    }
    private void Start()
    {
        pooledObjects = new List<GameObject>();
        for(int i = 0; i < AmountToPooled; i++)
        {
            temp = Instantiate(objectToPool);
            temp.SetActive(false);
            pooledObjects.Add(temp);
        }
    }

    public GameObject GetPooledObject()
    {
       for(int i = 0; i < AmountToPooled; i++)
       {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
       }
        return null;

    }

   


}