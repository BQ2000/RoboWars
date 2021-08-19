using UnityEngine;

[System.Serializable]
public class Weapon : MonoBehaviour {
    [SerializeField] private string weaponName = "Boomer";
    [SerializeField] private int damage = 1;
    [SerializeField] private float range = 100f;

    public ParticleSystem muzzleFlashL;
    public ParticleSystem muzzleFlashR;

    public GameObject hitEffectPrefab;
    public GameObject sprayPrefab;

    public AudioSource shootSFXL;
    public AudioSource shootSFXR;
    public AudioSource spraySFX;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public float getRange() {
        return this.range;
    }

    public int getDamage() {
        return this.damage;
    }
}
