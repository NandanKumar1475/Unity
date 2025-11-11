// Selection Manager -->  How to select data and Transfer the Elements in another scene

1.Script --> to keep the data Global

public static class SelectionDataTransfer : MonoBehaviour
{
  // theElemets chosen in the periodic table can be store here
  
  public static List<ElementData> selectedElement =  new List<ElementData>();
  
  public static void clear()
  {
     selectedElement.clear();
  }
}
