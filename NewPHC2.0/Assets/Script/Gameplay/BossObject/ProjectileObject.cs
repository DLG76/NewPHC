using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileObject : MonoBehaviour
{
    [SerializeField] private Sprite[] allSprites;

    private Rigidbody2D _rigidbody;

    private List<System.Type> statusEffects = new List<System.Type>();
    private float damage;
    private bool setted = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (allSprites.Length > 0)
            GetComponent<SpriteRenderer>().sprite = allSprites[Random.Range(0, allSprites.Length)];
    }

    public void Setup(float damage, Vector2 velocity, float stayTime, float speed) => StartCoroutine(SetupIE(damage, velocity, stayTime, speed));

    public void AddStatusEffectOnHit<T>() where T : StatusEffect
    {
        statusEffects.Add(typeof(T));
    }

    private IEnumerator SetupIE(float damage, Vector2 velocity, float stayTime, float speed)
    {
        this.damage = damage;

        var guideLine = CreateGuideLine(velocity);

        yield return new WaitForSeconds(stayTime);

        Destroy(guideLine);

        _rigidbody.velocity = velocity.normalized * speed * 35;

        Destroy(gameObject, 15);

        setted = true;
    }

    private GameObject CreateGuideLine(Vector2 velocity)
    {
        var guideLine = Instantiate(Resources.Load<Transform>("GuideLine/RedBox"));

        var stonePos = transform.position;
        var direction = velocity.normalized.ConvertTo<Vector3>();

        float distance = 100;

        guideLine.localScale = new Vector3(distance, 1, 1);
        guideLine.position = stonePos + (direction * (distance / 2f));
        guideLine.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        return guideLine.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!setted) return;

        if (collision.TryGetComponent(out PlayerCombat player))
        {
            var myPos = transform.position;
            var plrPos = player.transform.position;
            var direction = (plrPos - myPos).normalized;

            player.TakeDamage(damage);
            player.ApplyForce(direction, 3);

            foreach (var statusEffect in statusEffects)
            {
                var method = typeof(PlayerCombat).GetMethod("AddStatusEffect");
                var genericMethod = method.MakeGenericMethod(statusEffect);
                genericMethod.Invoke(player, null);
            }

            Destroy(gameObject);
        }
    }
}
