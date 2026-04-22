using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReciver : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("부모 연결")]
    public PlayerController playerController;

    void EndRoll()
    {
        if (playerController != null)
        {
            playerController.EndRoll();
        }
    }
}
