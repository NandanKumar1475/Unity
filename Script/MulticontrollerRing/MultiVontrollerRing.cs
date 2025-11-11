
// Ring Entry  means Ring properties and electrons GameObjects 
public class RingEntry 
{
   public ParticleSystem particlesystem ;
   public List<Transfrom> Electrons  = new List<Electrons>();
   public float speedMultiplier = 1f;   //-> how must Electron move in A particlesystem
   
   
   public List<float> baseAngle =  new List<float>();
   public int lastChildCount ;
}

// find all the Rings in the Gameobject

public class RingDiscovery : MonoBehaviour 
{
	public bool includeInactiveparticleSystems =  true ;
	public bool evenSpacing =  false ;
	
	public randomStartAngle = false ;
	
	public List<RingEntry> rings  =  new List<RingEntry>();
	
	
	public void RefreshAllRings()
	{
		rings.clear() ; // clear all old data 
		
		var  allParticleSystemRings = GetComponentInchildren<ParticleSystem>(includeInActive : IncludeInactiveparticleSystem);
		
		for(var ps in allParticleSystemRings)
		{
			RingEntry entry = new RingEntry();
			entry.particlesystem = ps ;
			entry.electrons = new List<Transfrom>();
			{
			
			
			forEach(Transfrom child in ps.transfrom)
			{
				if(child == null) continue ;
				if(child.GetComponent<particlesystem>() ! = null ) continue ;
				
				if(! child .gameObject.activeInHirearchy  && includeInactiveparticleSystems ) continue ;
				entry.electrons.Add(child);
			}
			int count = entry.electrons.Count;
				if (count > 0)
				{
					if (evenSpacing)
					{
						float step = 360f / count;
						for (int i = 0; i < count; i++)
							entry.baseAngles.Add(i * step);
					}
					else if (randomStartAngle)
					{
						for (int i = 0; i < count; i++)
							entry.baseAngles.Add(Random.Range(0f, 360f));
					}
					else
					{
						for (int i = 0; i < count; i++)
							entry.baseAngles.Add(0f);
					}
				}

							entry.lastChildCount = count;
							rings.Add(entry);
		}
}
}


// find the Radius of rings 

var 

