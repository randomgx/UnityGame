using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;
    public float throwForce = 600f;
    public float health;
    public float maxHealth = 100f;

    private bool[] inputs;
    private float yVelocity = 0;

    public int itemAmount = 0;
    public int maxItemAmount = 3;

    [HideInInspector] public Quaternion camRotation;

    [SerializeField] bool spectating;

    #region Points
    public int points;

    private int killMurderer;
    private int killDetective;
    private int killBystander;
    private int wrongKill;
    #endregion

    #region Items
    public List<Item> items = new List<Item>();
    public int currentItemIndex = -1;
    #endregion

    #region Teams
    public enum Team { Bystander, Murderer, Detective}
    public Team team;
    #endregion

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        killMurderer = RoundManager.instance.killMurdererPoints;
        killDetective = RoundManager.instance.killDetectivePoints;
        killBystander = RoundManager.instance.killBystanderPoints;
        wrongKill = RoundManager.instance.wrongKillPoints;

        if (RoundManager.instance.GetPlayerCount() >= RoundManager.instance.minimumPlayers && RoundManager.instance.rState != RoundManager.RoundState.COUNTDOWN)
        {
            RoundManager.instance.OnChangeRoundState(RoundManager.RoundState.COUNTDOWN);
        }

        if(RoundManager.instance.rState == RoundManager.RoundState.COUNTDOWN)
        {
            ServerSend.RoundCountdown(id, RoundManager.instance.countdownCurrentTimer);
        }

        ServerSend.RoundState((int)RoundManager.instance.rState);

        inputs = new bool[5];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    /// <summary>Calculates the player's desired movement direction and moves him.</summary>
    /// <param name="_inputDirection"></param>
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        ServerSend.PlayerRotation(this);
        ServerSend.PlayerPosition(this); //send only moving players later
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void ShootClient(Player _player)
    {
        if (health <= 0f || RoundManager.instance.rState != RoundManager.RoundState.PLAYING || !items[currentItemIndex].IsEquipped() || !items[currentItemIndex].IsDrawn())
        {
            return;
        }

        _player.TakeDamage(items[currentItemIndex].info.damage);

        if (_player.team == Team.Bystander && team == Team.Detective)
        {
            points -= wrongKill;
            ServerSend.HandleKill(this, (int)Team.Bystander);
        }
        //If murderer and kill a murderer --
        else if (_player.team == Team.Murderer && team == Team.Murderer)
        {
            points -= wrongKill;
            ServerSend.HandleKill(this, (int)Team.Murderer);
        }
        //if detective kill a murderer +++
        else if (_player.team == Team.Murderer && team == Team.Detective)
        {
            points += killMurderer;
            ServerSend.HandleKill(this, (int)Team.Murderer);
        }
        //if murderer kill a bystander ++
        else if (_player.team == Team.Bystander && team == Team.Murderer)
        {
            points += killBystander;
            ServerSend.HandleKill(this, (int)Team.Bystander);
        }
        //if murderer kill a detective +++
        else if (_player.team == Team.Detective && team == Team.Murderer)
        {
            points += killDetective;
            ServerSend.HandleKill(this, (int)Team.Detective);
        }
        //if detective kill a detective ---
        else if (_player.team == Team.Detective && team == Team.Detective)
        {
            points -= wrongKill;
            ServerSend.HandleKill(this, (int)Team.Detective);
        }
    }

    public void Shoot(Vector3 _viewDirection)
    {
        if (health <= 0f || RoundManager.instance.rState != RoundManager.RoundState.PLAYING || !items[currentItemIndex].IsEquipped() || !items[currentItemIndex].IsDrawn())
        {
            return;
        }

        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 10))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(100f);
                
                //If detective and kill a bystander --
                if (_hit.collider.GetComponent<Player>().team == Team.Bystander && team == Team.Detective)
                {
                    points -= wrongKill;
                    ServerSend.HandleKill(this, (int)Team.Bystander);
                }
                //If murderer and kill a murderer --
                else if (_hit.collider.GetComponent<Player>().team == Team.Murderer && team == Team.Murderer)
                {
                    points -= wrongKill;
                    ServerSend.HandleKill(this, (int)Team.Murderer);
                }
                //if detective kill a murderer +++
                else if (_hit.collider.GetComponent<Player>().team == Team.Murderer && team == Team.Detective)
                {
                    points += killMurderer;
                    ServerSend.HandleKill(this, (int)Team.Murderer);
                }
                //if murderer kill a bystander ++
                else if (_hit.collider.GetComponent<Player>().team == Team.Bystander && team == Team.Murderer)
                {
                    points += killBystander;
                    ServerSend.HandleKill(this, (int)Team.Bystander);
                }
                //if murderer kill a detective +++
                else if (_hit.collider.GetComponent<Player>().team == Team.Detective && team == Team.Murderer)
                {
                    points += killDetective;
                    ServerSend.HandleKill(this, (int)Team.Detective);
                }
                //if detective kill a detective ---
                else if (_hit.collider.GetComponent<Player>().team == Team.Detective && team == Team.Detective)
                {
                    points -= wrongKill;
                    ServerSend.HandleKill(this, (int)Team.Detective);
                }
            }
        }
    }

    public void Equip(int _index)
    {
        if(currentItemIndex == -1)
        {
            currentItemIndex = _index;
            items[_index].SetEquipped(id, true);
        }
        else
        {
            items[currentItemIndex].SetEquipped(id, false);
            items[_index].SetEquipped(id, true);
        }
    }

    public void DrawItem(bool _draw)
    {
        if(health <= 0 || currentItemIndex == -1)
        {
            return;
        }

        items[currentItemIndex].SetDraw(id, _draw);
    }

    public void AddItem(Item _item)
    {
        items.Add(_item);
    }

    public void AddItem(Item _item, bool _active)
    {
        items.Add(_item);
        if (_active)
        {
            Equip(items.Count - 1);
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            items.Clear();
            currentItemIndex = -1;
            controller.enabled = false;
            transform.position = new Vector3(0f, 2f, 0f);
            ServerSend.PlayerPosition(this);

            ServerSend.PlayerSetSpectate(id, true);
            spectating = true;

            if (team == Team.Murderer)
            {
                RoundManager.instance.murderersAlive--;
                if(RoundManager.instance.murderersAlive <= 0)
                {
                    RoundManager.instance.EndRound(Team.Bystander);
                }
            }
            else if(team == Team.Bystander)
            {
                RoundManager.instance.bystandersAlive--;
                if (RoundManager.instance.bystandersAlive <= 0)
                {
                    RoundManager.instance.EndRound(Team.Murderer);
                }
            }
        }
        ServerSend.PlayerHealth(this);
    }

    public void DropWeapon()
    {
        //carryingWeapon = false;
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(0.5f);

        health = maxHealth;
        controller.enabled = true;
        spectating = false;
        ServerSend.PlayerSetSpectate(id, false);
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }
}
