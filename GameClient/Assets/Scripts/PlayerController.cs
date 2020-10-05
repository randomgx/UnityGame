using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;

    private PlayerManager playerManager;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        //For now, we're full trusting the client.
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(playerManager.items.Count > 0 && playerManager.health > 0 && UIManager.instance.gameState == 2)
            {
                if(playerManager.items[playerManager.currentItemIndex] != null)
                {
                    //do animation here
                    if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit _hit, playerManager.items[playerManager.currentItemIndex].info.range))
                    {
                        if (_hit.transform.parent.CompareTag("Player"))
                        {
                            ClientSend.PlayerShootClient(_hit.collider.GetComponentInParent<PlayerManager>());
                        }
                    }
                    //ClientSend.PlayerShoot(camTransform.forward);
                }
            }
            else if(playerManager.health <= 0)
            {
                playerManager.CycleSpectator();
            }
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ClientSend.PlayerDrawItem(false);
        }

        if (UIManager.instance.gameState != 2)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerDrawItem(true);
        }
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        ClientSend.PlayerMovement(_inputs);
    }
}
