using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogRenderer : MonoBehaviour //скрипт для рендеринга диалогов
{
    public Dialog Current_dialog;
    public int CurrentPhrase;

    [SerializeField] private TMP_Text PlayerPhrases;
    [SerializeField] private TMP_Text NpcPhrases;
    [SerializeField] private Image NpcIcon;
    [SerializeField] private Image PlayerIcon;
    [SerializeField] private TMP_Text PlayerName;
    [SerializeField] private TMP_Text NpcName;
    [SerializeField] private GameObject MainObject;

    [SerializeField] private AudioClip talksound;

    public void ShowDialog() //показываем фразу
    {
        MainObject.SetActive(true);
        //очистка
        PlayerPhrases.text = "";
        NpcPhrases.text = "";
        PlayerName.text = "";
        NpcName.text = "";

        AudioManager.Instance.PlaySFX(talksound, 2f);

        if (Current_dialog.dialog[CurrentPhrase].Player)
        {
            PlayerPhrases.text = Current_dialog.dialog[CurrentPhrase].text;
            PlayerName.text = Current_dialog.dialog[CurrentPhrase].Name;
        }
        else
        {
            NpcPhrases.text = Current_dialog.dialog[CurrentPhrase].text;
            NpcIcon.sprite = Current_dialog.dialog[CurrentPhrase].icon;
            NpcName.text = Current_dialog.dialog[CurrentPhrase].Name;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Current_dialog != null && CurrentPhrase + 1 != Current_dialog.dialog.Count)
            {
                CurrentPhrase++;
                ShowDialog();
            }
            else
            {
                MainObject.SetActive(false);
                CurrentPhrase = 0;
                Current_dialog = null;
            }
        }
    }
}
