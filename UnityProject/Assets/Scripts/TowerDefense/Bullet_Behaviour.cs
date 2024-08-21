using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bullet_Behaviour : MonoBehaviour
{
    public float bulletSpeed = 400f;
    AudioSource _shotSound;
    public UnityEvent OnBulletFired;
    public UnityEvent OnTargetHit;
    void Start()
    {
        _shotSound = this.gameObject.GetComponent<AudioSource>();
    }


    public void Shoot(GameObject spawnPoint)
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = true; //make visible
        //starting position and rotations of the projectile
        // this.gameObject.transform.position = spawnPoint.gameObject.transform.position;
        this.gameObject.transform.localPosition = Vector3.zero;
        this.gameObject.transform.rotation = Quaternion.identity;
        
        ResetRigidBody();
        OnBulletFired?.Invoke();

        this.GetComponent<Rigidbody>().AddForce(bulletSpeed * spawnPoint.gameObject.transform.forward);

        this.transform.parent = null;
        if (_shotSound != null)
            _shotSound.Play();


    }
    bool once = true;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") || collision.gameObject.CompareTag("FlyingTarget"))
        {

            //Physics.IgnoreCollision(this.gameObject.GetComponent<Collider>(), collision.gameObject.GetComponent<Collider>());
            if (once)
            {
                OnTargetHit?.Invoke();
                once = false;
            }
            //bring the bullet back to the origin and make it invisible
            StartCoroutine(resetBullet());

        }
        else //shot missed or against an obstacle
        {
            //if(collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Obstacle"))
           // {
                StartCoroutine(resetBullet());
           // }
        }
        if (!this.gameObject.GetComponent<Rigidbody>().isKinematic)
        {
            if(!collision.gameObject.CompareTag("Tower") && !collision.gameObject.CompareTag("Bullet") && !collision.gameObject.CompareTag("Ground"))
                Debug.Log("Collision with: " + collision.gameObject.name + "\n");
        }
            

    }
    

    private void ResetRigidBody()
    {
        this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero; 
    }

    IEnumerator resetBullet()
    {
        ResetRigidBody();
        this.transform.position = Vector3.zero;
        this.gameObject.GetComponent<SphereCollider>().enabled = false;
        this.gameObject.GetComponent<MeshRenderer>().enabled = false; //make it invisible
        yield return new WaitForSeconds(_shotSound.clip.length);
        this.gameObject.SetActive(false);
        once = true;
        yield return new WaitForSeconds(0);
    }
}
