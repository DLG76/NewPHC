using Unity.VisualScripting;
using UnityEngine;

public abstract class Stage : MonoBehaviour
{
    public static string currentStage;
    [SerializeField] private bool isStartStage = false;
    public string stageName;
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
        if (Input.GetKeyDown(KeyCode.Space) && currentStage == stageName)
            Enter();
    }

    private void OnMouseDown() => Select();

    public void Setup(bool isLock, bool isSuccess)
    {
        this.isLock = isLock;
        this.isSuccess = isSuccess;

        if (isSuccess)
        {
            foreach (var stage in nextStages)
            {
                stage.Unlock();
            }
        }
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
            StageManager.Instance.LandmarkMoveTo(this);

            Debug.Log($"currentStage: {currentStage}");
        }
    }

    public abstract void Enter();

    public virtual void Success()
    {
        if (isSuccess) return;

        Debug.Log("Success");

        isSuccess = true;

        DatabaseManager.Instance.SuccessStage(this);
        // get reward
        foreach (var stage in nextStages)
        {
            stage.Unlock();
        }
    }

    public void Unlock()
    {
        isLock = false;
        StageManager.unlockStages.Add(stageName);
    }

    public void Lock()
    {
        if (!isStartStage)
        {
            isLock = true;
            StageManager.unlockStages.Remove(stageName);
        }
    }
}
