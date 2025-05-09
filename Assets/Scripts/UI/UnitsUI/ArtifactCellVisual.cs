using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArtifactCellVisual : CellVisual, IPointerClickHandler
{
    public Artifact linkedArtifact;
    private delegate void OnButtonClick();
    private OnButtonClick onButtonClick;

    public void Setup(Artifact artifact)
    {
        linkedArtifact = artifact;
    }

    public void Setup(Artifact artifact, PlayerCharacterUI playerCharacterUI, bool isSlot = false)
    {
        linkedArtifact = artifact;
        onButtonClick = () => 
        { 
            if(isSlot)
                playerCharacterUI.OnArtifactSwitch(this);
            else
                playerCharacterUI.OnArtifactSlotSelection(this); 
        };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onButtonClick();
    }
}
