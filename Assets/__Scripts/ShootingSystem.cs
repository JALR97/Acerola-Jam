using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShootingSystem : MonoBehaviour
{
    //Component
    [SerializeField] private PlayerControl _playerControl;

    [SerializeField] private Vision viewCone;
    [SerializeField] private Vision roundView;

    [SerializeField] private DrawManager _drawManager;
    [SerializeField] private LayerMask targetMask;
    
    //Balance Vars
    [SerializeField] private int maxAmmo;
    [SerializeField] private int shotDamage;
    [SerializeField] private float shotRange;
    [SerializeField] private float aimAngleChange;
    [SerializeField] private float aimLineAngleChange = 25;
    [SerializeField] private float viewRangeChange;
    [SerializeField] private float areaRangeChange;
    [SerializeField] private float aimTime = 2.0f;
    [SerializeField] private float aimLineStart = 1;
    [SerializeField] private float aimLineEnd = 5;
    [SerializeField] private float aimLineAngle = 15;
    
    
    //Private vars
    private int _currentAmmo;
    private int _roundsInMag;//for future use if reloading is added

    private float defaultViewAngle;
    private float defaultViewRange;
    private float defaultAreaRange;
    private float defaultAimAngle;

    private bool aiming = false;
    
    //Public Functions
    public bool hasAmmo(int qty = 1) {
        if (qty <= 0){
            Debug.Log("Shouldn't be asking for negative ammo");
            return _currentAmmo > 0;
        }
        else
            return _currentAmmo >= qty;
    }

    public void Shoot(int ammo) {
        if (!hasAmmo(ammo)) {
            //Not enough ammo
            return;
        }
        _currentAmmo -= ammo;
        //Shooting sound
        //Shoot raycast behavior
    }

    public void GrabAmmo(int qty) {
        _currentAmmo = Mathf.Min(maxAmmo, _currentAmmo + qty);
    }
    
    //Private
    private IEnumerator AimDownLerp() {
        float startViewAngle = viewCone.viewAngle;
        float startViewRange = viewCone.viewRange;
        float startAreaRange = roundView.viewRange;
        float startAimAngle = defaultAimAngle;
        
        float targetViewAngle = defaultViewAngle - aimAngleChange;
        float targetViewRange = defaultViewRange + viewRangeChange;
        float targetAreaRange = defaultAreaRange - areaRangeChange;
        float targetAimAngle = defaultAimAngle - aimLineAngleChange;
        
        float timeElapsed = 0;

        while (timeElapsed < aimTime) {
            var t = timeElapsed / aimTime;
            viewCone.viewAngle = Mathf.Lerp(startViewAngle, targetViewAngle, t);
            viewCone.viewRange = Mathf.Lerp(startViewRange, targetViewRange, t);
            roundView.viewRange = Mathf.Lerp(startAreaRange, targetAreaRange, t);
            aimLineAngle = Mathf.Lerp(startAimAngle, targetAimAngle, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        viewCone.viewAngle = targetViewAngle;
        viewCone.viewRange = targetViewRange;
        roundView.viewRange = targetAreaRange;
        aimLineAngle = targetAimAngle;
    }
    private IEnumerator AimUpLerp() {
        float startViewAngle = viewCone.viewAngle;
        float startViewRange = viewCone.viewRange;
        float startAreaRange = roundView.viewRange;
        float startAimAngle = aimLineAngle;
        
        float targetViewAngle = defaultViewAngle;
        float targetViewRange = defaultViewRange;
        float targetAreaRange = defaultAreaRange;
        float targetAimAngle = defaultAimAngle;
        
        float timeElapsed = 0;
        while (timeElapsed < aimTime) {
            var t = timeElapsed / aimTime;
            viewCone.viewAngle = Mathf.Lerp(startViewAngle, targetViewAngle, t);
            viewCone.viewRange = Mathf.Lerp(startViewRange, targetViewRange, t);
            roundView.viewRange = Mathf.Lerp(startAreaRange, targetAreaRange, t);
            aimLineAngle = Mathf.Lerp(startAimAngle, targetAimAngle, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        viewCone.viewAngle = targetViewAngle;
        viewCone.viewRange = targetViewRange;
        roundView.viewRange = targetAreaRange;
        aimLineAngle = targetAimAngle;
    }

    private void Start() {
        defaultViewAngle = viewCone.viewAngle;
        defaultViewRange = viewCone.viewRange;
        defaultAreaRange = roundView.viewRange;
        defaultAimAngle = roundView.viewRange;
    }
    
    private void Update() {
        //Start to aim
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _playerControl.AimSwitch();
            StopAllCoroutines();
            StartCoroutine(nameof(AimDownLerp));
            _drawManager.DrawLines();
            aiming = true;
        }
        
        //Stop aiming
        if (Input.GetKeyUp(KeyCode.Mouse1)) { 
            _playerControl.AimSwitch();
            StopAllCoroutines();
            StartCoroutine(nameof(AimUpLerp));
            _drawManager.ClearLines();
            aiming = false;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && aiming) {//Need to add fire rate limit, not free to mouse click
            float shotAngle = Random.Range(-viewCone.viewAngle / 2 + aimLineAngle,
                                            viewCone.viewAngle / 2 - aimLineAngle);
            Vector3 shotVector = viewCone.DirFromAngle(shotAngle, false);
            var hit = Physics2D.Raycast(transform.position, shotVector, shotRange, targetMask);
            if (hit.collider != null) {
                //Hit a target
                var enemy = hit.transform;
                enemy.GetComponent<Health>().Damage(shotDamage);
                //Knockback
                //
            }
        }
    }

    private void LateUpdate() {
        if (Input.GetKey(KeyCode.Mouse1)) {
            Vector3 viewAngleA = viewCone.DirFromAngle(-viewCone.viewAngle / 2 + aimLineAngle, false);
            Vector3 viewAngleB = viewCone.DirFromAngle(viewCone.viewAngle / 2 - aimLineAngle, false);

            var position = viewCone.transform.position;
            _drawManager.UpdateLine(1, position + viewAngleA * aimLineStart, position + viewAngleA * aimLineEnd);
            _drawManager.UpdateLine(2, position + viewAngleB * aimLineStart, position + viewAngleB * aimLineEnd);
        }
    }
}
