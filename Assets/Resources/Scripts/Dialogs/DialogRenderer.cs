using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogRenderer : MonoBehaviour //скрипт для рендеринга диалогов
{
    public Dialog Current_dialog;
    public int CurrentPhrase;

    [SerializeField] private GameObject PlayerPhrases;
    [SerializeField] private GameObject NpcPhrases;

    public void ShowDialog() //показываем фразу
    {
        //очистка
        PlayerPhrases.SetActive(false);
        NpcPhrases.SetActive(false);

        if (Current_dialog.dialog[CurrentPhrase].Player)
        {
            //настройка у игрока
            PlayerPhrases.SetActive(true);
            this.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].text;
            this.transform.GetChild(0).GetChild(1).GetComponent<Image>().sprite = Current_dialog.dialog[CurrentPhrase].icon;
            this.transform.GetChild(0).GetChild(2).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].Name;
        }
        else
        {
            //у нпс
            NpcPhrases.SetActive(true);
            this.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].text;
            this.transform.GetChild(1).GetChild(1).GetComponent<Image>().sprite = Current_dialog.dialog[CurrentPhrase].icon;
            this.transform.GetChild(1).GetChild(2).GetComponent<TMP_Text>().text = Current_dialog.dialog[CurrentPhrase].Name;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
