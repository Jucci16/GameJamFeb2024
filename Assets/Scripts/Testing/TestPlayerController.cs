using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayerController : NetworkBehaviour
{
    private const int playerHeadIndex = 0;
    private const int playerShellIndex = 1;
    private const int playerTreadsIndex = 2;

    [SerializeField]
    private float _moveSpeed = 50.0f;

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

    private PlayerData _playerData;
    private TestPlayerInputActions _inputAction;
    private InputAction _moveInputAction;
    private InputAction _fireInputAction;
    private InputAction _lookInputAction;
    public PlayerDisplayState state = PlayerDisplayState.show;
    private Rigidbody _rigidBody;
    private AudioSource _movementAudioSource;
    private AudioSource _missingProjectileAudioSource;
    private float _currentBodyRotation = 0f;
    private Vector2 _lastMovementInput;
    private float _bodyRotationProgress = 1.0f;

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
        _yRotation = transform.rotation.eulerAngles.y;
        _rigidBody = GetComponent<Rigidbody>();
        var audioSources = GetComponents<AudioSource>();
        _movementAudioSource = audioSources[0];
        _missingProjectileAudioSource = audioSources[1];
        if (!IsOwner) {
            _camera.enabled = false;
            Destroy(_camera.gameObject.GetComponent<AudioListener>());
        }
        _playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
        var playerColor = MultiplayerManager.Instance.GetPlayerColor(_playerData.ColorId);
        _playerVisual.SetPlayerColor(playerColor);
        if (IsOwner) MatchUIManager.instance.SetPlayerColor(playerColor);
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
        if (IsOwner && state == PlayerDisplayState.show)
        {
            Look();
            MovementSoundEffect();
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner && state == PlayerDisplayState.show)
        {
            Movement();
        }
    }

    public void resetYRotation(float rotation) {
        _yRotation = rotation;
    }

    private void Movement()
    {
        var movement = _moveInputAction.ReadValue<Vector2>();
        var direction = transform.forward * movement.y + transform.right * movement.x;

        // Rotate bottom half of tank
        RotateTankBody(movement, direction);

        // Move object
        _rigidBody.AddForce(direction.normalized * _moveSpeed, ForceMode.Force);
    }

    private void RotateTankBody(Vector2 movementInput, Vector3 direction) {
        var playerShellObject = _playerVisual.transform.GetChild(playerShellIndex).gameObject;
        var playerTreadsObject = _playerVisual.transform.GetChild(playerTreadsIndex).gameObject;

        if(_lastMovementInput != movementInput) {
            _lastMovementInput = movementInput;
            _bodyRotationProgress = 0.0f;
        }
        if(Math.Abs(movementInput.y) > 0.1f || Math.Abs(movementInput.x) > 0.1f) {
             _bodyRotationProgress += Time.deltaTime;

            // Figure out the current angle/axis
            Quaternion sourceOrientation = playerShellObject.transform.rotation;
            float sourceAngle;
            Vector3 sourceAxis;
            sourceOrientation.ToAngleAxis(out sourceAngle, out sourceAxis);

            // Calculate a new target orientation
            var targetAngle = (TransformUtils.GetYRotFromVec(new Vector2(0f,0f), new Vector2(direction.x, direction.z)) + 270) % 360;
            // Ensure the shortest path is taken to the angle (by allowing over 360 and under 0 degrees)
            if((sourceAngle % 360) - targetAngle > 180) targetAngle = sourceAngle + (360 - (sourceAngle % 360) + targetAngle);
            else if(targetAngle - (sourceAngle % 360) > 180) targetAngle = sourceAngle - (sourceAngle % 360) - (360 - targetAngle);
         
            // Interpolate to get the current angle/axis between the source and target.
            float currentAngle = Mathf.Lerp(sourceAngle, targetAngle, Math.Min(_bodyRotationProgress, 1));
           
            // Assign the current rotation
            _currentBodyRotation = currentAngle;
            if(currentAngle >= 360) {
                _currentBodyRotation = currentAngle % 360;
            } else if(currentAngle < 0){
                _currentBodyRotation = 360 + (currentAngle % 360);
            }
        }
        var targetAxis = new Vector3(0f,1f,0f);
        playerShellObject.transform.rotation = Quaternion.AngleAxis(_currentBodyRotation, targetAxis);
        playerTreadsObject.transform.rotation = Quaternion.AngleAxis(_currentBodyRotation, targetAxis);
    }

    private void MovementSoundEffect() {
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
        if(IsOwner && state == PlayerDisplayState.show) {
            if(!MatchUIManager.instance.isReloading) {
                // Add the projectile GameObject and the explosion
                var playerHeadObject = _playerVisual.transform.GetChild(playerHeadIndex).gameObject;

                var thisObjectHeight = transform.position.y;
                var playerHeadObjectSize = playerHeadObject.GetComponent<Renderer>().bounds.size;
                var playerHeadObjectHeightCenter = playerHeadObject.transform.position.y + (playerHeadObjectSize.y * 0.45f); // 0.45 is exact location of cannon in head
                var playerHeadObjectWidth = playerHeadObjectSize.z;
                var cannonHeight = playerHeadObjectHeightCenter - thisObjectHeight;
                
                Vector3 launchPosition = new Vector3(transform.position.x, cannonHeight, transform.position.z);
                Vector3 forwardOffset = transform.rotation * Vector3.forward * (playerHeadObjectWidth * 0.5f);
                Vector3 explosionForwardOffset = transform.rotation * Vector3.forward * (playerHeadObjectWidth * 0.7f);
                Quaternion playerRotation = transform.rotation;

                SpawnProjectileServerRpc(launchPosition, forwardOffset, explosionForwardOffset, playerRotation);

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

    [ServerRpc]
    private void SpawnProjectileServerRpc(Vector3 launchPosition, Vector3 forwardOffset, Vector3 explosionForwardOffset, Quaternion playerRotation)
    {
        var projectile = Instantiate(_projectilePrefab, launchPosition + forwardOffset, playerRotation);
        var projectileScript = projectile.GetComponent<ShellProjectile>();
        projectile.GetComponent<NetworkObject>().SpawnWithOwnership(_playerData.ClientId);
        StartCoroutine(StartProjectileDespawnTimer(projectileScript));
        var explosion = Instantiate(_projectileExplosionPrefab, launchPosition + explosionForwardOffset, transform.rotation);
        explosion.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator StartProjectileDespawnTimer(ShellProjectile projectile) {
        yield return new WaitForSeconds(3);
        if(!projectile.isDestroyed) {
            projectile.DespawnServerRpc();
        }
    }
}
