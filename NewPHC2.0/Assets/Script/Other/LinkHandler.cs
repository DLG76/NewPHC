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
        // ��Ǩ�ͺ��Ҥ�ԡ����ԧ���
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, eventData.position, Camera.main);
        if (linkIndex != -1) // ������ԧ��㹵��˹觷���ԡ
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID(); // �֧ URL �ҡ�ԧ��

            Application.OpenURL(url); // �Դ�ԧ������������
        }
    }
}
