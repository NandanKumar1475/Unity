public class Bullet : MonoBehaviour 
{
	public float lifetime = 3f ;
	private float timer 
	private Rigidbody rb ;
	
	void Awake()
	{
		rb.GetComponent<Rigidbody>();
		
	}
	
	void onEnable()
	{
		timer = lifetime ;
		if(rb ! =null)
		{
			rb.velocity = vector3.zero ;
		}
	}
	
   void onCollisionEnter()
   {
	   
   }
   
   
   void ReturntoPool()
   {
	   if( BulletPolling.poll ! = null)
	   {
		   bulletpolling.Return(gameobject)
	   }else
	   {
		   gameobject.setActive(false);
	   }
   }
	
}