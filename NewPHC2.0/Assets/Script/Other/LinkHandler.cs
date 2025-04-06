using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkHandler : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text textMeshPro;

    void Awake()
    {
        textMeshPro = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ตรวจสอบว่าคลิกที่ลิงก์ใด
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, Camera.main);
        if (linkIndex != -1) // ถ้ามีลิงก์ในตำแหน่งที่คลิก
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID(); // ดึง URL จากลิงก์

            Application.OpenURL(url); // เปิดลิงก์ในเบราว์เซอร์
        }
    }
}
