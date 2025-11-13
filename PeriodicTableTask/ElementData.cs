// ElementData.cs
using System;

[Serializable]
public class ElementData
{
    public string symbol;
    public string name;
    public int atomicNumber;
    public int electrons;           
    public int neutrons;            
    public int[] electronShells;    
    public int valenceElectrons;   
    public float electronegativity; 
    public string uiColorHex;      
    public string description;
}

[Serializable]
public class ElementDatabase
{
    public ElementData[] elements;
}
