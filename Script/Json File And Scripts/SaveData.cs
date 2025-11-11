public class SaveData : MonoBehaviour
{
	[Serilizable]
	public class PlayersSaveData
	{
		public String name;
		public int playerHealth;
		public int NumberOfKills ;
		public int Level ;
	}
	
	public intputField name;
	public Text_meshpro playerHealth ;
	public Text_meshpro NumberOfKills ;
	public Text_meshpro Level;
	
	PlayersSaveData playerdatas =  new PlayerSaveData();
	
	String fileName = "saveplayerData" ;
	
	private void Awake()
	{
		LoadData();
	}
	
	public void onApplicationQuit()
	{
		Save();
	}
	
	private  void update()
	{
		name.text = playerdatas.name.ToString();
		playerHealth.text = playerdatas.playerHealth.ToString();
		NumberOfKills.text = playerdatas.NumberOfKills.toString();
		Level.text = playerdatas.Level.toString();

    }
	
	#regin save and  LoadData
	
	public void Save()
	{
		string jsonData =  jsonutility.ToJson(playerdatas,true);
		string filepth =  path.combine(Application.persistentDatapath,jsonData);
		path.WriteAllText(path,jsonData);
	}
	
	public void LoadData()
	{
		string path = path.combine(Application.persistentDatapath,jsonData);
		if(file.exit)
		{
			String json =  file.ReadAllText(json);
			PlayerSaveData data = jsonutility.fromJson<>(json)
		}
	}
	#endRegin 

	
}