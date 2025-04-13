using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float edgeThreshold = 50f;
    
    public SpriteRenderer BoardSprite;

    private Vector3 lowBounds => BoardSprite.bounds.min;
    private Vector3 highBounds => BoardSprite.bounds.max;


    void Update()
    {
        Vector3 move = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;
        
        if (mousePos.x <= edgeThreshold) move.x = -1;
        if (mousePos.x >= Screen.width - edgeThreshold) move.x = 1;
        if (mousePos.y <= edgeThreshold) move.y = -1; 
        if (mousePos.y >= Screen.height - edgeThreshold) move.y = 1; 
        
        move.Normalize();
        transform.position += move * moveSpeed * Time.deltaTime;

        float clampedX = Mathf.Clamp(transform.position.x, lowBounds.x, highBounds.x);
        float clampedY = Mathf.Clamp(transform.position.y, lowBounds.y, highBounds.y);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
