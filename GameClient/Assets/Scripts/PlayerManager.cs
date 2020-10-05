using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public TextMesh usernameText;

    public float health;
    public float maxHealth = 100f;
    public int itemCount = 0;
    public MeshRenderer model;

    public Transform camTransform;

    public GameObject eye;

    public Transform specCam;
    public bool spectating;
    public int spectatingId;

    public Transform itemHolder;
    public List<Item> items = new List<Item>();
    public int currentItemIndex = -1;

    [HideInInspector] public Animator knifeAnimator;
    [HideInInspector] public Animator pistolAnimator;

    public int team;
    public int points;
    [HideInInspector]
    public int oldPoints;

    #region Interpolation
    //Interpolation
    [HideInInspector] public int delayTick;
    [HideInInspector] public float timeElapsed;
    [HideInInspector] public float timeToReachTarget;
    [HideInInspector] public int localTick;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;
    public bool interpolate = true;
    [HideInInspector] public List<TransformUpdate> futureUpdates = new List<TransformUpdate>();
    #endregion

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        usernameText.text = username;
        health = maxHealth;
        GameManager.spectatblePlayersIds.Add(id);

        InvokeRepeating("SendPing", 1, 5);
    }

    private void SendPing()
    {
        ClientSend.SendPing();
    }

    private void Start()
    {
        to = new TransformUpdate(localTick, transform.position);
        from = new TransformUpdate(delayTick, transform.position);
        previous = new TransformUpdate(delayTick, transform.position);
    }

    public void SetDesiredPosition(TransformUpdate position)
    {
        if(!interpolate || health <= 0)
        {
            transform.position = position.position;
        }
        
        if(health > 0)
        {
            futureUpdates.Add(position);
            delayTick = position.tick - 4;
        }
    }

    private void Update()
    {
        UpdateInterpoaltion();
    }

    public void CycleSpectator()
    {
        //find a better way later
        for (int i = 0; i < GameManager.spectatblePlayersIds.Count-1; i++)
        {
            if (GameManager.spectatblePlayersIds[i] == spectatingId)
            {
                if(GameManager.players[GameManager.spectatblePlayersIds[i]].health > 0)
                {
                    SetSpectating(GameManager.spectatblePlayersIds[i + 1]);
                    break;
                }
                continue;
            }
            else
            {
                for (int k = GameManager.spectatblePlayersIds.Count-1; k > 0; k--)
                {
                    if (GameManager.spectatblePlayersIds[k] == spectatingId)
                    {
                        if (GameManager.players[GameManager.spectatblePlayersIds[k]].health > 0)
                        {
                            SetSpectating(GameManager.spectatblePlayersIds[k - 1]);
                            break;
                        }
                    }
                    continue;
                }
            }
        }
    }

    private void UpdateInterpoaltion()
    {
        float t = 0;
        t = Time.time * 40;
        localTick = (int)t;

        for (int i = 0; i < futureUpdates.Count; i++)
        {
            if (localTick >= futureUpdates[i].tick)
            {
                previous = to;
                to = futureUpdates[i];
                from = new TransformUpdate(delayTick, transform.position);
                futureUpdates.RemoveAt(i);
                timeElapsed = 0;
                timeToReachTarget = (to.tick - from.tick) * 0.025f;
            }
        }

        timeElapsed += Time.deltaTime;
        Interpolate(timeElapsed / timeToReachTarget);
    }

    private void Interpolate(float _lerpAmount)
    {
        if (to.position == previous.position)
        {
            // If this object isn't supposed to be moving, we don't want to interpolate and potentially extrapolate
            if (to.position != from.position)
            {
                // If this object hasn't reached it's intended position
                transform.position = Vector3.Lerp(from.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount clamped so no extrapolation occurs
            }
            return;
        }
        transform.position = Vector3.LerpUnclamped(from.position, to.position, _lerpAmount); // Interpolate with the _lerpAmount unclamped so it can extrapolate
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            //interpolate = false;
            Die();
        }
    }

    public void DrawItem(string _name, bool _drawn)
    {
        foreach(Item _item in items)
        {
            if(_item.info.name == _name)
            {
                _item.SetDraw(_drawn);
            }
        }
    }

    public void AddItem(GameObject _itemObj)
    {
        GameObject go = Instantiate(_itemObj, itemHolder, false);
        go.GetComponent<Item>().itemGameObject.SetActive(false);
        items.Add(go.GetComponent<Item>());
    }
    public void AddItem(GameObject _itemObj, bool _equipped)
    {
        GameObject go = Instantiate(_itemObj, itemHolder, false);
        go.GetComponent<Item>().itemGameObject.SetActive(false);
        items.Add(go.GetComponent<Item>());
        if(_equipped)
        {
            EquipItem(_itemObj.GetComponent<Item>().info.name, true);
        }
    }

    public void EquipItem(string _name, bool _equipped)
    {
        foreach(Item _item in items)
        {
            if(_item.info.name == _name)
            {
                if(currentItemIndex != -1)
                {
                    if(_equipped)
                    {
                        items[currentItemIndex].SetEquipped(false);
                        items[currentItemIndex].SetDraw(false);
                    }
                }
                _item.SetEquipped(_equipped);
                currentItemIndex = items.IndexOf(_item);
                break;
            }
        }
    }

    public void HandleKill(int _points, int _killedTeam)
    {
        oldPoints = points;
        points = _points;

        int _tempPoints = points - oldPoints;

        //Points are being stored client and server side, but we may have to rethink how it works, so we're not showing it on screen right now
        UIManager.instance.StartCoroutine(UIManager.instance.Kill(team, _killedTeam, _tempPoints));
    }

    public void ClearWeapons()
    {
        foreach (Item _item in items)
        {
            Destroy(_item.gameObject);
        }
        currentItemIndex = -1;
        items.Clear();
    }

    public void Die()
    {
        model.enabled = false;
        ClearWeapons();

        if (eye != null)
        {
            eye.SetActive(false);
        }
    }

    public void Spectate(bool _spectating)
    {
        if (_spectating)
        {
            foreach (var _player in GameManager.players)
            {
                if (_player.Value.id != Client.instance.myId && _player.Value.health > 0)
                {
                    GetComponentInChildren<Camera>().enabled = false;
                    _player.Value.specCam.GetComponent<Camera>().enabled = true;
                    spectatingId = _player.Value.id;
                    spectating = true;
                    UIManager.instance.HandleSpectating(_player.Value.username);
                    break;
                }
            }
        }
        else
        {
            GameManager.players[spectatingId].specCam.GetComponent<Camera>().enabled = false;
            GetComponentInChildren<Camera>().enabled = true;
            spectating = false;
        }
    }

    public void SetSpectating(int _id)
    {
        GameManager.players[spectatingId].specCam.GetComponent<Camera>().enabled = false;
        GameManager.players[_id].specCam.GetComponent<Camera>().enabled = true;
        UIManager.instance.HandleSpectating(GameManager.players[_id].username);
        spectatingId = _id;
    }

    public void Respawn()
    {
        model.enabled = true;
        ClearWeapons();

        if (eye != null)
        {
            eye.SetActive(true);
        }
        SetHealth(maxHealth);
    }
}
