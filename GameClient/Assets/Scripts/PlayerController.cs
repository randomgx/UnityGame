using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform camTransform;
    public float range;

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
            playerManager.knifeAnimator.SetTrigger("attack");
            playerManager.pistolAnimator.SetTrigger("attack");
            if (Physics.Raycast(camTransform.position, camTransform.forward, out RaycastHit _hit, range))
            {
                if(_hit.transform.parent.CompareTag("Player"))
                {
                    ClientSend.PlayerShootClient(_hit.collider.GetComponentInParent<PlayerManager>());
                }
            }
            //ClientSend.PlayerShoot(camTransform.forward);
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ClientSend.PlayerDrawWeapon(0);
            if (playerManager.carryingWeapon || !playerManager.carryingWeapon)
            {
                if (playerManager.team == 0 || playerManager.team == 2)
                {
                    playerManager.DrawWeapon(0, 0);
                }
                else
                {
                    playerManager.DrawWeapon(0, 1);
                }
            }
        }

        if (UIManager.instance.gameState != 2)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ClientSend.PlayerDrawWeapon(1);
            if(playerManager.carryingWeapon)
            {
                if (playerManager.team == 0 || playerManager.team == 2)
                {
                    playerManager.DrawWeapon(1, 0);
                }
                else
                {
                    playerManager.DrawWeapon(1, 1);
                }
            }
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
