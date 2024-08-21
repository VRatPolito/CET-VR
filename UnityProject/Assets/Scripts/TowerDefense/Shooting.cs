using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    #region input
    InputManagement _input;
    #endregion

    #region gun properties
    bool canShoot = true; //user can only shoot when the first round begins
    bool _shooting = false;

    [SerializeField]
    GameObject gun, shootingPoint, bullet, munitionCounter, precisionIndicator;
    [SerializeField]
    LineRenderer laserPointer;
    public GameObject bullets; //empty parent of bullets
    public Bullet_Behaviour[] _bulletPool; //finite number of projectiles to avoid sudden instantiations
    private int _totalBullets = 60;
    private int _currentBullet; //index of the current bullet in the buffer
    [SerializeField]
    private float _laserLength = 15f;
    #endregion

    TowerDefenseLevelManager _manager;
    [SerializeField]
    GameObject leftController, rightController;
    internal bool _isLeftHanded = false;
    RaycastHit hit;

    #region reload

    [SerializeField]
    int _maxHeat = 15; //maximum number of bullets that can be fired before overheating
    AudioSource _reloadSound;

    #region heat 
    [SerializeField]
    private int heat; //heat of the weapon
    [SerializeField]
    private float lastShotTime; //moment of the last shot
    [SerializeField]
    private float bulletCooldownTime = 1f; //heat cooling time at maxheat
    [SerializeField]
    private float coolDownTime = 0.2f; //heat cooling time of a projectile
    bool heatReloading = false;

    [SerializeField]
    GameObject heatColorObj, heatImageMask;
    [SerializeField]
    AudioSource overHeatSound;
    #endregion

    #endregion
    #region platform
    [SerializeField]
    GameObject platform;
    [SerializeField]
    GameObject PlayerCam;
    [SerializeField]
    Collider platformBorderCollider;
    bool borderHit = false;
    internal bool firingEnabled = false;

    #endregion
    private void Awake()
    {

        if (GameManager.Instance != null)
            _isLeftHanded = GameManager.Instance._isLeftHanded;

        if(_isLeftHanded)
        {
            var gunLocalStartPos = gun.transform.localPosition;
            var gunLocalStartRot = gun.transform.localEulerAngles;
            gun.transform.parent = leftController.transform;
            gun.transform.localPosition = gunLocalStartPos;
            gun.transform.localEulerAngles = gunLocalStartRot;
            Vector3 _newPos = munitionCounter.transform.localPosition;
            _newPos.x *= -1; //opposite side of the gun
            munitionCounter.transform.localPosition = _newPos;
            _newPos = precisionIndicator.transform.localPosition;
            _newPos.x *= -1;
            precisionIndicator.transform.localPosition = _newPos;
        }

        _input = GetComponent<InputManagement>();
        _input.OnLeftTriggerClicked += LeftTriggerClicked;
        _input.OnRightTriggerClicked += RightTriggerClicked;
        if(GameManager.Instance != null)
            _manager = (TowerDefenseLevelManager) GameManager.Instance.LevelManager;
        else
            _manager = FindObjectOfType<TowerDefenseLevelManager>();
        #region bulletSettings
        _currentBullet = 0;

        _bulletPool = new Bullet_Behaviour[_totalBullets];
        GameObject parent = Instantiate(bullets, Vector3.zero, Quaternion.identity);
        parent.name = "Bullets";
        for (int i = 0; i < _totalBullets; i++)
        {
            GameObject b = Instantiate(bullet, Vector3.zero, Quaternion.identity);
            b.tag = "Bullet";
            b.name = "Bullet_" + i.ToString();
            _bulletPool[i] = b.GetComponent<Bullet_Behaviour>();
            _bulletPool[i].GetComponent<MeshRenderer>().enabled = false; //bullets hidden by default, until used
            _bulletPool[i].transform.parent = shootingPoint.transform;
            _bulletPool[i].gameObject.SetActive(false);

            //no projectile collisions with player
            if (GameManager.Instance != null) 
                Physics.IgnoreCollision(_bulletPool[i].GetComponent<Collider>(), GameManager.Instance.LevelManager.Player.GetComponent<CharacterController>());

            Physics.IgnoreCollision(_bulletPool[i].GetComponent<Collider>(), gun.gameObject.GetComponent<Collider>());
            Physics.IgnoreCollision(_bulletPool[i].GetComponent<Collider>(), platform.gameObject.GetComponent<Collider>());
        }
        #endregion

        #region laserSettings
        Vector3 _laserEndPosition = laserPointer.GetComponent<LineRenderer>().GetPosition(1);
        Vector3 _laserStartPosition = laserPointer.GetComponent<LineRenderer>().GetPosition(0);
        _laserEndPosition.z = _laserStartPosition.z + _laserLength;
        laserPointer.GetComponent<LineRenderer>().SetPosition(1, _laserEndPosition);
        laserPointer.GetComponent<LineRenderer>().startWidth = 0.006f;
        laserPointer.GetComponent<LineRenderer>().endWidth = 0.006f;
        laserPointer.GetComponent<LineRenderer>().enabled = false; //disabled by default
        #endregion

        #region reloadSound Settings
        _reloadSound = gun.gameObject.GetComponent<AudioSource>();
        _reloadSound.spatialBlend = 1f;
        _reloadSound.spatialize = true;
        _reloadSound.spread = 360f;
        #endregion

        #region heatSettings
        heat = 0;
        lastShotTime = 0;
        heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        heatColorObj.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        #endregion
    }

    private void RightTriggerClicked(object obj)
    {
        if (!_isLeftHanded && firingEnabled && canShoot && !borderHit)
            {//if can shoot
                FireWeapon();
            }
    }

    private void LeftTriggerClicked(object obj)
    {
        if (_isLeftHanded && firingEnabled && canShoot && !borderHit)
        {//if can shoot
            FireWeapon();
        }
    }

    public void ShowGun()
    {
        gun.gameObject.SetActive(true);
        if (_isLeftHanded)
            _manager.HideLeftController();
        else
            _manager.HideRightController();
        firingEnabled = true;
    }

    public void HideGun()
    {
        //hide the gun
        gun.gameObject.SetActive(false);
        if (_isLeftHanded)
            _manager.ShowLeftController();
        else
            _manager.ShowRightController();
        firingEnabled = false;
    }

    void Update()
    {
        #region gun towards glass -> can't shoot
        borderHit = false;
        var hits = Physics.RaycastAll(new Ray(laserPointer.transform.position, laserPointer.transform.forward));

        foreach (var hit in hits)
        {
            if (hit.collider == platformBorderCollider)
            {
                borderHit = true;
                break;
            }
        }
        #endregion

        //the laser is only visible when the user can shoot
        enableLaserPointer(canShoot && firingEnabled && !borderHit);

        #region heat cooldown
        #region gradual cooldown
        if (bullet!=null)
        if(heat >0 && heat < _maxHeat && canShoot)
        {
            if (Time.time >= lastShotTime + bulletCooldownTime) //if bulletCooldownTime has passed since the last shot, start cooling
                {
                heat--;
                float perc = (float)heat / (float)_maxHeat;
                heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
                heatColorObj.gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.green, Color.red, perc);

                lastShotTime = Time.time;
            }

        }
        #endregion
        #region overheat
        else if (heat == _maxHeat || heatReloading)
        {
            canShoot = false;
            if (heat == _maxHeat && !heatReloading)
            {
                overHeatSound.Play();
                heatReloading = true;
            }

            if (heat > 0 && Time.time>=lastShotTime + coolDownTime)
            {
                heat--;
                heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.red;  
                float perc = (float)heat / (float)_maxHeat; 
                heatColorObj.gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.green, Color.red, perc);
                lastShotTime = Time.time;
            }
            if (heat == 0)
            {
                canShoot = true;
                heatReloading = false;
                heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            }
        }
        #endregion
        #endregion
    }

    void FireWeapon()
    {
        float perc;
        if (bullet != null && heat<=_maxHeat)
        {
            Bullet_Behaviour bulletInstance = _bulletPool[_currentBullet % _totalBullets];
            bulletInstance.gameObject.SetActive(true);
            bulletInstance.transform.parent = shootingPoint.transform;

            bulletInstance.gameObject.GetComponent<SphereCollider>().enabled = true;
            bulletInstance.GetComponent<Bullet_Behaviour>().Shoot(shootingPoint);

            _currentBullet++;
            heat++;

            heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
            perc = (float)heat / (float)_maxHeat;
            heatColorObj.gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.green, Color.red, perc);
            lastShotTime = Time.time;
        }
    }

    public void resetBullets()
    {
        heat = 0;
        lastShotTime = 0;
        heatReloading = false; 
        heatImageMask.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        heatColorObj.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
    }
    public void enableLaserPointer(bool isEnabled)
    {
        laserPointer.GetComponent<LineRenderer>().enabled = isEnabled; //activate/deactivate laser
    }


}
