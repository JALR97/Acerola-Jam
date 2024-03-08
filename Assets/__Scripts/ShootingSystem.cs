using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShootingSystem : MonoBehaviour
{
    //Component
    [SerializeField] private PlayerControl _playerControl;

    [SerializeField] private Vision viewCone;
    [SerializeField] private Vision roundView;
    
    //Balance Vars
    [SerializeField] private int maxAmmo;
    [SerializeField] private float shotDamage;
    [SerializeField] private float aimAngleChange;
    [SerializeField] private float viewRangeChange;
    [SerializeField] private float areaRangeChange;
    [SerializeField] private float aimTime = 2.0f;
    
    //Private vars
    private int _currentAmmo;
    private int _roundsInMag;//for future use if reloading is added

    private float defaultViewAngle;
    private float defaultViewRange;
    private float defaultAreaRange;
    
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
        
        float targetViewAngle = defaultViewAngle - aimAngleChange;
        float targetViewRange = defaultViewRange + viewRangeChange;
        float targetAreaRange = defaultAreaRange - areaRangeChange;

        float timeElapsed = 0;

        while (timeElapsed < aimTime) {
            var t = timeElapsed / aimTime;
            viewCone.viewAngle = Mathf.Lerp(startViewAngle, targetViewAngle, t);
            viewCone.viewRange = Mathf.Lerp(startViewRange, targetViewRange, t);
            roundView.viewRange = Mathf.Lerp(startAreaRange, targetAreaRange, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        viewCone.viewAngle = targetViewAngle;
        viewCone.viewRange = targetViewRange;
        roundView.viewRange = targetAreaRange;
    }
    private IEnumerator AimUpLerp() {
        float startViewAngle = viewCone.viewAngle;
        float startViewRange = viewCone.viewRange;
        float startAreaRange = roundView.viewRange;
        
        float targetViewAngle = defaultViewAngle;
        float targetViewRange = defaultViewRange;
        float targetAreaRange = defaultAreaRange;
        
        float timeElapsed = 0;
        while (timeElapsed < aimTime) {
            var t = timeElapsed / aimTime;
            viewCone.viewAngle = Mathf.Lerp(startViewAngle, targetViewAngle, t);
            viewCone.viewRange = Mathf.Lerp(startViewRange, targetViewRange, t);
            roundView.viewRange = Mathf.Lerp(startAreaRange, targetAreaRange, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        viewCone.viewAngle = targetViewAngle;
        viewCone.viewRange = targetViewRange;
        roundView.viewRange = targetAreaRange;
    }

    private void Start() {
        defaultViewAngle = viewCone.viewAngle;
        defaultViewRange = viewCone.viewRange;
        defaultAreaRange = roundView.viewRange;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            _playerControl.AimSwitch();
            StopAllCoroutines();
            StartCoroutine(nameof(AimDownLerp));
        }
        if (Input.GetKeyUp(KeyCode.Mouse1)) {
            _playerControl.AimSwitch();
            StopAllCoroutines();
            StartCoroutine(nameof(AimUpLerp));
        }
    }
}
