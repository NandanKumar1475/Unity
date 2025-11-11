// player movement 

[RequireComponent[typoeof(charactercollider)]
public void PlayerMovement : MonoBehaviour 
{
    private Charctercontroller controllers ;
	private Animator animator ;
	private InputSystem_Actions InputAction ;
	
	
	[Headeer("Player Setting ");
	
	private float WalkSpeed =  10f;
	private float SprintSpeed  = 20f;
	private float JumpSpeed = 10f;
	
	
	
	private vector2  MoveInput ;
	void Awake()
	{
	  controllers =  GetComponent<Charctercontroller>();
	  InputAction =  new InputSystem_Actions();
	  InputAction.player.Move.performed =+ ctx => MoveInput = ctx.ReadValue<Vector2>();
	  InputAction.player.Move.cancelled += ctx => MoveInput =  Vector3.zero ;
	  
	  // jumping 
	  
	  InputAction.Player.Jump.performed =+ ctx => TryJump();
	  
	}

    private void OnEnable()
	{
		InputAction.Enabled();
	}
	
	private void OnDishable()
	{
		InputAction.Disable();
	}
    private void Update()
	{
	    GroundCheck();
		ApplyGravity();
		MovePlayer();
		TryJump();
	
	}
	
	//Move the Player 
	
	private void MovePlayer()
	{
		vector3 inputDir =  new Vector3(MoveInput.x , 0f , MoveInput.z).normalized ;
		
		if(inputDir.sqrtMagnitue > 0.01f)
		{
			vector3 camforwrd =  cam.forward ;
			vector3 camRight = cam.right ;
			
			camforwrd.y  = 0f;
			camRight.y = 0f ;
			
			camforwrd.Normalized();
			camRight.Normalized();
			
			vectore moveDir =   camforwrd + inputDir.z   * camRight + inputDir.x '
			
			bool isSprinting =  InputAction.player.Sprint.isPresed();
			
			float Speed =  isSprinting ?  SprintSpeed : WalkSpeed ;
			
			
			// rotation 
			float angle  =  Mathf.atan2(moveDir.x,moveDir.z) * Mathf.Deg2rad ;
			float smoothAngle = mathf.smoothDampAngle(transform.eulerAngle.y , angle , ref turnSmoothVelocity , rotationSmoothTime);
			transform.rotation  =  Quterion.Eulear(0f,smoothAngle,of);
			
			// Animtion Controller 
			if(isSprinting)
			{
				animator.setBool("Run" ,true);
				animator.setBool("Walk",false);
				
			}
			else{
				
				animator.setBool("Run" ,false);
				animator.setBool("Walk" ,true);
			}
			
		}
		else{
			animator.setBool("Walk",false);
			animator.setBool("Run" , false);
		}
		
	}
	
	private void OnfootStep()
	{
		
	}
	
	private void TryJump()
	{
	  if(isGrounded)
	  {
		   verticalVelocity = Mathf.Sqrt(jumSpeed * -2f * gravity );
		   animator.SetTrigger("jump");
	  }
		
	}
	
	// checking player is grounded or not 
	
	private bool isGrounded ;
	private float verticalVelocity  ;
	private Void GroundCheck()
	{
	   isGrounded  = controllers.isGrounded ;
	   if(isGrounded && verticalVelocity < 0f)
	   {
	          verticalVelocity =  -2f;
	   }
	}
	
	
	//Apply Gravithy 
	private float terminalVelocity =  50f ;
	private float gravity =  -9.8f;
	private void ApplyGravity()
	{
	   if(isGrounded && verticalVelocity < terminalVelocity )
	   {
	       verticalVelocity =  verticalVelocity + gravity* Time * time.deltatime ;
	   }
	   
	    controllers.Move(vertical3.up * verticalvelocity * time.deltatime ;
	}
}