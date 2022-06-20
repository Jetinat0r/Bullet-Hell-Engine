using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Projectile baseProjectile;
    public List<Projectile> instantiatedProjectiles = new List<Projectile>();

    private void Awake()
    {
        Projectile newProjectile = Instantiate(baseProjectile);

        BoxCollider2D bc = newProjectile.gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;

        newProjectile.SetupNewPrefab("Test", _color: Color.red, _colliderType:ColliderType.BOX);

        instantiatedProjectiles.Add(newProjectile);
        newProjectile.gameObject.SetActive(false);

        Projectile x = Instantiate(instantiatedProjectiles[0]);
        x.gameObject.SetActive(true);
        x.name = "Awake";
    }

    private void Start()
    {
        Projectile x = Instantiate(instantiatedProjectiles[0]);
        x.gameObject.SetActive(true);
        x.name = "Start";
    }

    //For some reason if spawned at start doesn't actually remove the circle collider.
    //It does, however, disable it so it probably wont be a problem
    //I'm assuming the component is disabled the same frame, then deleted the next frame
    //Looked it up, object destruction is reserved for after the current update loop, and I assume that Awake and Start are part of the same loop
    //  Luckily, I should never have projectiles being instantiated as soon as they are added to the dictionary
    bool hasspawned = false;
    private void Update()
    {
        if (!hasspawned)
        {
            Projectile x = Instantiate(instantiatedProjectiles[0]);
            x.gameObject.SetActive(true);
            x.name = "Update";

            hasspawned = true;
        }
    }
}
