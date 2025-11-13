using UnityEngine;

public class ElectronMarker : MonoBehaviour
{
    [Tooltip("Symbol of the element this electron is associated with (e.g. 'Na')")]
    public string elementSymbol;

    [Tooltip("Shell index (0 = innermost). Used for donor selection/preference.")]
    public int shellIndex = 0;

    [Tooltip("Current orbital angle in degrees. Controllers may set/read this.")]
    public float angleDeg = 0f;

    [Tooltip("Logical flag: has this electron already been transferred/consumed?")]
    public bool transferred = false;

    [Tooltip("Temporary flag: is this electron currently animating a transfer? Controllers skip it while true.")]
    public bool inTransfer = false;

 
    public void SetTransferState(bool isTransferred, bool isInTransfer)
    {
        transferred = isTransferred;
        inTransfer = isInTransfer;
    }

}
