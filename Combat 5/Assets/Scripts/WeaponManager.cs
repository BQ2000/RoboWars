using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeaponManager : NetworkBehaviour {

    [SerializeField]
    Weapon primaryWeapon;
    Weapon currentWeapon;

    // Start is called before the first frame update
    void Start() {
        EquipWeapon(primaryWeapon);
    }

    private void EquipWeapon(Weapon weapon) {
        currentWeapon = weapon;
    }

    Weapon getCurrentWeapon() {
        return currentWeapon;
    }

    // Update is called once per frame
    void Update() {

    }
}
