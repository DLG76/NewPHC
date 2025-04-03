using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class Stage : MonoBehaviour
{
    public static string currentStage;

    public bool isStartStage = false;
    public string stageId;

    public StageConnection[] ConnectingStages { get => _connectingStages; set => _connectingStages = value; }
    [SerializeField, FormerlySerializedAs("_nextStages")] protected StageConnection[] _connectingStages;

    public JObject StageData { get => _stageData; }
    protected JObject _stageData;

    public JObject MyClearedStage { get => _myClearedStage; }
    protected JObject _myClearedStage;

    private GameObject linesObj;

    [SerializeField] private Vector2 lineSize = new Vector2(0.25f, 0.1f); // ขนาดเส้นประ (ยาว, หนา)
    [SerializeField, Min(0)] private float gapSize = 0.1f; // ระยะห่างระหว่างเส้นประ
    [SerializeField, Min(0)] private float padding = 0.4f; // ระยะห่างระหว่างเส้นประทั้งหมดกับจุดต้นทางและปลายทาง

    public bool isLock = false;

    private void OnDrawGizmos()
    {
        foreach (var stageLine in _connectingStages)
        {
            if (stageLine?.stageB == null) continue;

            Vector3 startPos = transform.position;
            Vector3 endPos = stageLine.stageB.transform.position;

            // หาทิศทางหลักของเส้นตรงจาก A → B
            Vector3 mainDirection = (endPos - startPos).normalized;

            // ปรับ padding เพื่อเลื่อน A และ B ออกจากกัน
            float totalDistance = Vector3.Distance(startPos, endPos);
            int resolution = Mathf.FloorToInt(totalDistance / lineSize.x);
            float padding = Mathf.Min(this.padding, totalDistance / 2); // ป้องกัน padding มากเกินไป

            startPos += mainDirection * padding;
            endPos -= mainDirection * padding;

            List<Vector3> curvePoints = new List<Vector3>();

            // คำนวณเวกเตอร์ตั้งฉากเพื่อใช้ขยับเส้นโค้ง
            Vector3 perpendicular = new Vector3(-mainDirection.y, mainDirection.x, 0).normalized;

            // สร้างจุดโค้งตาม AnimationCurve
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 point = Vector3.Lerp(startPos, endPos, t);

                // ใช้ AnimationCurve เพื่อเพิ่มค่าความโค้งไปตามแนวตั้งฉากของเส้น
                float curveOffset = stageLine.curve.Evaluate(t) * totalDistance * 0.2f; // ปรับค่า 0.2f เพื่อควบคุมขนาดโค้ง
                point += perpendicular * curveOffset;

                curvePoints.Add(point);
            }

            // คำนวณความยาวเส้นโค้ง
            float curveLength = 0;
            for (int i = 1; i < curvePoints.Count; i++)
            {
                curveLength += Vector3.Distance(curvePoints[i - 1], curvePoints[i]);
            }

            // คำนวณจำนวนสี่เหลี่ยมแทนเส้นประ
            float dashSegmentLength = lineSize.x + gapSize;
            int dashCount = Mathf.FloorToInt(curveLength / dashSegmentLength);
            float remainingLength = curveLength - (dashCount * dashSegmentLength);
            float halfRemainder = remainingLength / 2;

            Gizmos.color = Color.white;

            for (int i = 0; i < dashCount; i++)
            {
                float startDist = halfRemainder + i * dashSegmentLength;
                float endDist = startDist + lineSize.x;

                Vector3 startDash = GetPointAtDistance(curvePoints, startDist);
                Vector3 endDash = GetPointAtDistance(curvePoints, endDist);

                // คำนวณตำแหน่งตรงกลางของสี่เหลี่ยม
                Vector3 midPoint = (startDash + endDash) / 2;
                Vector3 direction = (endDash - startDash).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction); // หมุนตามเส้นโค้ง

                // วาดสี่เหลี่ยมแทนเส้นประ
                Gizmos.matrix = Matrix4x4.TRS(midPoint, rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, new Vector3(0, lineSize.y, lineSize.x)); // กำหนดขนาดสี่เหลี่ยม
            }

            // รีเซ็ต Gizmos.matrix ให้กลับเป็นค่าปกติ
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    private Vector3 GetPointAtDistance(List<Vector3> points, float distance)
    {
        float traveled = 0;
        for (int i = 1; i < points.Count; i++)
        {
            float segmentLength = Vector3.Distance(points[i - 1], points[i]);
            if (traveled + segmentLength >= distance)
            {
                float t = (distance - traveled) / segmentLength;
                return Vector3.Lerp(points[i - 1], points[i], t);
            }
            traveled += segmentLength;
        }
        return points[points.Count - 1];
    }

    protected virtual void Awake()
    {
        if (linesObj == null)
        {
            linesObj = new GameObject("Lines");
            linesObj.transform.SetParent(transform);
        }

        foreach (var stageLine in _connectingStages)
        {
            if (stageLine?.stageB == null) continue;

            Vector3 startPos = transform.position;
            Vector3 endPos = stageLine.stageB.transform.position;

            // หาทิศทางหลักของเส้นตรงจาก A → B
            Vector3 mainDirection = (endPos - startPos).normalized;

            // ปรับ padding เพื่อเลื่อน A และ B ออกจากกัน
            float totalDistance = Vector3.Distance(startPos, endPos);
            int resolution = Mathf.FloorToInt(totalDistance / lineSize.x);
            float padding = Mathf.Min(this.padding, totalDistance / 2); // ป้องกัน padding มากเกินไป

            startPos += mainDirection * padding;
            endPos -= mainDirection * padding;

            List<Vector3> curvePoints = new List<Vector3>();

            // คำนวณเวกเตอร์ตั้งฉากเพื่อใช้ขยับเส้นโค้ง
            Vector3 perpendicular = new Vector3(-mainDirection.y, mainDirection.x, 0).normalized;

            // สร้างจุดโค้งตาม AnimationCurve
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 point = Vector3.Lerp(startPos, endPos, t);

                // ใช้ AnimationCurve เพื่อเพิ่มค่าความโค้งไปตามแนวตั้งฉากของเส้น
                float curveOffset = stageLine.curve.Evaluate(t) * totalDistance * 0.2f; // ปรับค่า 0.2f เพื่อควบคุมขนาดโค้ง
                point += perpendicular * curveOffset;

                curvePoints.Add(point);
            }

            // คำนวณความยาวเส้นโค้ง
            float curveLength = 0;
            for (int i = 1; i < curvePoints.Count; i++)
            {
                curveLength += Vector3.Distance(curvePoints[i - 1], curvePoints[i]);
            }

            // คำนวณจำนวนสี่เหลี่ยมแทนเส้นประ
            float dashSegmentLength = lineSize.x + gapSize;
            int dashCount = Mathf.FloorToInt(curveLength / dashSegmentLength);
            float remainingLength = curveLength - (dashCount * dashSegmentLength);
            float halfRemainder = remainingLength / 2;

            Gizmos.color = Color.white;

            for (int i = 0; i < dashCount; i++)
            {
                float startDist = halfRemainder + i * dashSegmentLength;
                float endDist = startDist + lineSize.x;

                Vector3 startDash = GetPointAtDistance(curvePoints, startDist);
                Vector3 endDash = GetPointAtDistance(curvePoints, endDist);

                // คำนวณตำแหน่งตรงกลางของสี่เหลี่ยม
                Vector3 midPoint = (startDash + endDash) / 2;
                Vector3 direction = (endDash - startDash).normalized;
                Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction); // หมุนตามเส้นโค้ง
                rotation *= Quaternion.Euler(0, 0, 90);

                GameObject lineObj = new GameObject("Line");
                SpriteRenderer spriteRenderer = lineObj.AddComponent<SpriteRenderer>();

                spriteRenderer.sprite = Resources.Load<Sprite>("Sprite/EmptySprite");
                spriteRenderer.color = Color.white;
                lineObj.transform.localScale = lineSize;
                lineObj.transform.position = midPoint;
                lineObj.transform.rotation = rotation;

                lineObj.transform.SetParent(linesObj.transform);
            }

            stageLine.stageA = this;
        }
    }

    public virtual void Setup(JObject stageData, JObject myClearedStage)
    {
        _stageData = stageData;
        _myClearedStage = myClearedStage;

        if (isStartStage)
            Unlock();
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
        else
        {
            StageManager.Instance.PlayerMoveTo(this);

            //Debug.Log($"currentStage: {currentStage}");
        }
    }

    public abstract void Enter();

    public virtual void Success()
    {
        GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprite/SuccessStageIcon");
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

[System.Serializable]
public class StageConnection
{
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 0);
    public Stage stageA;
    [FormerlySerializedAs("stage")] public Stage stageB;
}