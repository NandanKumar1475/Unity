"Horizontal" → A/D or Left/Right arrow
"Vertical" → W/S or Up/Down arrow
"Mouse X" → Mouse movement left/right
"Mouse Y" → Mouse movement up/down

---------------------------\
--> Input.GetAxisRaw("Mouse X")
Returns how much the mouse moved horizontally in the last frame.
Move mouse right → Positive number
Move mouse left → Negative number

✔ Input.GetAxisRaw("Mouse Y")
Returns how much the mouse moved vertically.
Move mouse up → Positive
Move mouse down → Negative

| Function                      | Smoothing?                    | What you get                     |
| ----------------------------- | ----------------------------- | -------------------------------- |
| `Input.GetAxis("Mouse X")`    | YES (Unity smooths the value) | Soft, slow, filtered movement    |
| `Input.GetAxisRaw("Mouse X")` | NO smoothing                  | Exact mouse delta for that frame |


public class MouseInput : MonoBehaviour
{
 // how to take input from moudr
    float MouseX = Input.GetAxisRaw("Mouse X");
    float MouseY = Input.GetAxixRaw("Mouse Y");
    
    
}
