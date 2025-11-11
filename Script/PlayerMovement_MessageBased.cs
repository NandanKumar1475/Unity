public class MovePlayer_MessageBaesd : MonoBehaviour
{
  public float Walkspeed = 2f;
  public float SprintSpeed = 10f;
  
  private Transform cam;
  private CharacterController cc;
  
  private Vector2 MoveInput ;
  
  private bool isSprinting
  
  Void Awake()
  {
	 cc = GetComponent<CharacterController>(); 
  }
  
  void start()
  {
	cam  = Camera.main ? Camera.main.Transform : null; 
  }
  
  void onMove(InputValue value)
  {
	  MoveInput = value.Get<Vector>();
  }
  
  void OnSprint(InputValue value)
  {
	 isSprinting = value.isPressed;
  }
  
  void Update()
  {
	  vector3 dir = new  vector3(MoveInput.x ,0f,MoveInput.y);
	  
	  if(dir.magnitude > 0.01f and cam !=null )
	  {
		  // for forward
	  vector3 forward = cam.forward;
	  forward.y = 0;
	  forward.normalize();
	  
	  // for backward
	  
	  vector3 right = cam.right;
	  right.y = 0 ;
	  right.normalize();
	  dir = forward * MoveInput.y + right * MoveInput.x ;
	  
	  }
	  
	  if(dir.magnitude > 0.001)
	  {
		  dir.normalize();
		  Quterian target = Quaterian.LookRotation(dir,vector3.up);
		  transform.rotation = Quaterian.RotateTowards(
		   transform.rotation,target,turnSpeed* Time.deltaTime);
		   
		   float targetSpeed =  isSprinting;SprintSpeed : Walkspeed ;
		   cc.Move(dir . target * Time.deltaTime);
	  }
	  
	  
  }
  
  void PlayerMovement()
  {
  }
  
  
  
}