using System.Collections.Generic;

public static class SelectionDataTransfer
{
    public static ElementData elementA = null;
    public static ElementData elementB = null;

    public static List<ElementData> selectedElements = null;

    public static TransferResult lastAnalysis = null;
    public static List<TransferResult> lastTransfers = null;

    public static void Clear()
    {
        elementA = null;
        elementB = null;
        selectedElements = null;
        lastAnalysis = null;
        lastTransfers = null;
    }

    public static void EnsureLists()
    {
        if (selectedElements == null) selectedElements = new List<ElementData>();
        if (lastTransfers == null) lastTransfers = new List<TransferResult>();
    }
}
