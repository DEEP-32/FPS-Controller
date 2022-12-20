using UnityEngine;
using System.Collections;



public class Gun : MonoBehaviour
{
    [Header("Gun Specific")]
    [SerializeField] private float damgAmt=10f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private float lastTimeShot = 0f;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float range = 10f;
    [SerializeField] private float bulletSpreadX = 0.01f;
    [SerializeField] private float bulletSpreadY = 0.01f;
    [SerializeField] private LineRenderer line;

    /*[Header("Recoil")]
    [SerializeField] private Recoil recoilScript = null;
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float snapiness;*/


    [Header("References")]
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private LayerMask WhatIsHitable;
    [SerializeField] private bool allowButtonHold = true;
    [SerializeField] private GameObject bulletEffect;

    private bool isReloading = false;
    private bool shootingInput;
    private bool reloadingInput;

    private Vector3 bulletDir;

    private Recoil recoil_script = null;
    private ObjectPooler pooler = null;





    public int CurrentAmmo { get => currentAmmo; 
        
        set 
        {
           if(value <= 0)
           {
                currentAmmo = 0;
                return;
           }
           if(value > maxAmmo)
           {
                currentAmmo = maxAmmo;
                return;
           }

           currentAmmo = value;


        } 
    }

    private void OnEnable()
    {
        if (!isMagFull() && Input.GetKey(KeyCode.R))
        {
            StartCoroutine(Reloading());
        }
    }
    private void Awake()
    {
        recoil_script = fpsCam.GetComponentInParent<Recoil>();
        CurrentAmmo = maxAmmo;
        pooler = ObjectPooler.instance;
        
    }

    private void Update()
    {
        TakeInput();
        if (reloadingInput && shouldReload())
        {
            Debug.Log("Reloading");
            StartCoroutine(Reloading());
            return;
        }
        if (shootingInput && shouldShoot() && !isMagEmpty())
        {
      
            lastTimeShot = Time.time;
            Shoot();
        }
    }

    private void Shoot()
    {
        CurrentAmmo--;

        recoil_script.recoilOnShoot();
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward + getBulletSpreadVector(), out hit, range,WhatIsHitable))
        {
            Target target = hit.transform.GetComponent<Target>();
            if(target != null)
            {
                target.TakeDamage(damgAmt);
                GameObject impactGO = Instantiate(bulletEffect, hit.point, Quaternion.LookRotation(hit.normal));
                /*GameObject impactGO = pooler.GetPooledObject();
                if (impactGO != null)
                {
                    impactGO.transform.position = hit.transform.position;
                    impactGO.transform.rotation = Quaternion.LookRotation(hit.normal);
                }*/
                if (target.getCurrentHealth() == 0)
                {
                    Destroy(impactGO);
                }
                Destroy(impactGO,3f);
            }


            GameObject impactGONonTarget = Instantiate(bulletEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGONonTarget,3f);
        }

      
    }

    private IEnumerator Reloading()
    {
        isReloading = true;
        Debug.Log("Reloading started");

        yield return new WaitForSeconds(reloadTime);
        CurrentAmmo = maxAmmo;

        Debug.Log("Reloading finished");

        isReloading = false;
    }

    private void TakeInput()
    {
        shootingInput = allowButtonHold? Input.GetKey(KeyCode.Mouse0): Input.GetKeyDown(KeyCode.Mouse0);
        reloadingInput = Input.GetKeyDown(KeyCode.R);
    }

    private bool isMagFull()
    {
        return CurrentAmmo == maxAmmo;
    }

    private bool shouldReload()
    {
        return !isReloading && !isMagFull();
    }

    private bool shouldShoot()
    {
        return controller.canShoot && Time.time >= fireRate + lastTimeShot && !isReloading;
    }

    private Vector3 getBulletSpreadVector()
    {
        bulletDir.x = Random.Range(-bulletSpreadX, bulletSpreadX);
        bulletDir.y = Random.Range(-bulletSpreadY, bulletSpreadY);
        bulletDir.z = 0;
        return bulletDir;
    }

    private bool isMagEmpty()
    {
        return CurrentAmmo == 0;
    }

   
}
