// formatio of circle 

public clss FormCircle : MonOBehaviour
{
    [serlizfiled] private Transfrom centre  ;
	
	void upadte()
	{
	  spawanCircle();
	}
	
	private void SpawanCirle()
	{
	      float angle =  i *(360 / NumberOfObject) * Mathf.Rad2deg ;
		  
		  float x = centre.x + Mathf.cos(angle) * radious;
		  flaot z  = centre.z + Mathf.sin(angle) * radious ;
		  
		  vector3 SpawnPoint = new vector3(z,0f,z);
		  
		  instiate(Prefabs,spawanPoint,Quterian,identity);
	}

}