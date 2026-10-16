using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

// Каталог резьбы: какие бывают лица (иконки, названия) и список всех готовых вариантов тыквы.
// Варианты (лицо x свеча) создаются автоматически: ПКМ по заголовку компонента -> Generate Variants.
[CreateAssetMenu(
    fileName = "PumpkinCarvingCatalog",
    menuName = "Game/Carving/Pumpkin Carving Catalog"
)]
public class PumpkinCarvingCatalog : ScriptableObject
{
    [Serializable]
    public class FaceVisual
    {
        public PumpkinFace face;
        [Tooltip("Название на кнопке. Если пусто - название enum.")]
        public string label;
        [Tooltip("Иконка кнопки в меню изменения (если пусто - иконка тыквы).")]
        public Sprite buttonIcon;
        [Tooltip("Иконка тыквы с этим лицом (без свечи).")]
        public Sprite pumpkinIcon;
        [Tooltip("Иконка тыквы с этим лицом и свечой. Если пусто - берётся обычная.")]
        public Sprite pumpkinIconWithCandle;
    }

    [Header("Faces (добавьте по одной записи на КАЖДОЕ значение PumpkinFace, включая None)")]
    [SerializeField] private List<FaceVisual> faces = new List<FaceVisual>();

    [Header("Generation settings")]
    [SerializeField] private string baseName = "Pumpkin";
    [SerializeField] private PickupItem worldPrefab;
    [SerializeField, Min(1)] private int maxStackSize = 99;

    [Header("Generated variants (заполняется автоматически)")]
    [SerializeField] private List<PumpkinItemDefinition> variants = new List<PumpkinItemDefinition>();

    public IReadOnlyList<FaceVisual> Faces => faces;

    public FaceVisual GetFaceVisual(PumpkinFace face)
    {
        foreach (FaceVisual visual in faces)
        {
            if (visual != null && visual.face == face)
                return visual;
        }
        return null;
    }

    // Можно ли эту вещь резать (это тыква-вариант)
    public bool IsCarvable(ItemDefinition item)
    {
        return item is PumpkinItemDefinition;
    }

    // Найти готовый вариант тыквы по лицу и свече
    public bool TryGetVariant(PumpkinFace face, bool hasCandle, out PumpkinItemDefinition result)
    {
        foreach (PumpkinItemDefinition variant in variants)
        {
            if (variant != null && variant.Face == face && variant.HasCandle == hasCandle)
            {
                result = variant;
                return true;
            }
        }

        result = null;
        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Variants")]
    private void GenerateVariants()
    {
        string catalogPath = AssetDatabase.GetAssetPath(this);
        if (string.IsNullOrEmpty(catalogPath))
        {
            Debug.LogWarning("Сначала сохраните каталог как ассет.", this);
            return;
        }

        string directory = Path.GetDirectoryName(catalogPath).Replace("\\", "/");
        string folder = directory + "/Variants";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(directory, "Variants");

        variants.Clear();

        foreach (PumpkinFace face in Enum.GetValues(typeof(PumpkinFace)))
        {
            foreach (bool candle in new[] { false, true })
            {
                string suffix = candle ? "Candle" : "NoCandle";
                string assetName = "Pumpkin_" + face + "_" + suffix;
                string assetPath = folder + "/" + assetName + ".asset";

                PumpkinItemDefinition asset =
                    AssetDatabase.LoadAssetAtPath<PumpkinItemDefinition>(assetPath);
                if (asset == null)
                {
                    asset = CreateInstance<PumpkinItemDefinition>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                FaceVisual visual = GetFaceVisual(face);
                string faceLabel = visual != null && !string.IsNullOrEmpty(visual.label)
                    ? visual.label
                    : face.ToString();

                string displayName = face == PumpkinFace.None
                    ? baseName
                    : faceLabel + " " + baseName;
                if (candle) displayName += " (candle)";

                Sprite icon = null;
                if (visual != null)
                {
                    icon = candle && visual.pumpkinIconWithCandle != null
                        ? visual.pumpkinIconWithCandle
                        : visual.pumpkinIcon;
                }

                SerializedObject so = new SerializedObject(asset);
                so.FindProperty("itemId").stringValue = assetName.ToLowerInvariant();
                so.FindProperty("displayName").stringValue = displayName;
                so.FindProperty("icon").objectReferenceValue = icon;
                so.FindProperty("worldPrefab").objectReferenceValue = worldPrefab;
                so.FindProperty("maxStackSize").intValue = Mathf.Max(1, maxStackSize);
                so.FindProperty("face").enumValueIndex = (int)face;
                so.FindProperty("hasCandle").boolValue = candle;
                so.ApplyModifiedPropertiesWithoutUndo();

                EditorUtility.SetDirty(asset);
                variants.Add(asset);
            }
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("Pumpkin variants generated: " + variants.Count, this);
    }
#endif
}
