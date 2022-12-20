using UnityEngine;

public class Crosshair : MonoBehaviour
{
    private void Start()
    {
        if (!isCursorVisible())
            transform.position = Input.mousePosition;
    }
    private void Update()
    {
        if(!isCursorVisible())  
           transform.position = Input.mousePosition;
    }

    private bool isCursorVisible()
    {
        return Cursor.visible;
    }

   /* private bool isOutOfBounds()
    {

    }*/
}
