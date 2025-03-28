using Newtonsoft.Json.Linq;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static string currentStage;

    public bool isStartStage = false;
    public string stageId;
    public Stage[] NextStages { get => _nextStages; }
    [SerializeField] protected Stage[] _nextStages;

    public JObject StageData { get => _stageData; }
    protected JObject _stageData;

    public JObject MyClearedStage { get => _myClearedStage; }
    protected JObject _myClearedStage;

    public bool isLock = false;
    protected Stage previewStage;

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentStage == stageId)
            Enter();
    }

    public virtual void Setup(JObject stageData, JObject myClearedStage)
    {
        _stageData = stageData;
        _myClearedStage = myClearedStage;

        if (isStartStage)
            Unlock(null);
        else
            Lock();

        if (_stageData != null)
            if (_stageData["type"]?.ToString() == "CodeStage")
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/CodeStageIcon");
            else if (_stageData["type"]?.ToString() == "CombatStage")
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/CombatStageIcon");

        if (_myClearedStage != null)
            Success();
    }

    public void Select()
    {
        if (_stageData == null) return;
        if (isLock) return;
        if (!StageManager.Instance.CanEnterStage()) return;

        if (currentStage == stageId)
        {
            Enter();
        }
        else if ((previewStage != null && currentStage == previewStage.stageId) || _nextStages.FirstOrDefault(ns => ns?.stageId == currentStage) != null)
        {
            currentStage = stageId;
            StageManager.Instance.PlayerMoveTo(this);

            //Debug.Log($"currentStage: {currentStage}");
        }
    }

    public abstract void Enter();

    public virtual void Success()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/SuccessStageIcon");
    }

    public void Unlock(Stage previewStage)
    {
        if (previewStage != null)
            this.previewStage = previewStage;

        isLock = false;
    }

    public void Lock()
    {
        isLock = true;
    }
}
