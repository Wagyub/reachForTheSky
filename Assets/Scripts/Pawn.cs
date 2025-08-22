using UnityEngine;

public class Pawn : MonoBehaviour
{
    public void PlaceSlab(SlabPlacer placer)
    {
        placer.SetActivePawn(this);  // informe le placer du pawn actif
        placer.TryPlaceSlab();
    }
}
