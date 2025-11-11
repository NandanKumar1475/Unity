public class BulletPooling : MonoBehaviour
{
	[serlizfiled] private GameObject prefab ;
	[serlizfiled] private int intitalSize =  10 ;
	
	private List<GameObject> pool ;
	
    private void Awake()
	{
		pool = new List<GameObject>(intitalSize);
		
		for(int i = 0 ; i < intitalSize;i++)
		{
			GameObject bullets =  instantiate(prefab,transform);
			bullets.setActive(false);
			pool.Add(bullets);
		}
	}
	
	//take bullets from pool
	
	public GameObject GetBullet()
	{
		foreach(GameObject bullet in pool)
		{
			if(!bullet.isActiveinHierarchy)
			{
				bullet.setActive(true);
				return bullet ;
			}
			
		}
		
		return null ;
	}
	
	// return to the pool 
	
	public GameObject ReturnBullet(GameObject bullet)
	{
		bullet.setActive(false);
		bullet.transform.SetParent(transform);
	}
}