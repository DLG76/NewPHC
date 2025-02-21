using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeCanvas : Singleton<FadeCanvas>
{
    [SerializeField] private Transform content;
    [SerializeField] private Gradient color;
    private static List<Box> boxs = new List<Box>();
    [SerializeField] private int scale = 7;
    [SerializeField] private float createBoxTick = 0.01f;

    private bool exiting = false;

    public IEnumerator ExitFade(string scene)
    {
        if (exiting) yield break;

        exiting = true;

        boxs.Clear();

        float width = Screen.width;
        float height = Screen.height;

        float boxSize = Mathf.Ceil(height / scale);

        for (float ver = 0; ver < scale; ver++)
        {
            for (float hor = 0; hor < Mathf.Ceil(width / boxSize); hor++)
            {
                var box = new Box(hor * boxSize, ver * boxSize, boxSize, boxSize, color.Evaluate(Random.value));

                boxs.Add(box);
            }
        }

        System.Random random = new System.Random();
        boxs = boxs.OrderBy(x => random.Next()).ToList();

        CameraController.Instance.TriggerShake(0.06f, boxs.Count * createBoxTick, 0.4f);

        foreach (var box in boxs)
        {
            box.CreateBox(content, true);

            yield return new WaitForSeconds(createBoxTick);
        }

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(scene);

        exiting = false;
    }

    public IEnumerator EnterFade()
    {
        if (boxs.Count == 0) yield break;

        List<Image> boxImgs = new List<Image>();

        foreach (var box in boxs)
        {
            var boxImg = box.CreateBox(content, false);

            boxImgs.Add(boxImg);
        }

        boxs.Clear();

        foreach (var boxImg in boxImgs)
        {
            var boxColor = boxImg.color;
            boxColor.a = 0;

            boxImg.DOColor(boxColor, 0.4f);

            yield return new WaitForSeconds(createBoxTick);
        }

        yield return new WaitForSeconds(1);

        foreach (var boxImg in boxImgs)
            Destroy(boxImg.gameObject);
    }

    private class Box
    {
        public float x { get; private set; }
        public float y { get; private set; }
        public float h { get; private set; }
        public float w { get; private set; }
        public Color c { get; private set; }

        public Box(float x, float y, float h, float w, Color c)
        {
            this.x = x;
            this.y = y;
            this.h = h;
            this.w = w;
            this.c = c;
        }

        public Image CreateBox(Transform content, bool isAnimation)
        {
            if (content != null)
            {
                var boxObj = Instantiate(Resources.Load<RectTransform>("UI/Box"), content);
                var boxImg = boxObj.GetComponent<Image>();
                boxObj.sizeDelta = new Vector2(h, w);
                boxObj.anchoredPosition = new Vector2(x + (w / 2f), y + (h / 2f));

                if (isAnimation)
                {
                    var boxEffect = Instantiate(Resources.Load<RectTransform>("UI/Box"), content);
                    var boxEffectImg = boxObj.GetComponent<Image>();

                    boxEffect.sizeDelta = new Vector2(h, w);
                    boxEffect.anchoredPosition = new Vector2(x + (w / 2f), y + (h / 2f));
                    boxEffectImg.color = Color.white;

                    boxEffect.DOSizeDelta(boxEffect.sizeDelta * 1.15f, 0.5f);

                    Destroy(boxEffect.gameObject, 1);

                    Color startColor = Color.white;

                    boxImg.color = startColor;

                    boxImg.DOColor(c, 0.4f);
                }
                else boxImg.color = c;

                return boxImg;
            }

            return null;
        }
    }
}
