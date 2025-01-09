using UnityEngine;

public class FPS_limit : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 120;
    }
}
