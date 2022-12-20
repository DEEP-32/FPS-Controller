
using UnityEngine;

public class Recoil : MonoBehaviour
{
    public float x;
    public float y;
    public float z;
    public float returnSpeed;
    public float snapiness;
    Vector3 targetRotation, currentRotation;
    //public Transform fpsCam;

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed*Time.deltaTime);
        currentRotation = Vector3.Lerp(currentRotation, targetRotation, snapiness*Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
        //fpsCam.localRotation = Quaternion.Euler(currentRotation);
    }

    public void recoilOnShoot()
    {
        //Debug.Log($"x:{x}, y: {y},z: {z}, return speed: {returnSpeed}, Snapiness: {snapiness}");
        targetRotation += new Vector3 (x, Random.Range(-y,y), Random.Range(-z,z));
    }
}
