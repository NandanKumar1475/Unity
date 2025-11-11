// -- > selction Manager 

public class SelectionManager : MonoBehaviour 
{
   // key filed required 
   
   public static SelectionManager instance {get , private set};
   
   private Dictionary<string ,int> selectedElement = new Dictionary<string,int>();
   
   private Dictionary<string,Elementdata(a class) =  new Dictionary<string,Elementdata>();
   
   public Tmp_textmeshproGUI selectedElements ;
   
   public event Action onSelectionChanged;
   
   
   void Awake()
   {
	   if(instance == null){
		   instance =  this 
	   }else 
	   {
		   Destroy(gameObject);
	   }
		   
   }
}