using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public float m_grappleSpeed = 1;
    public float m_grapplingStartHeightOffset = 1;
    public Vector3 m_hookColliderSize = new Vector3(0.5f, 0.5f, 0.5f);
    public bool m_tethered = false;
    public float m_pullUpForce = 10f;
    public bool m_pullingUp = false;

    private bool m_grapplingHookOut = false;
    private LineRenderer m_grapplingHookRenderer;
    private Vector3 m_clickPosition;
    private Vector3 m_currentPosition;
    private Vector3 m_directionUnitVector;
    private BoxCollider2D m_hookCollider;

    [SerializeField] private PlayerController m_player;
    private Rigidbody2D m_playerRigidBody;
    private DistanceJoint2D m_distJoint;

    // Start is called before the first frame update
    void Start()
    {
        m_grapplingHookRenderer = GetComponent<LineRenderer>();
        m_hookCollider = GetComponentInChildren<BoxCollider2D>();
        m_distJoint = m_player.GetComponent<DistanceJoint2D>();
        m_playerRigidBody = m_player.GetComponent<Rigidbody2D>();

        m_hookCollider.enabled = false;
        m_distJoint.enabled = false;
        m_grapplingHookOut = false;
        m_tethered = false;

        m_currentPosition = new Vector3(0, 0, 0);
        m_clickPosition = new Vector3(0, 0, 0);
        m_directionUnitVector = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        HandleGrapplingHook();
        RefreshHookPosition();
    }

    public void LaunchHook(Vector3 pMouseClickPosition)
    {
        // Handling launch

        if (m_grapplingHookOut)
        {
            RetractHook();
        }
        else
        {
            // Launching grappling hook

            // Changing Z value of mouse pos to a positive value, to avoid the object showing behind the near clip plane of the camera.
            pMouseClickPosition.z = 1;

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(pMouseClickPosition);

            // Setting worldpos z to 1 so it doesnt draw behind the background.
            worldPosition.z = 0;

            // Calculating direction vector
            Vector3 direction = worldPosition - (m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0));
            m_directionUnitVector = direction / direction.magnitude;

            m_clickPosition = worldPosition;
            m_currentPosition = m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0);

            m_grapplingHookOut = true;

            Vector3 position = m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0);
            m_hookCollider.gameObject.transform.position = new Vector3(position.x, position.y, 0);
            m_hookCollider.size = m_hookColliderSize;

            m_hookCollider.enabled = true;
            m_pullingUp = false;
        }
    }

    public void RetractHook()
    {
        m_hookCollider.enabled = false;

        // Reset hook display
        m_grapplingHookRenderer.positionCount = 0;
        m_currentPosition = new Vector3(0, 0, 0);
        m_clickPosition = new Vector3(0, 0, 0);

        // Retracting hook
        m_grapplingHookOut = false;

        Vector3 position = m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0);
        m_hookCollider.gameObject.transform.position = new Vector3(position.x, position.y, 0);

        m_tethered = false;
        m_distJoint.enabled = false;

        m_player.ResetGravity();
    }

    public void PullUp()
    {
        if(m_tethered)
        {
            // We have to be tethered to pull up.
            Vector3 direction = m_clickPosition - (m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0));
            direction = direction / direction.magnitude;

            m_playerRigidBody.velocity = new Vector2(0, 0); // Resetting velocity

            m_pullingUp = true;
        }
    }

    public void HandleCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            Debug.Log("Hit!");
            m_tethered = true;

            m_distJoint.enabled = true;
            m_distJoint.connectedAnchor = m_hookCollider.transform.position;
        }
    }
    void HandleGrapplingHook()
    {        
        // Handling display and collision
        if (m_grapplingHookOut && !m_tethered)
        {
            // Making a line
            m_grapplingHookRenderer.positionCount = 2;

            RefreshHookPosition();

            // Increasing the length of the whole line
            m_currentPosition = m_currentPosition + (new Vector3(m_directionUnitVector.x, m_directionUnitVector.y, 0) * m_grappleSpeed * Time.fixedDeltaTime);

            Vector3 secondPointPosition = m_currentPosition;
            secondPointPosition.z = 0;
            m_grapplingHookRenderer.SetPosition(1, secondPointPosition);

            // Handle Hook Collider
            Vector3 position = new Vector3(secondPointPosition.x, secondPointPosition.y, 0);
            m_hookCollider.attachedRigidbody.MovePosition(position);
        }
    }

    void RefreshHookPosition()
    {
        if(m_grapplingHookOut)
        {
            // We also add the vector 0, offset, 0 to make the grappling hook start above our character.

            Vector3 firstPointPosition = m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0);
            firstPointPosition.z = 0;
            m_grapplingHookRenderer.SetPosition(0, firstPointPosition);
        }
    }

    public Vector3 GetHookDirection()
    {
        Vector3 direction = m_hookCollider.gameObject.transform.position - (m_player.transform.position + new Vector3(0, m_grapplingStartHeightOffset, 0));
        return direction.normalized;
    }
}