using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogRenderer : MonoBehaviour
{
    public Dialog Current_dialog;
    public int CurrentPhrase;

    [SerializeField] private GameObject PlayerPhrases;
    [SerializeField] private GameObject NpcPhrases;

    public void ShowDialog()
    {
        PlayerPhrases.SetActive(false);
        NpcPhrases.SetActive(false);

        if (Current_dialog.dialog[CurrentPhrase].Player)
        {
            PlayerPhrases.SetActive(true);
            this.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].text;
            this.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = Current_dialog.dialog[CurrentPhrase].icon;
            this.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].Name;
        }
        else
        {
            NpcPhrases.SetActive(true);
            this.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].text;
            this.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Current_dialog.dialog[CurrentPhrase].icon;
            this.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].Name;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (CurrentPhrase + 1 != Current_dialog.dialog.Count)
            {
                CurrentPhrase++;
                ShowDialog();
            }
            else
            {
                this.gameObject.SetActive(false);
                CurrentPhrase = 0;
                Current_dialog = null;
            }
        }
    }

    private void OnEnable()
    {
        ShowDialog();
    }
}
