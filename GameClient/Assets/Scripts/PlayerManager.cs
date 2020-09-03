using System.Collections;
using System.Collections.Generic;
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

    public GameObject knife;
    public GameObject pistol;

    public Animator knifeAnimator;
    public Animator pistolAnimator;

    private GameObject activeWeapon;

    public int team;
    public bool carryingWeapon;

    #region Interpolation
    //Interpolation
    public int delayTick;
    public float timeElapsed;
    public float timeToReachTarget;
    private int localTick;
    private TransformUpdate to;
    private TransformUpdate from;
    private TransformUpdate previous;
    public bool interpolate = true;
    public List<TransformUpdate> futureUpdates = new List<TransformUpdate>();
    #endregion

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        usernameText.text = username;
        health = maxHealth;
    }

    private void Start()
    {
        to = new TransformUpdate(localTick, transform.position);
        from = new TransformUpdate(delayTick, transform.position);
        previous = new TransformUpdate(delayTick, transform.position);
    }

    public void SetDesiredPosition(TransformUpdate position)
    {
        if(!interpolate)
        {
            transform.position = position.position;
            return;
        }
        
        futureUpdates.Add(position);
        delayTick = position.tick - 4;
    }

    private void Update()
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
            Die();
        }
    }

    public void DrawWeapon(int _showing, int _weapon)
    {
        switch (_showing)
        {
            case 0:
                if (_weapon == 0)
                {
                    pistol.SetActive(false);
                    //pistolAnimator.SetBool("draw", false);
                }
                else if(_weapon == 1)
                {
                    knife.SetActive(false);
                    knifeAnimator.SetBool("draw", false);
                }
                break;
            case 1:
                if (_weapon == 0)
                {
                    pistol.SetActive(true);
                    //pistolAnimator.SetBool("draw", true);
                }
                else if (_weapon == 1)
                {
                    knife.SetActive(true);
                    knifeAnimator.SetBool("draw", true);
                }
                break;
        }
    }

    public void GetWeapon()
    {
        carryingWeapon = true;
        if(team == 1)
        {
            activeWeapon = knife;
        }
        else
        {
            activeWeapon = pistol;
        }
    }

    public void Die()
    {
        model.enabled = false;
        carryingWeapon = false;
        if(activeWeapon != null)
            activeWeapon.SetActive(false);
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }
}
