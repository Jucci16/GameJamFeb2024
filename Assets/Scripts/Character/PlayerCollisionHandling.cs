using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCollisionHandling : NetworkBehaviour
{   
    [SerializeField]
    // Used for testing purposes only.
    public string overridePlayerId = null;

    [SerializeField]
    private GameObject _explosionPrefab; 

    private PlayerData _playerData;

    void Start()
    {
        _playerData = MultiplayerManager.Instance.GetPlayerDataFromClientId(OwnerClientId);
    }

    void OnTriggerEnter(Collider projectile)
    {
        if(projectile.gameObject.tag == ("Projectile"))
        {
            var projectileObject = projectile.gameObject.GetComponent<IProjectile>();
            // Only take action if the bullet was not shot by the current player
            string playerId = string.IsNullOrEmpty(overridePlayerId) ? _playerData.PlayerId.ToString() : overridePlayerId;
            if(projectileObject.originPlayerId.ToString() != playerId) {
                Destroy(projectile.gameObject);
                var explosion = Instantiate(_explosionPrefab, transform.position, transform.rotation);
                Destroy(explosion, 3);
                PlayerDeath();
            }
        }
    }

    private void PlayerDeath() {
        Destroy(this.gameObject);
    }
}
