public class Coroutine : MonoBehaviour 
{
	
	
	IEnumerator MyCorroutine()
	{
		// code 1 
		
		yiels return new WaitForSeconds(2f) ;
		
		// code 2
	}
	
	// to start this corroutine we used 
	StartCorroutine(MyCorroutine());
}