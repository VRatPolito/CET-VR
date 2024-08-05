using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Coin : MonoBehaviour
{
    [SerializeField]
    Coin _nextCoin;
    [SerializeField]
    float _rotationSpeed = 50;
    public UnityEvent OnCoinCollected;
    // Start is called before the first frame update
  
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(_nextCoin != null)
                _nextCoin.gameObject.SetActive(true);
            gameObject.SetActive(false);
            OnCoinCollected?.Invoke();
        }
    }
    private void Update()
    {
        transform.rotation *= Quaternion.Euler(Vector3.up * _rotationSpeed * Time.deltaTime);
    }
    public Transform getNextCoinTransform()
    {
        if (_nextCoin != null)
            return _nextCoin.gameObject.transform;
        else
            return null;
    }
}
