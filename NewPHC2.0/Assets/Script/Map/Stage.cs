using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static string currentStage;

    [SerializeField] private bool isStartStage = false;
    public string stageId;
    public Stage[] NextStages { get => _nextStages; }
    [SerializeField] protected Stage[] _nextStages;

    public JObject StageData { get => _stageData; }
    protected JObject _stageData;

    public JObject MyClearedStage { get => _myClearedStage; }
    protected JObject _myClearedStage;

    protected bool isLock = false;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(currentStage) && isStartStage)
            currentStage = stageId;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && currentStage == stageId)
            Enter();
    }

    private void OnMouseDown() => Select();

    public virtual void Setup(JObject stageData, JObject myClearedStage)
    {
        _stageData = stageData;
        _myClearedStage = myClearedStage;

        if (isStartStage)
        {
            Unlock();
        }
        else
        {
            Lock();
        }
    }

    public void Select()
    {
        if (_stageData == null) return;
        if (isLock) return;

        if (currentStage == stageId)
        {
            Enter();
        }
        else
        {
            currentStage = stageId;
            StageManager.Instance.PlayerMoveTo(this);

            //Debug.Log($"currentStage: {currentStage}");
        }
    }

    public abstract void Enter();

    public virtual void Success()
    {
        Debug.Log("Success");

        GetComponent<SpriteRenderer>().color = Color.green;
    }

    public void Unlock()
    {
        isLock = false;
    }

    public void Lock()
    {
        isLock = true;
    }
}
