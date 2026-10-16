using UnityEngine;

public class PlayerFootstepsLongAudio : MonoBehaviour
{
    [Header("Компоненты звука")]
    [SerializeField] private AudioSource footstepSource; // Сюда перетащи AudioSource игрока
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    [Header("Настройки чувствительности")]
    [SerializeField] private float movementThreshold = 0.01f; // Минимальное движение

    private Vector3 lastPosition;

    private void Start()
    {
        // Если забыли привязать AudioSource, скрипт попытается найти его на игроке
        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
        }

        // Автоматически настраиваем компонент под цикличный длинный звук
        if (footstepSource != null)
        {
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
            footstepSource.volume = volume;
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (footstepSource == null || footstepSource.clip == null) return;

        // 1. Проверяем, нажимает ли игрок кнопки движения ИЛИ сдвинулся ли он физически
        bool inputsPressed = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool positionChanged = Vector3.Distance(transform.position, lastPosition) > movementThreshold;

        // Игрок идет, если есть ввод с клавиатуры/джойстика или он реально смещается на сцене
        if (inputsPressed || positionChanged)
        {
            // Если звук еще не запущен — включаем дорожку с шагами
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            // Игрок остановился — ставим аудио на паузу или выключаем
            if (footstepSource.isPlaying)
            {
                // Используем Pause вместо Stop, чтобы при следующей ходьбе звук 
                // продолжался, а не начинался заново с тишины
                footstepSource.Pause();
            }
        }

        // Запоминаем позицию для следующего кадра
        lastPosition = transform.position;
    }
}
