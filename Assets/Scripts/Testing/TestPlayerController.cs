using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : NetworkBehaviour
{
    [SerializeField]
    private float _moveSpeed = 300.0f;

    [SerializeField]
    private float _lookSpeed = 300.0f;

    [SerializeField]
    private float _fireRecoilForce = 15.0f;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private PlayerVisual _playerVisual;

    [SerializeField]
    private GameObject _projectilePrefab; 

    [SerializeField]
    private GameObject _projectileExplosionPrefab; 

    private TestPlayerInputActions _inputAction;
    private InputAction _moveInputAction;
    private InputAction _fireInputAction;
    private InputAction _lookInputAction;
    private Rigidbody _rigidBody;
    private AudioSource _movementAudioSource;
    private AudioSource _missingProjectileAudioSource;

    private float _yRotation;

    private void OnEnable()
    {
        _inputAction = new TestPlayerInputActions();
        _moveInputAction = _inputAction.Player.Move;
        _fireInputAction = _inputAction.Player.Fire;
        _lookInputAction = _inputAction.Player.Look;

        _moveInputAction.Enable();
        _fireInputAction.Enable();
        _lookInputAction.Enable();
 
        _fireInputAction.performed += Fire;
    }

    private void OnDisable()
    {
        _moveInputAction.Disable();
        _fireInputAction.Disable();
        _lookInputAction.Disable();

        _fireInputAction.performed -= Fire;
    }

    private void Awake()
    {
        if (IsOwner) _inputAction = new TestPlayerInputActions();
    }


    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        var audioSources = GetComponents<AudioSource>();
        _movementAudioSource = audioSources[0];
        _missingProjectileAudioSource = audioSources[1];
        if (!IsOwner) _camera.enabled = false;
        var playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        _playerVisual.SetPlayerColor(MultiplayerManager.Instance.GetPlayerColor(playerData.ColorId));
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _inputAction = new TestPlayerInputActions();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            Look();
            MovementSoundEffect();
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Movement();
        }
    }

    private void Movement()
    {
        var movement = _moveInputAction.ReadValue<Vector2>();
        var direction = transform.forward * movement.y + transform.right * movement.x;
        _rigidBody.AddForce(direction.normalized * _moveSpeed, ForceMode.Force);
    }

    private void MovementSoundEffect() {
        Debug.Log(_rigidBody.velocity);

        bool isMoving = Math.Abs(_rigidBody.velocity.x) > 0.5 || Math.Abs(_rigidBody.velocity.z) > 0.5;
        if(isMoving && !_movementAudioSource.isPlaying) {
            _movementAudioSource.Play(0);
        } else if(!isMoving && _movementAudioSource.isPlaying) {
            _movementAudioSource.Stop();
        }
    }

    private void Look()
    {
        var lookDirection = _lookInputAction.ReadValue<Vector2>() * Time.deltaTime * _lookSpeed;
        _yRotation += lookDirection.x;

        transform.rotation = Quaternion.Euler(0, _yRotation, 0);
    }

    private void Fire(InputAction.CallbackContext context)
    {
        if(!MatchUIManager.instance.isReloading) {
            // Add the projectile GameObject and the explosion
            var playerHeadObject = _playerVisual.transform.GetChild(1).gameObject;

            var thisObjectHeight = transform.position.y;
            var playerHeadObjectHeight = playerHeadObject.transform.position.y;
            var playerHeadObjectWidth = playerHeadObject.GetComponent<Renderer>().bounds.size.z;
            var cannonHeight = playerHeadObjectHeight - thisObjectHeight;
            
            var launchPosition = new Vector3(transform.position.x, cannonHeight, transform.position.z);
            var forwardOffset = transform.rotation * Vector3.forward * (playerHeadObjectWidth * 0.5f);
            var explosionForwardOffset = transform.rotation * Vector3.forward * (playerHeadObjectWidth * 0.7f);

            Instantiate(_projectilePrefab, launchPosition + forwardOffset, transform.rotation);
            Instantiate(_projectileExplosionPrefab, launchPosition + explosionForwardOffset, transform.rotation);

            // Add recoil to the tank
            var direction = transform.forward * -1;
            _rigidBody.AddForce(direction.normalized * _fireRecoilForce, ForceMode.Impulse);

            // Update game ui to display "reloading" state
            MatchUIManager.instance.StartReload();
        } else {
            _missingProjectileAudioSource.Play(0);
        }
    }
}
