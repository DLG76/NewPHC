using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class VoidItemDrop : MonoBehaviour, ICollectable
{
    public static List<VoidItemDrop> items = new List<VoidItemDrop>();

    public UnityAction<PlayerCombat> onItemCollected;
    public VoidItem Item { get => _item; }
    private VoidItem _item;
    private PlayerCombat player;

    private Rigidbody2D _rigidbody;

    private float startSpeed = 7.5f;
    private float startRotation = 45;
    private float autoCollectTime = 2;
    private bool setted = false;
    private bool collected = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _rigidbody.freezeRotation = true;
        _rigidbody.gravityScale = 0;
    }

    public void Setup(VoidItem item)
    {
        if (item.voidPrefab != null)
            Instantiate(Resources.Load<GameObject>("Effect/ItemDropEffect1"), transform.position, transform.rotation, transform);

        _item = item;

        name = _item.Name + _item.GetHashCode() + "Drop";

        startRotation *= Random.Range(-1f, 1f);

        StartCoroutine(RandomForceIE());

        items.RemoveAll(i => i == null || i.gameObject == null);
        items.Add(this);
    }

    private IEnumerator RandomForceIE()
    {
        var direction = Random.insideUnitCircle.normalized;

        _rigidbody.velocity = direction * 7.5f;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        yield return new WaitForSeconds(0.25f);

        _rigidbody.velocity = Vector2.zero;
        setted = true;

        yield return new WaitForSeconds(autoCollectTime);

        if (_item.Owner.Character != null)
            MoveTo(_item.Owner.Character);
    }

    public void MoveTo(PlayerCombat player)
    {
        this.player = player;
    }

    private void Update()
    {
        if (player != null)
        {
            Vector2 direction = (player.transform.position + player.Offset - transform.position).normalized;

            _rigidbody.velocity = Quaternion.Euler(0, 0, startRotation) * direction * startSpeed * 3.5f;

            startSpeed += Time.deltaTime;
            startRotation = Mathf.Lerp(startRotation, 0, Time.deltaTime * 1.5f);
        }
    }

    public void Collect()
    {
        if (collected) return;

        collected = true;

        AudioManager.Instance?.PlaySound("CollectItem", 0.1f, 0.7f);

        onItemCollected?.Invoke(_item.Owner.Character);
        VoidHotbarUI.Instance.AddItem(_item);

        var player = _item.Owner.Character;

        if (player != null)
        {
            player.ChangeMaterial(Resources.Load<Material>("Material/ItemGlowMaterial"), Color.white, 0.1f, 0);

            var itemDropEffect2 = Instantiate(Resources.Load<GameObject>("Effect/ItemDropEffect2"), transform.position + player.Offset.ConvertTo<Vector3>(), Quaternion.identity);

            Destroy(itemDropEffect2, 5);

            CameraController.Instance?.TriggerShake(0.075f, 0.15f, 0.2f);
        }

        items.Remove(this);

        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.CompareTag("Wall") || collision.CompareTag("Enemy")) && !setted)
        {
            _rigidbody.velocity = Vector2.zero;
        }
    }
}
