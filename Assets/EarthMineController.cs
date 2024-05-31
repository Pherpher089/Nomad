using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class EarthMineController : MonoBehaviour
{

    Vector3 target;
    bool go = false;
    PhotonView pv;
    float t = 0;
    Vector3 startPoint;
    float distance;
    float y = 9;
    public int damage = 10;
    GameObject m_Owner;
    GameObject m_Weapon;
    public void Initialize(Vector3 _target, GameObject _owner, GameObject _weapon)
    {
        pv = GetComponent<PhotonView>();
        target = _target;
        startPoint = transform.position;
        distance = Vector3.Distance(_target, startPoint);
        go = true;
        m_Owner = _owner;
        m_Weapon = _weapon;
    }

    void Update()
    {

        if (go)
        {
            y = Mathf.Lerp(9, 0, t);
            Vector3 _target = new Vector3(target.x, target.y + y, target.z);
            Vector3 modifier = new Vector3(0, y, 0);
            transform.position = Vector3.Slerp(startPoint, _target, t);
            t += Time.deltaTime * 2;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.name.Contains("Grass")) return;
        if (other.gameObject.name.Contains("HitBox")) return;

        if (!GameStateManager.Instance.friendlyFire && other.gameObject.CompareTag("Player")) return;
        if (other.gameObject.CompareTag("Tool")) return;
        if (other.tag is "Tool" or "HandSocket" or "Beast" or "DoNotLand" or "Item")
        {
            return;
        }
        if (other.TryGetComponent(out Item item))
        {
            if (item.isEquipped) return;
        }
        if (!pv.IsMine)
        {
            Destroy(this.gameObject);
            return;
        }

        if (other.gameObject.CompareTag("Enemy"))
        {
            GameObject explostion = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "FireBallExplosion"), transform.position + transform.forward, Quaternion.LookRotation(transform.forward));

            explostion.GetComponent<FireBallExplosionControl>().Initialize(m_Owner, m_Weapon);
            PhotonNetwork.Destroy(this.gameObject);
        }
        go = false;
    }
}
