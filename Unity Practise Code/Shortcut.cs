public int GetCount(ElementData e) => e == null ? 0 : GetCount(e.symnbol);
// this is same as 

public int GetCount(ElementData e)
{
  if(e == null)
  {
    return 0 ;
  }

  return GetCount(e.symbol);
}
