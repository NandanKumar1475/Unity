

 //healthbar ui which is connect to the health bar game object 

public class HealthBarUi : MonoBehaviour
{
	private Image healthFlledImge ;
	private float smoothSpeed ;
	private int targetToFill ;

	 void update(){
		healthFlledImge.fillAmount = mathf.lerp(healthFlledImge.fillAmount,targetToFill,time.deltaTime * smoothSpeed);
		 
	 }
	 void updateHealth(int currentHealth,int maxHealth){
		 targetToFill = mathf.clamp0(currentHealth / maxHealth);
	 }
	
	
	
	// ------------------------------------------------------------//
	//playerhealth which is attached to the player //
	
	public class PlayerHealth : MonoBehaviour{
		 private int CurrentHealth;
		 private const int maxHealth  = 100;
		 private int health = 100 ;
		 
		 private int Health
		 {
			 
			 get => health ;
			 private set
			 {
				 health.mathf.clamp(value,0,maxHealth)
			 }
		 }
		 
		 private void start(){
			 health = Health ;
		 }
		 
		 private void takeDamage(int damageAmount){
			 
			 health = health - damageAmount ;
			 // die function called
		 }
		 
		 private void healPlayer(int healAmount){
			 health =  health +  healAmount ;
			 
		 }
		 
		 
		
	}
	
	
	//-----------------------------------------------------------//
	// Zombie give the Damage
	
	public class ZombieDamage : MonoBehaviour {
		private int damageAmount  =  25;
		
		void onColiisionEnter(Collision col){
			if(col.gameObject.comparetag(
		}
	}
	
	
	
