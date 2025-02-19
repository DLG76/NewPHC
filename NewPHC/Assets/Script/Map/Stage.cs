using Unity.VisualScripting;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static string currentStage;
    [SerializeField] private bool isStartStage = false;
    [SerializeField] protected string stageName;
    [SerializeField] protected bool isLock = false;
    [SerializeField] protected bool isSuccess = false;
    [SerializeField] private Stage[] nextStages;
    //[SerializeField] private Reward reward;

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(currentStage) && isStartStage)
        {
            currentStage = stageName;
            Unlock();
        }
        else if (!string.IsNullOrEmpty(currentStage) && currentStage == stageName)
        {
            Unlock();
        }
    }

    protected virtual void Update()
    {

    }

    public void Setup(bool isSuccess)
    {
        this.isSuccess = isSuccess;
    }

    public void Select()
    {
        if (isLock) return;

        if (currentStage == stageName)
        {
            Enter();
        }
        else
        {
            currentStage = stageName;
            StageManager.Instance.MoveTo(this);

            Debug.Log($"currentStage: {currentStage}");
        }
    }

    public abstract void Enter();

    public virtual void Success()
    {
        Debug.Log("Success");

        isSuccess = true;

        // get reward
        foreach (var stage in nextStages)
        {
            stage.Unlock();
        }
    }

    public void Unlock()
    {
        isLock = false;
    }

    public void Lock()
    {
        if (!isStartStage)
            isLock = true;
    }
}
