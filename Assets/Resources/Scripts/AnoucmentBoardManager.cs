using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnoucmentBoardManager : MonoBehaviour
{
    [SerializeField] private Transform[] poses = new Transform[6]; //это позиции на которые буду клеиться листовки

    [SerializeField] private GameObject AnnoucmentPrefab;
    public GameObject[] AnnoucmentsObj = new GameObject[6];
    public Annoucment[] CurrentAnnoucments = new Annoucment[6];

    public void AddAnnoucment(Annoucment announce)
    {
        for (int i = 0; i < CurrentAnnoucments.Length; i++)
        {
            if (CurrentAnnoucments[i] != null)
            {
                CurrentAnnoucments[i] = announce;
            }
        }
    }

    public void RemoveAnnoucmentAt(int index)
    {
        CurrentAnnoucments[index] = null;
    }

    public void RenderAnnoucments()
    {
        ClearAnnoucments();

        for (int i = 0; i < CurrentAnnoucments.Length; i++)
        {
            if (CurrentAnnoucments[i].exist)
            {
                GameObject instance = Instantiate(AnnoucmentPrefab, poses[i]);
                instance.transform.GetChild(0).GetComponent<TMP_Text>().text = CurrentAnnoucments[i].label;
                instance.transform.GetChild(1).GetComponent<TMP_Text>().text = CurrentAnnoucments[i].text;

                AnnoucmentsObj[i] = instance;
            }
        }
    }

    private void ClearAnnoucments()
    {
        for (int i = 0; i < poses.Length; i++)
        {
            if (poses[i].childCount > 0)
            {
                Destroy(poses[i].GetChild(0).gameObject);
            }
        }
    }

    private void OnEnable()
    {
        RenderAnnoucments();
    }
}

[System.Serializable]
public class Annoucment
{
    public bool exist; //я сделал тут булевую переменную потому что я использую квадратные массивы значит тут всегда не будет нул и будут спавниться пустые обьявлния где не должны
    public string label;
    public string text;
}
