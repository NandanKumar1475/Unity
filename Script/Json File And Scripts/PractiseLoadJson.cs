//json objects 

--> Data -->

{
	elements:[
	{
	       "Symnol":H,
		   "Name" :Nandan,
		   "electros":1
		   "protons" :  5 ;
	}
	
	]
	
}

---> Step 1 Create Elemet Data -->

public class Element_data 
{
	public String Symbol;
	public int electrons;
	public int protons ;
	public string namec;
	
	
}

[Serliabable]
public class Element_DataBase
{
	public Element_data[] elementdatabase;
}


// Element to Show in the Ui so below Are the code Which Handle the Objects 

public class DataShowInUi :  MonoBehaviour 
{
	
	public TMp_text name ;
	public Tmp_Text Symbol ;
	public TMP_text AtomicMass ;
	
	public Image bgimage ;
	
	public Button TIleButton ;
	
	
	Element_data ed;
	void Setdata(Element_data e)
	{
		ed = e ;
		
		if(name!= null ) {
			name.text = e.name;
		}
		if(Symbol !=null)
		{
			Symbol.text = e.Symbol ;
		}
		if (AtomicMass != null )
		{
			AtomicMass.text =
		}
		
		if(bgimage != null || !string.isEmptyOrNull(bgimage){
			if(color.utlity.TryParseHtmlSteing(e.colorhex , out var c)
			{
				bgimage.Color =  c ;
			}
			
		}
		
		if(TileButton !=null)
		{
			tileButton.onCLick.RemoveAllEventListener();
			tileButton.onCLick.AddEventListenr(onTileClicked());
		}
		
		public void onTileClicked()
		{
			if(JsonDataHandler.instance != null)
			{
				JsonDataHandler.instance.ShowDetails(ed);
			}
			
		}
		
	}
	
}

--> Step 2 create c# script file which load the data json and convert into c# object 

public class JsonDataHandler : MonoBehaviour 
{
	
	void Awake()
	{
		
	}
}