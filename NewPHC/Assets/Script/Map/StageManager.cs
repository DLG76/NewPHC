using DG.Tweening;
using UnityEngine;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] private Stage[] allStages;
    [SerializeField] private Transform landmark;
    [SerializeField] private float landmarkMoveTime = 0.75f;

    private void Awake()
    {
        LoadStages();
    }

    private void LoadStages()
    {
        // ชั้่วคราว
        foreach (var stage in allStages)
        {
            stage.Lock();
            stage.Setup(false);
        }
    }

    private void SaveStages()
    {

    }

    public void MoveTo(Stage stage)
    {
        landmark.DOMove(stage.transform.position, landmarkMoveTime);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var ray = Physics2D.CircleCast(rayPos, 0.15f, Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Stage"));

            if (ray.collider != null)
            {
                if (ray.transform.TryGetComponent(out Stage stage))
                {
                    stage.Select();
                }
            }
        }
    }
}
