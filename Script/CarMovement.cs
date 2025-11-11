// car Movement 
RequireComponent[TypeOf("WheelCollider")] ;
public class CarControllers : Monobehaviour 
{
	[Header ("car Setting ")]
	 private float motorForce= 1500f ;
	 private float brakeTorque = 3000f;
	 private float MaxsteeringAngle = 30f;
	// WheelCollider
   public WheelCollider frontLeftWheelCollder ;
   public WheelCollider frontRightWheelVCollder ;
   public WheelCollider rearLeftWheelCollider ;
   public WheelCollider reafRightWheelCollider ;
   
   
   // whell transfrom ;
   
   public Transfrom frontLeftWheel ;
   public Transfrom frontRightWheel ;
   public Transfrom reafRightWheel ;
   public Transfrom rearLeftWheel;
   
   // important variables 
   bool isBraking  ;
   
   
  private void Update()
  {
	  GetInput();
	  HandleSteering();
	  HandleMotor();
	  
  }
  
  // Getting input 
  private float horizonatlInput ;
  private  float verticalInput ;
  private Void GetInput()
  {
	  horizonatlInput = Input.GetAxix("Horizontal");
	  verticalInput  =  Input.GetAxix("Vretical");
	  isBraking = Input.GetKey(keycode.space)"
  }
  
  // handling Sterring 
  private float steeringAngle ;
  private void HandleSteering()
  {
	 steeringAngle = MaxsteeringAngle * horizonatlInput ;
	 frontLeftWheelCollder.steerAngle =  steeringAngle ;
	 frontRightWheelVCollder.steerAngle = steerAngle ;
  }
  
   // Handling Motors
   private float currentbrake ;
   private void HandleMotor()
   {
	   frontLeftWheelCollder.motorTorque = verticalInput * motorForce;
	   frontRightWheelVCollder.motorTorque = verticalInput * motorForce ;
	   
	   currentbrake.brakeTorque = isBraking ? brakeforce : 0f;
	   ApplyBraking();
	   
   }
   
   private void ApplyBraking()
   {
	   frontLeftWheelCollder.brakeTorque = currentbrake ;
	   frontRightWheelVCollder.brakeTorque = currentbrake  ;
	   reafRightWheelCollider.brakeTorque = currentbrake ;
	   rearLeftWheelCollider.brakeTorque = currentbrake ;
   }
   
   private void UpdateWheel()
   {
	   UpdateWheelColliderPose(frontLeftWheelCollder , frontLeftWheel);
	   UpdateWheelColliderPose(frontRightWheeCollder ,frontRightWheel);
	   UpdateWheelColliderPose(rearLeftWheelCollider ,rearLeftWheel);
	   UpdateWheelColliderPose(reafRightWheelCollider.reafRightWheel);
   }
   private void UpdateWheelColliderPose(WheelCollider collider , Transfrom transfrom)
   {
	   vector3 pos ;
       Quternion Quat ;
	   
	   collider.getWorldPos (out pos ,out Quat);
	   transfrom.position = pos ;
	   transfrom.rotation = ros ;
   }
   

}