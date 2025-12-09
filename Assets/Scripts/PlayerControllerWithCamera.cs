using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerControllerWithCamera : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5f;
    public float jumpForce = 4f;
    public float jumpCooldown = 1.5f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashCooldown = 3f;

    [Header("UI")]
    [Tooltip("TextMeshPro Text to show jump cooldown")]
    public TextMeshProUGUI jumpCooldownText;
    [Tooltip("TextMeshPro Text to show dash cooldown")]
    public TextMeshProUGUI dashCooldownText;
    [Tooltip("Color for cooldown text when ready")]
    public Color cooldownReadyColor = Color.green;
    [Tooltip("Color for cooldown text when on cooldown")]
    public Color cooldownOnCooldownColor = Color.red;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float cameraFOV = 50f;
    public float groundCheckDistance = 0.25f;
    public LayerMask groundLayer = ~0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpClip;
    public AudioClip dashClip;

    [Tooltip("Audio source for continuous footsteps while moving (looping)")]
    public AudioSource footstepAudioSource;
    [Tooltip("Audio clip for footsteps when moving (should be a short looping clip)")]
    public AudioClip footstepClip;
    [Range(0f, 1f)]
    public float footstepVolume = 0.3f;

    public Rigidbody rb;
    public bool isGrounded;

    private CapsuleCollider capsuleCollider;
    private float currentMoveSpeed;
    private bool isSprinting = false;
    private float nextJumpTime = 0f;
    private float nextDashTime = 0f;

    [Header("Footprints")]
    public GameObject footprintPrefab;
    public float footprintInterval = 0.5f;
    private float nextFootprintTime = 0f;
    public float footprintHeightOffset = 0.1f;
    private Quaternion cameraOriginalLocalRot;
    float rotationX;
    float rotationY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        // Setup footstep audio source
        if (!footstepAudioSource)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length > 1)
                footstepAudioSource = sources[1];
            else
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
        }

        footstepAudioSource.loop = true;
        footstepAudioSource.volume = footstepVolume;
        footstepAudioSource.clip = footstepClip;

        // Add retro PS2 audio effect to main audio sources for crunchy, low-fi sound
        if (audioSource != null)
        {
            var eff = audioSource.GetComponent<PS2AudioEffect>();
            if (eff == null) eff = audioSource.gameObject.AddComponent<PS2AudioEffect>();
            eff.SetBitDepth(8);
            eff.SetLowpassCutoff(6000f);
            eff.SetEffectAmount(0.85f);
        }

        if (footstepAudioSource != null && footstepAudioSource != audioSource)
        {
            var eff2 = footstepAudioSource.GetComponent<PS2AudioEffect>();
            if (eff2 == null) eff2 = footstepAudioSource.gameObject.AddComponent<PS2AudioEffect>();
            eff2.SetBitDepth(7);
            eff2.SetLowpassCutoff(5000f);
            eff2.SetEffectAmount(0.9f);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentMoveSpeed = walkSpeed;
        nextJumpTime = 0f;
        nextDashTime = 0f;

        // Force these runtime defaults to ensure Inspector overrides don't keep old values.
        // NOTE: This will overwrite values set in the Inspector at runtime.
        jumpCooldown = 1.5f;
        dashCooldown = 3f;
        dashForce = 20f;
        Debug.Log($"PlayerController enforced runtime defaults: jumpCooldown={jumpCooldown}, dashCooldown={dashCooldown}, dashForce={dashForce}");

        // Hide legacy UI text elements for jump/dash if present (we're removing textual UI)
        if (jumpCooldownText != null)
            jumpCooldownText.gameObject.SetActive(false);
        if (dashCooldownText != null)
            dashCooldownText.gameObject.SetActive(false);

        if (cameraTransform)
        {
            var cam = cameraTransform.GetComponent<Camera>();
            if (cam != null)
                cam.fieldOfView = cameraFOV;

            cameraOriginalLocalRot = cameraTransform.localRotation;
            cam.nearClipPlane = 0.01f;
        }
    }

    // Public helpers to show/hide the cooldown UI from other systems (e.g., StartTrigger)
    public void ShowCooldownUI()
    {
        if (jumpCooldownText != null)
            jumpCooldownText.gameObject.SetActive(true);
        if (dashCooldownText != null)
            dashCooldownText.gameObject.SetActive(true);
    }

    public void HideCooldownUI()
    {
        if (jumpCooldownText != null)
            jumpCooldownText.gameObject.SetActive(false);
        if (dashCooldownText != null)
            dashCooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleCamera();
        HandleSprint();
        UpdateMovementSpeed();
        HandleMove();
        HandleJump();
        HandleDash();
        UpdateSprintUI();
    }

    void HandleCamera()
    {
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, -80, 90);

        transform.rotation = Quaternion.Euler(0, rotationX, 0);

        if (cameraTransform)
            cameraTransform.localRotation = Quaternion.Euler(rotationY, 0, 0);
    }

    void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            isSprinting = true;
        else
            isSprinting = false;
    }

    void HandleCrouch()
    {
        // Crouch removed
    }

    void UpdateMovementSpeed()
    {
        if (isSprinting)
            currentMoveSpeed = sprintSpeed;
        else
            currentMoveSpeed = walkSpeed;
    }

    void HandleMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x).normalized;

        // FIXED moveSpeed → currentMoveSpeed
        rb.velocity = new Vector3(dir.x * currentMoveSpeed, rb.velocity.y, dir.z * currentMoveSpeed);

        bool isMoving = (x != 0f || z != 0f);
        bool grounded = IsGrounded();

        // Footsteps loop
        if (footstepAudioSource && footstepClip)
        {
            if (grounded && isMoving)
            {
                if (!footstepAudioSource.isPlaying)
                    footstepAudioSource.Play();
            }
            else
            {
                if (footstepAudioSource.isPlaying)
                    footstepAudioSource.Stop();
            }
        }

        // Footprints
        if (grounded && isMoving && Time.time >= nextFootprintTime)
        {
            SpawnFootprint();
            nextFootprintTime = Time.time + footprintInterval;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && Time.time >= nextJumpTime)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            nextJumpTime = Time.time + jumpCooldown;

            if (audioSource && jumpClip)
                audioSource.PlayOneShot(jumpClip);
        }
    }

    void HandleDash()
    {
        // Dash only on LeftShift release if player is grounded
        // (to avoid conflict with sprint which holds LeftShift)
        if (Input.GetKeyUp(KeyCode.LeftShift) && IsGrounded() && Time.time >= nextDashTime)
        {
            rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
            nextDashTime = Time.time + dashCooldown;

            if (audioSource && dashClip)
                audioSource.PlayOneShot(dashClip);

            if (cameraTransform)
                StartCoroutine(ShakeCamera(0.35f, 3.0f));

            Debug.Log("Player dashed!");
        }
    }

    void UpdateSprintUI()
    {
        // Update jump cooldown text
        if (jumpCooldownText != null)
        {
            float jumpTimeLeft = Mathf.Max(0f, nextJumpTime - Time.time);
            if (jumpTimeLeft <= 0f)
            {
                jumpCooldownText.text = "JUMP: READY";
                jumpCooldownText.color = cooldownReadyColor;
            }
            else
            {
                jumpCooldownText.text = $"JUMP: {jumpTimeLeft:F1}s";
                jumpCooldownText.color = cooldownOnCooldownColor;
            }
        }

        // Update dash cooldown text
        if (dashCooldownText != null)
        {
            float dashTimeLeft = Mathf.Max(0f, nextDashTime - Time.time);
            if (dashTimeLeft <= 0f)
            {
                dashCooldownText.text = "DASH: READY";
                dashCooldownText.color = cooldownReadyColor;
            }
            else
            {
                dashCooldownText.text = $"DASH: {dashTimeLeft:F1}s";
                dashCooldownText.color = cooldownOnCooldownColor;
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // Only the current Taya can transfer the tag
            if (gm.currentTaya != gameObject)
                return;

            // Prevent redundant swaps: don't swap if the collision target is already the Taya
            if (col.gameObject == gm.currentTaya)
                return;

            bool isFriendTag = col.gameObject.CompareTag("Friend");
            bool inFriendsList = (gm.friends != null && gm.friends.Contains(col.gameObject));

            if (isFriendTag || inFriendsList)
                gm.SwitchTaya(col.gameObject);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    bool IsGrounded()
    {
        if (isGrounded)
            return true;

        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        return Physics.Raycast(ray, groundCheckDistance + 0.05f, groundLayer);
    }

    System.Collections.IEnumerator ShakeCamera(float duration, float magnitude)
    {
        if (cameraTransform == null)
            yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float damper = 1f - (elapsed / duration);
            float x = (Random.value * 2f - 1f) * magnitude * damper;
            float y = (Random.value * 2f - 1f) * magnitude * 0.5f * damper;
            float z = (Random.value * 2f - 1f) * magnitude * 0.2f * damper;

            Quaternion offset = Quaternion.Euler(x, y, z);
            cameraTransform.localRotation = cameraOriginalLocalRot * offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localRotation = cameraOriginalLocalRot;
    }

    void SpawnFootprint()
    {
        if (footprintPrefab == null)
            return;

        Vector3 footPos = transform.position + Vector3.down * footprintHeightOffset;
        Instantiate(footprintPrefab, footPos, Quaternion.identity);
    }
}
