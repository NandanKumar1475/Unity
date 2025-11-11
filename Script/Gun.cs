public class Gun : MonoBehaviour 
{
	public BulletPool bulletbool ;
	public transform firePoint ;
	public float bulletSpeed = 20f;
	
	void Update()
	{
		if(Input.GetkeyDown(Keycode.Space))
		{
			
			GameObject bullet = bulletbool.Get();
			if(bullet == null )
			{
				debug.Log("Null");
				return ;
			}
			
			bullet.transform.poition =  firePoint.position;
			bullet.transform,rotation = firePoint.rotatoion ;
			
			Rigidbody rb = bullet.GetComponent<Rigidbody>();
			if(rb != null)
			{
				rb.lineravelocity =  firePoint.forWard * bulletSpeed ;
				
			}
		}
	
}