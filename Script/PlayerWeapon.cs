public class PlayerWeapon : MonoBehaviour
{
	bool isFiring = false;
	// [Serlialized] GameObject emisionModule;
	[Serlialized] GameObject[] lasers;
	private void Update()
	{
		 ProceesingFire();
		
	}
	public void OnFire(InputValue  value)
	{
		 isFiring  = value.isPressed ;
		 
	}
	
	void ProceesingFire()
	{
		if(isFiring)
		{
			//var emisionModule = laser.GetComponent<PartcleSystem>();
			//emisionModule.enabled = true;
			
			// for array of bullet 
			for(GameObject laser in lasers)
			{
				var bullet = laser.GetComponent<PartcleSystem>();
				bullet.enabled =  true;
			}
			Debug.log("firing");   
		}else
		{
			emisionModule.enabled = false ;
		}
		
	}
	
}