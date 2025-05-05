using UnityEngine;

public class TraitCellVisual : CellVisual
{
    private Trait linkedTrait;

    public void Setup(Trait trait)
    {
        linkedTrait = trait;
    }
}
