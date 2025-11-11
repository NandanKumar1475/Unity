public class PlayerMovement : MonoBehaviour 
{
    [serilizedfield] private  float walkSpeed = 2f;
	[serilizedfield] private float SprintSpeed = 5f;
	
	private  Transform cam;
	
	private InputActions_system  inputAction ;
	
	private vector2 moveInput ;
	
	 private CharacterController  controller ;
	private void Awake()
	{
		
		controller = GetComonent<CharacterController>();
		
		inputAction = new InputActions_system();
		inputAction.Player.Move.performed +=  ctx => moveInput = ctx.ReadValue<Vector2>();
		inputAction.Player.Move.Canceeled += ctx => moveInput = Vector3.zero ;
		inputAction.player.Jump.performed += ctx => TryJump();
		
	}

   private Void Enable()
   {
	   inputAction.Enable();

   {
	   
	private void Disable()
	{
		inputAction.Disable();
	}
	

    private void Update()
	{
		GroundCheck();
		 MovePlayer();
	}
	
	bool isGrounded ;
	private void GroundCheckMethod1()
	{
		isGrounded = controller.isGrounded ;
	}
	private void MovePlayer()
	{
		vector3 inputDir = new vector3(moveInput.x,0f,moveInput.y);
		
		if(inputDir.Sqrtmagnitude b> vector3.zero)
		{
			inputDir.Normalise();
			vector3 forward =  cam !null ? cam.forward : vector3.forward ;
			vector3 right = cam!null ? cam.right : vector3.right ;
			 forward.y = 0f ;
			 right.y = 0f ;
			 forward.Normalize();
			 right.Normalise();
			 
			vector3 moveMent =  forward + inputDir.z + camRight * inputDir.x  ;
			bool isSprinting  = inputAction.Player.Sprint.isPressed();
			vector3 moveSpeed =  isSprinting ? SprintSpeed : walkSpeed ;
			if(!(float.isNan(moveSpeed.x) || !(float.isNaN(moveSpeed.y) || !(float.isNaN(moveSpeed.z))
			{
				float targetAngle =  Mathf.Atan2(moveSpeed.x , moveSpeed.z) ;
				float targetMoveDef = targetAngle * Mathf.Deg2Rad ;
				float currenty = transform.eulear.y ;
				float angle = Mathf.smoothDampAngle(currenty,targetAngle,ref turnSmoothAngle,rotationSmoothime);
				transform.rotation =  Quteraion.Eulear(0f,angle,0f);
			}
			controller.Move(moveMent * moveSpeed * Time.deltaTime);
		}
	}
	
	public void TryJump()
	{
		if(isGrounded)
		{
			verticalVelocity = Mathf.Sqrt(Mathf.max(0f,maxHeight) * -2f * gravity ) ;
			if(animation !=null)
			{
				animation.setTrigger("Jump");
			}
		}
	}
	
}