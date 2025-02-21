using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowingStatusEffect : StatusEffect
{
    public override bool CanStack => false;

    private List<Transform> slowingEffects = new List<Transform>();

    public override void Setup(float tickTime, int maxTickCount, float point)
    {
        character.MultiplySpeed(point);

        AudioManager.Instance?.PlaySound("Chain", 0.1f, Random.Range(0.8f, 1.2f));

        var chaPos = character.transform.position + character.Offset;

        var slowingEffectModel = Resources.Load<Transform>("Effect/SlowingEffectEntity");

        var chainSize = slowingEffectModel.localScale.y * slowingEffectModel.GetComponentsInChildren<SpriteRenderer>().Length * 0.25f;
        var mainStartChainPos = Random.onUnitSphere;

        for (int i = 0; i < 3; i++)
        {
            var startChainPos = chaPos + (Quaternion.Euler(0, 0, i * 120) * mainStartChainPos * chainSize);

            var ray = Physics2D.Raycast(chaPos, startChainPos - chaPos, chainSize, LayerMask.GetMask("Wall"));

            if (ray.collider != null)
                startChainPos = ray.point;

            var direction = startChainPos - chaPos;
            direction.z = 0;
            direction.Normalize();

            var slowingEffect = Instantiate(slowingEffectModel, chaPos, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            var startChain = slowingEffect.Find("Start");
            var startChainHJ = startChain.GetComponent<HingeJoint2D>();
            var endChain = slowingEffect.Find("End");
            var endChainHJ = endChain.GetComponent<HingeJoint2D>();

            startChainHJ.autoConfigureConnectedAnchor = false;
            startChainHJ.connectedBody = null;
            startChainHJ.connectedAnchor = startChainPos;
            endChainHJ.autoConfigureConnectedAnchor = false;
            endChainHJ.connectedBody = character.GetComponent<Rigidbody2D>();
            endChainHJ.connectedAnchor = character.Offset;

            float auraAdd = Random.Range(0, 100);

            for (int c = 0; c < slowingEffect.childCount; c++)
            {
                Transform chain = slowingEffect.GetChild(c);
                var spriteRenderer = chain.GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    var propBlock = new MaterialPropertyBlock();

                    spriteRenderer.GetPropertyBlock(propBlock);
                    propBlock.SetFloat("_AuraAdd", auraAdd + (c * 2.5f));
                    spriteRenderer.SetPropertyBlock(propBlock);
                }
            }

            slowingEffects.Add(slowingEffect);
        }

        onTick += (isEnd) =>
        {
            if (isEnd)
            {
                character.MultiplySpeed(1 / point);
                RemoveAllChains();
            }
        };

        base.Setup(tickTime, maxTickCount, point);
    }

    protected override void CreateEffect()
    {
 
    }

    protected override void Tick()
    {

    }

    private void OnDestroy()
    {
        RemoveAllChains();
    }

    private void RemoveAllChains()
    {
        slowingEffects.RemoveAll(ef => ef == null || ef.gameObject == null);

        if (slowingEffects.Count > 0)
        {
            foreach (var slowingEffect in slowingEffects)
                Destroy(slowingEffect.gameObject);
        }
    }
}
