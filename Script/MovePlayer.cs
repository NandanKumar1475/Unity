using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponents(typeof("CharacterController")]

publc class PlayerMovement : Monobehaviour
{
	[Header("Movement Setting")]
	[serlizfiels] private float walkSpeed= 2f;
	[serlizfiels] private float sprintSpeed= 10f;
	[serlizfiels] private float smoothRotationTime = 1f ;
	
	[Header("Required for player")]
	private CharacterController controller;
	private Transform transformCamera;
	
	private vector2 Moveinput;
	
	private float smoothTurnVelocity;
	
	// --> the script which was created by the inutAction button
	private PlayerInput inputAction ; 
	
	void Awake()
	{
		controller = GetComponent<CharacterController>();
		// c Gets the main camera in the scene. Camera.main finds the camera with the tag “MainCamera”.
		transformCamera = Camera.main.transform ;
		
		inputAction = new PlayerInput();
		
		inputAction.Player.Move.performd += onMovePerformed ;
		inputAction.Player.Move.cancelled -= onMoveCancel;
		
	}
	
	//It represents the context (info) about an input event — like which key or button was pressed, what value was input (e.g., a float, Vector2), and when it happened.
	private void onEnable()
	{
		inputAction.Enable();
	}
	private void onDishable()
	{
		inputAction.Disable();
	}
	private void onMovePerformed(InputAction.callbackcontext btn)
	{
		Moveinput  = btn.ReadValue<vector2>();
	}
	private void onMoveCancel()
	{
		Moveinput = btn.ReadValue<vector2>();
	}
	
	private void Update()
	{
		MoveHorizontal();
	}
	
	public void MoveHorizontal()
	{
	   	vectore inputDirection = new vector3(Moveinput.x , 0f,Moveinput.y).normalized;
		// spped of the player
		
		float targetSpeed =  isSprinting ? sprintSpeed : walkSpeed ;
		if(Moveinput == vector2.zero)
		{
			targetSpeed = 0f;
		}
		
		float horizontalSpeed  = new vector3(controller.velocity.x 0f,controller.velocity.z).magnitude;
		
		if(Mathf.abs(horizontalSpeed - targetSpeed > 0.1f)
		{
			currentSpeed  = math.lerp(horizontalSpeed,targetSpeed.Time.deltatime*speedChangerate);
			
		}else
		{
			currentSpeed = targetSpeed ;
		}
		
		if(inputDirection.magnitude > 0.1f)
		{
			// getting camera direction ;
			vector3 camForward  = transformCamera.forward;
			vector3 camright =  transformCamera.right ;
			camForward.y = 0 ;
			camright.y =0 ;
			camForward.Normalize();
			camright.Normalize();
			moveDir = (camForward *inputDirection.z + camright*inputDirection.x).normalized;
			
			//rotation
			float targetAngle = mathf.Atan2(moveDir.x,moveDir.z) * Mathf.rad2Deg;
			float angle = mathf.smoothDamoAngle(transform.eulearAngle.y,targetAngle,ref turnsmoothVelocity,rotationSmoothtime);
		    transform.rotation = Quterian.Euler(0f,angle,0f);
			
			// final movement
			controller.Move(moveDir(currentSpeed*time.deltatime);
			
			
			
		}
	}
	
	
	
}