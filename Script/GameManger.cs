//  how to store the data and manages the datat in unity 

public class GameManager : MonoBehaviour{
  public static GameManager Instance ;
  
   public  int CoinCollected = 0 ;
   Public int  PlayerHealth = 100;
   void Awake(){
	   if(Instance == null){
	   Instance = this;
       }
	  else {
	   Destroy(GameObject);
      }
   }
   
   void CoinCount(){
	   CoinCollected++;
   }
   
   void PlayerDamage(int DamageAmount){
	   Health -= DamageAmount ;
   }
   
   void Medkit(int HealAmount){
	   Health += HealAmount ;
   }
   
   

}




// ------------------------------
 //Coin Script
 
 public class CoinScript : MonoBehaviour {
	
	private void onCollisionEnter(Collision col){
		 
		 if(col.gameObject.ComapareTag("Player"){
			 
			 GameManager.Instance.CoinCount();
			  playAudio();
			  Destroy(GameObject);

		 }			 
	}
	 
 }
 
 
 // ----------------------------------------
 //Zombie Script
 
 public class DamageScript : MonoBehaviour {
	  int damageAmopunt =  10;
	  private void OnCollisinEnter(Collision col){
		  if(col.gameObject.comapareTag("Player){
			   animation.SetTriggered("Attack");
			   GameManager.Instance.PlayerDamage(damageAmount);   
		  }
	  }
	 
 }
 
 
 /--------------------------------------------------------------------------------/
 UI Script 
 
 Public class UIManager : MonoBehaviour {
	 public textMeshpro coinCollected ;
	 public textMeshpro  playerHealth ;
	 
	 void Update(){
		 coinCollected.text  = "coins" + GameManager.Instance.coinCollected ;
		 playerHealth.text  =  "Health" + GameManager.Instance.playerHealth ;
	 }
 }
 