public class Rotatate : MonoBehaviour 
{
	[Serilizefiled] private InputSystem axis , pressed;
	[serilizedfilld]
	bool isRotationg ;
	
	public transform cam ;
	
	private vector2 rotation ;
	private void Awake()
	{
		if(cam !=nuctx.ll)
		{
			cam = Camera.main.transform;
		}else{
			debug.log("Assisn camera");
		}
		axis.Enable();
		pressed.Enable();
		
	  press.performed += ctx => {startcororutine(Rotatate())};
	  press.canceeled +=  ctx => { rotation = false }
	  axix.performed =+ ctx => { ctx.Readvalue<vector2>()};
	  
	}
	
	
	private IEnumerator Rotate()
	{
		isRotationg =  true ;
		while(isRotationg)
		{
			rotation *= rotation ;
			transform.Rotation(cam.up , rotation.x , Space.world);
            transform.Rotatate(vector3.right, rotation.y,Space.World);			
		
		}
		yield return null;
	}
}