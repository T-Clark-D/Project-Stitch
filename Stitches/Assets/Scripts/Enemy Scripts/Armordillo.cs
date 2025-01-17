using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armordillo : MonoBehaviour
{
    public AudioClip[] m_earthLoopSounds;
    public AudioSource m_earthLoopAudioSource;
    public AudioClip[] m_rollLoopSounds;
    public AudioSource m_rollLoopAudioSource;
    public AudioClip[] m_bounceSounds;
    public AudioSource m_bounceAudioSource;
    public AudioClip[] m_hurtSounds;
    public AudioSource m_hurtAudioSource;
    public AudioClip[] m_gruntSounds;
    public AudioSource m_gruntAudioSource;
    public AudioClip[] m_breathingLoopSounds;
    public AudioClip[] m_deathSounds;

    private Rigidbody2D m_RB;
    [SerializeField] private GameObject m_topPlatform;
    [SerializeField] private GameObject m_leftPlatform;
    [SerializeField] private GameObject m_rightPlatform;

    [SerializeField] private GameObject m_bottomBouncePad;
    [SerializeField] private GameObject m_topLeftBouncePad;
    [SerializeField] private GameObject m_topRightBouncePad;

    [SerializeField] private GameObject m_bottomStunLock;
    [SerializeField] private GameObject m_topLeftStunLock;
    [SerializeField] private GameObject m_topRightStunLock;

    [SerializeField] private CapsuleCollider2D m_rollingCollider;
    [SerializeField] private PolygonCollider2D m_stunnedCollider;
    [SerializeField] private Animator m_anim;

    private Vector3 m_bottomPadShift = new Vector3(0, -3, 0);
    private Vector3 m_topLeftPadShift = new Vector3(3, 1, 0);
    private Vector3 m_topRightPadShift = new Vector3(-3, 1, 0);

    private GameObject m_moveTo = null;

    [SerializeField] private float m_inverseBounceSpeed = 0.2f;
    [SerializeField] private float m_rollSpeed = 75f;

    [SerializeField] private int m_stunLockedPosition;
    [SerializeField] private int m_health = 1;

    [SerializeField] private bool m_moveToTopPlatform = false;
    [SerializeField] private bool m_moveToRightPlatform = false;
    [SerializeField] private bool m_moveToLeftPlatform = false;

    [SerializeField] private bool m_moveToBottomPad = false;
    [SerializeField] private bool m_moveToTopLeftPad = false;
    [SerializeField] private bool m_moveToTopRightPad = false;

    [SerializeField] private bool m_padsEmerging = false;
    [SerializeField] private bool m_stunLocked = false;
    [SerializeField] private bool m_dead = false;

    [SerializeField] private bool m_padsHidden = false;
    [SerializeField] private bool m_stunLocksHidden = false;

    [SerializeField] private bool m_bottomOrigin = false;
    [SerializeField] private bool m_topLeftOrigin = false;
    [SerializeField] private bool m_topRightOrigin = false;
    [SerializeField] private bool m_hitFromLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        m_RB = GetComponent<Rigidbody2D>();
        m_RB.velocity = new Vector2(1,-1) * 10;
        ToggleStunLocksAndPads(true, true);

        // Play earth loop clip
        int randomIndex = UnityEngine.Random.Range(0, m_earthLoopSounds.Length);
        m_earthLoopAudioSource.clip = m_earthLoopSounds[randomIndex];
        m_earthLoopAudioSource.loop = true;
        m_earthLoopAudioSource.Play();

        // Play roll loop clip
        randomIndex = UnityEngine.Random.Range(0, m_rollLoopSounds.Length);
        m_rollLoopAudioSource.clip = m_rollLoopSounds[randomIndex];
        m_rollLoopAudioSource.loop = true;
        m_rollLoopAudioSource.Play();
    }

    private void ToggleStunLocksAndPads(bool hideStunLocks, bool hidePads)
    {
        if (hidePads && m_padsHidden)
        {
            m_bottomBouncePad.transform.localPosition -= m_bottomPadShift;
            m_topLeftBouncePad.transform.localPosition -= m_topLeftPadShift;
            m_topRightBouncePad.transform.localPosition -= m_topRightPadShift;
            m_padsHidden = false;
        }
        else if (hidePads && !m_padsHidden)
        {
            m_bottomBouncePad.transform.localPosition += m_bottomPadShift;
            m_topLeftBouncePad.transform.localPosition += m_topLeftPadShift;
            m_topRightBouncePad.transform.localPosition += m_topRightPadShift;
            m_padsHidden = true;
        }
        if (hideStunLocks && m_stunLocksHidden)
        {
            m_bottomStunLock.transform.localPosition -= m_bottomPadShift;
            m_topLeftStunLock.transform.localPosition -= m_topLeftPadShift;
            m_topRightStunLock.transform.localPosition -= m_topRightPadShift;
            m_stunLocksHidden = false;
        }
        else if (hideStunLocks && !m_stunLocksHidden)
        {
            m_bottomStunLock.transform.localPosition += m_bottomPadShift;
            m_topLeftStunLock.transform.localPosition += m_topLeftPadShift;
            m_topRightStunLock.transform.localPosition += m_topRightPadShift;
            m_stunLocksHidden = true;
        }
    }

    float timeSinceLastUpdate = 0;
    // Update is called once per frame
    void Update()
    {
        MoveTowardPoint();

        if(!m_padsEmerging && !m_stunLocked && !m_dead) StartCoroutine(PadsEmerge());

        UpdateAudioSourcesPosition();
    }

    private void UpdateAudioSourcesPosition()
    {      
        Vector3 bossPos = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
        m_earthLoopAudioSource.transform.position = bossPos;
        m_rollLoopAudioSource.transform.position = bossPos;
        m_hurtAudioSource.transform.position = bossPos;
        m_gruntAudioSource.transform.position = bossPos;

        if (!m_bounceAudioSource.isPlaying)
            m_bounceAudioSource.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
    }

    IEnumerator PadsEmerge()
    {
        m_padsEmerging = true;
        yield return new WaitForSeconds(Random.Range(4,7));
        ToggleStunLocksAndPads(false, true);
        yield return new WaitForSeconds(Random.Range(6, 12));
        ToggleStunLocksAndPads(true, true);
        yield return new WaitForSeconds(5);
        m_padsEmerging = false;
    }

    private void MoveTowardPoint()
    {
        if (m_moveToTopPlatform)
        {
            m_moveTo = m_topPlatform;
        }
        else if (m_moveToLeftPlatform)
        {
            m_moveTo = m_leftPlatform;
        }
        else if (m_moveToRightPlatform)
        {
            m_moveTo = m_rightPlatform;
        }
        else if (m_moveToBottomPad)
        {
            m_moveTo = m_bottomBouncePad;
        }
        else if (m_moveToTopLeftPad)
        {
            m_moveTo = m_topLeftBouncePad;
        }
        else if (m_moveToTopRightPad)
        {
            m_moveTo = m_topRightBouncePad;
        }
        else
        {
            m_moveTo = null;
        }

        if (!m_stunLocked)
        {
            if (m_moveTo != null) transform.position = Vector3.MoveTowards(transform.position, m_moveTo.transform.position, Time.deltaTime* m_inverseBounceSpeed);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisions(collision);
    }

    private void HandleCollisions(Collision2D collision)
    {

        int randomIndex;
        switch (collision.gameObject.name)
        {
            case "Bounce_Pad_Bottom":
                ResetMoveToBooleans();
                if (transform.localPosition.x > 0 || m_topRightOrigin)
                {
                    m_moveToRightPlatform = true;
                    m_bottomOrigin = true;
                    m_topLeftOrigin = false;
                    m_topRightOrigin = false;
                    m_hitFromLeft = true;
                }
                else if (transform.localPosition.x < 0 || m_topLeftOrigin)
                {
                    m_moveToLeftPlatform = true;
                    m_bottomOrigin = true;
                    m_topLeftOrigin = false;
                    m_topRightOrigin = false;
                    m_hitFromLeft = false;
                }
                m_RB.velocity = Vector3.zero;

                // Play audio clip
                randomIndex = UnityEngine.Random.Range(0, m_bounceSounds.Length);
                m_bounceAudioSource.PlayOneShot(m_bounceSounds[randomIndex]);
                break;
            case "Bounce_Pad_TopRight":
                ResetMoveToBooleans();
                if (m_bottomOrigin || m_topLeftOrigin || transform.localPosition.x < -38)
                {
                    m_moveToTopPlatform = true;
                    m_bottomOrigin = false;
                    m_topLeftOrigin = false;
                    m_topRightOrigin = true;
                    m_hitFromLeft = false;
                }
                else if (transform.localPosition.x > -38)
                {
                    m_moveToRightPlatform = true;
                    m_bottomOrigin = false;
                    m_topLeftOrigin = false;
                    m_topRightOrigin = true;
                    m_hitFromLeft = true;
                }

                m_RB.velocity = Vector3.zero;

                // Play audio clip
                randomIndex = UnityEngine.Random.Range(0, m_bounceSounds.Length);
                m_bounceAudioSource.PlayOneShot(m_bounceSounds[randomIndex]);
                break;
            case "Bounce_Pad_TopLeft":
                ResetMoveToBooleans();
                if (m_bottomOrigin || m_topRightOrigin || transform.localPosition.x > 38)
                {
                    m_moveToTopPlatform = true;
                    m_bottomOrigin = false;
                    m_topLeftOrigin = true;
                    m_topRightOrigin = false;
                    m_hitFromLeft = true;
                }
                else if (transform.localPosition.x < 38)
                {
                    m_moveToLeftPlatform = true;
                    m_bottomOrigin = false;
                    m_topLeftOrigin = true;
                    m_topRightOrigin = false;
                    m_hitFromLeft = false;
                }
                m_RB.velocity = Vector3.zero;

                // Play audio clip
                randomIndex = UnityEngine.Random.Range(0, m_bounceSounds.Length);
                m_bounceAudioSource.PlayOneShot(m_bounceSounds[randomIndex]);
                break;
            case "Top_Platform":
                ResetMoveToBooleans();
                if (m_bottomOrigin && m_hitFromLeft)
                {
                    m_moveToLeftPlatform = true;
                }
                else if (m_bottomOrigin && !m_hitFromLeft)
                {
                    m_moveToRightPlatform = true;
                }
                else if (m_topLeftOrigin && m_hitFromLeft)
                {
                    m_moveToLeftPlatform = true;
                }
                else if (m_topLeftOrigin && !m_hitFromLeft)
                {
                    m_moveToTopRightPad = true;
                }
                else if (m_topRightOrigin && m_hitFromLeft)
                {
                    m_moveToTopLeftPad = true;
                }
                else if (m_topRightOrigin && !m_hitFromLeft)
                {
                    m_moveToRightPlatform = true;
                }
                break;
            case "Left_Platform":
                ResetMoveToBooleans();
                if (m_bottomOrigin && m_hitFromLeft)
                {
                    m_moveToTopLeftPad = true;
                }
                else if (m_bottomOrigin && !m_hitFromLeft)
                {
                    m_moveToTopPlatform = true;
                }
                else if (m_topLeftOrigin && m_hitFromLeft)
                {
                    m_moveToRightPlatform = true;
                }
                else if (m_topLeftOrigin && !m_hitFromLeft)
                {
                    m_moveToRightPlatform = true;
                }
                else if (m_topRightOrigin && m_hitFromLeft)
                {
                    m_moveToTopPlatform = true;
                }
                else if (m_topRightOrigin && !m_hitFromLeft)
                {
                    m_moveToBottomPad = true;
                }
                break;
            case "Right_Platform":
                ResetMoveToBooleans();
                if (m_bottomOrigin && m_hitFromLeft)
                {
                    m_moveToTopPlatform = true;
                }
                else if (m_bottomOrigin && !m_hitFromLeft)
                {
                    m_moveToTopRightPad = true;
                }
                else if (m_topLeftOrigin && m_hitFromLeft)
                {
                    m_moveToTopRightPad = true;
                }
                else if (m_topLeftOrigin && !m_hitFromLeft)
                {
                    m_moveToTopPlatform = true;
                }
                else if (m_topRightOrigin && m_hitFromLeft)
                {
                    m_moveToLeftPlatform = true;
                }
                else if (m_topRightOrigin && !m_hitFromLeft)
                {
                    m_moveToLeftPlatform = true;
                }
                break;
            case "HookCollider":
                if (m_stunLocked)
                {
                    DamageOrNaw(true);
                }
                break;
        }
    }

    private void ResetMoveToBooleans()
    {
        m_moveToTopPlatform = false;
        m_moveToLeftPlatform = false;
        m_moveToRightPlatform = false;
        m_moveToBottomPad = false;
        m_moveToTopLeftPad = false;
        m_moveToTopRightPad = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.name)
        {
            case "Top_Boost":
                if (!m_dead)
                {
                    if (m_RB.velocity.x > 0)
                    {
                        m_RB.velocity = Vector2.right * m_rollSpeed;
                    }
                    else
                    {
                        m_RB.velocity = Vector2.left * m_rollSpeed;
                    }
                }
                break;
            case "BottomLeft_Boost":
                if (!m_dead)
                {
                    if (m_RB.velocity.x > 0)
                    {
                        m_RB.velocity = new Vector2(1, -2).normalized * m_rollSpeed;
                    }
                    else
                    {
                        m_RB.velocity = new Vector2(-1, 1).normalized * m_rollSpeed;
                    }
                }
                break;
            case "BottomRight_Boost":
                if (!m_dead)
                {
                    if (m_RB.velocity.x > 0)
                    {
                        m_RB.velocity = new Vector2(1, 1).normalized * m_rollSpeed;
                    }
                    else
                    {
                        m_RB.velocity = new Vector2(-1, -2).normalized * m_rollSpeed;
                    }
                }
                break;
            case "StunLock_Bottom":
                StunLock(new Vector3(0.568f, -0.065f, 0), 0);
                StartCoroutine(DoTheThing());
                break;
            case "StunLock_TopLeft":
                StunLock(new Vector3(0.335f, 0.4f, 0), 1);
                StartCoroutine(DoTheThing());
                break;
            case "StunLock_TopRight":
                StunLock(new Vector3(7.47f, -0.71f, 0), 2);
                StartCoroutine(DoTheThing());
                break;

        }
    }

    IEnumerator DoTheThing()
    {
        yield return new WaitForSeconds(2);
        if (m_stunLocked)
        {
            DamageOrNaw(false);
        }
    }

    // in stunLockedPosition determines which stunLock the boss is stopped at
    // 0 = bottom, 1 = top left, 2 = top right
    private void StunLock(Vector3 weakPointPosition, int stunLockedPosition)
    {
        m_stunLockedPosition = stunLockedPosition;
        m_stunLocked = true;
        switch (m_stunLockedPosition)
        {
            case 0:
                m_RB.transform.localPosition = new Vector3(-1.45f, -39.75f, 325);
                m_RB.transform.localEulerAngles = new Vector3(0, -180, 1.45f);
                break;
            case 1:
                transform.localScale = new Vector3(-1, 1, 1);
                m_RB.transform.localPosition = new Vector3(33.5f, 21.2f, 325);
                m_RB.transform.localEulerAngles = new Vector3(0, -180, -125);
                break;
            case 2:
                m_RB.transform.localPosition = new Vector3(-33.7f, 21.4f, 325);
                m_RB.transform.localEulerAngles = new Vector3(0, -180, -237);
                break;
        }
        m_anim.SetBool("stunLocked", m_stunLocked);
        m_rollingCollider.enabled = false;
        m_stunnedCollider.enabled = true;
        m_RB.constraints = RigidbodyConstraints2D.FreezeAll;
        ResetMoveToBooleans();

        m_rollLoopAudioSource.Stop();
        m_earthLoopAudioSource.Stop();

        // Play breathing loop clip
        int randomIndex = UnityEngine.Random.Range(0, m_breathingLoopSounds.Length);
        m_gruntAudioSource.clip = m_breathingLoopSounds[randomIndex];
        m_gruntAudioSource.loop = true;
        m_gruntAudioSource.Play();
    }

    public void DamageOrNaw(bool damageYe)
    {
        if (damageYe)
        {
            m_health -= 1;
            m_anim.SetInteger("bossHP", m_health);

            // Play audio clips
            int randomIndex = UnityEngine.Random.Range(0, m_hurtSounds.Length);
            m_hurtAudioSource.PlayOneShot(m_hurtSounds[randomIndex]);

            randomIndex = UnityEngine.Random.Range(0, m_gruntSounds.Length);
            m_gruntAudioSource.loop = false;
            m_gruntAudioSource.PlayOneShot(m_gruntSounds[randomIndex]);
        }
        m_RB.constraints = RigidbodyConstraints2D.None;
        if (m_health != 0)
        {
            m_stunLocked = false;
            m_anim.SetBool("stunLocked", m_stunLocked);
            m_rollingCollider.enabled = true;
            m_stunnedCollider.enabled = false;

            ToggleStunLocksAndPads(true, false);
            
            switch (m_stunLockedPosition)
            {
                case 0:
                    m_RB.transform.localPosition -= m_bottomPadShift * 0.5f;
                    m_RB.velocity = Vector2.right * m_rollSpeed;
                    break;
                case 1:
                    m_RB.transform.localPosition -= m_topLeftPadShift * 0.5f;
                    transform.localScale = new Vector3(1, 1, 1);
                    m_RB.velocity = new Vector2(-1, -1).normalized * m_rollSpeed;
                    break;
                case 2:
                    m_RB.transform.localPosition -= m_topRightPadShift * 0.5f;
                    m_RB.velocity = new Vector2(1, -1).normalized * m_rollSpeed;
                    break;
            }
        }
        else
        {
            m_stunLocked = false;
            m_dead = true;
            switch (m_stunLockedPosition)
            {
                case 1:
                    m_RB.velocity = new Vector2(-1, -1).normalized * m_rollSpeed;
                    break;
                case 2:
                    m_RB.velocity = new Vector2(1, -1).normalized * m_rollSpeed;
                    break;
            }
            StartCoroutine(DisableCollidersInSeconds());

            // Play death audio
            // Play audio clip
            int randomIndex = UnityEngine.Random.Range(0, m_deathSounds.Length);
            m_gruntAudioSource.PlayOneShot(m_deathSounds[randomIndex]);
        }

        m_rollLoopAudioSource.Play();
        m_earthLoopAudioSource.Play();

        if(m_gruntAudioSource.loop)
            m_gruntAudioSource.Stop();
    }

    IEnumerator DisableCollidersInSeconds()
    {
        yield return new WaitForSeconds(5);
        m_RB.constraints = RigidbodyConstraints2D.FreezeAll;
        m_rollingCollider.enabled = false;
        m_stunnedCollider.enabled = false;
    }

    public bool IsStunLocked()
    {
        return m_stunLocked;
    }
}
