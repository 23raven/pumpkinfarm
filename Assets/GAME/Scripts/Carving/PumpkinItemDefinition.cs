using UnityEngine;

// Предмет "тыква" с прикреплёнными данными: какое у неё лицо и есть ли свеча внутри.
// Это наследник ItemDefinition, поэтому инвентарь, хранилище, магазин, дроп и заказы
// работают с ним без каких-либо изменений.
// Одинаковые тыквы (то же лицо + та же свеча) - это ОДИН и тот же ассет, поэтому они стакаются.
[CreateAssetMenu(
    fileName = "NewPumpkin",
    menuName = "Game/Items/Pumpkin Item Definition"
)]
public class PumpkinItemDefinition : ItemDefinition
{
    [Header("Pumpkin")]
    [SerializeField] private PumpkinFace face = PumpkinFace.None;
    [SerializeField] private bool hasCandle;

    public PumpkinFace Face => face;
    public bool HasCandle => hasCandle;
}
