#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Меню Tools -> Tutorial -> Create Carving Tutorial Steps
// Собирает на открытой сцене 5 шагов обучения про резьбу и финал,
// соединяет их цепочкой и подключает к вашему TutorialManager.
// Если шаги уже были созданы раньше, предложит заменить их новой версией.
public static class CarvingTutorialBuilder
{
    private const string RootName = "CarvingTutorial";

    [MenuItem("Tools/Tutorial/Create Carving Tutorial Steps")]
    private static void CreateSteps()
    {
        TutorialManager manager = Object.FindFirstObjectByType<TutorialManager>();
        if (manager == null)
        {
            EditorUtility.DisplayDialog(
                "Carving Tutorial",
                "На открытой сцене не найден TutorialManager.", "OK");
            return;
        }

        // Если шаги уже создавались, удаляем старую версию
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
        {
            bool replace = EditorUtility.DisplayDialog(
                "Carving Tutorial",
                "Объект " + RootName + " уже есть на сцене. Заменить его новой версией?",
                "Заменить", "Отмена");
            if (!replace) return;

            Undo.DestroyObjectImmediate(oldRoot);

            Undo.RecordObject(manager, "Clean tutorial steps list");
            manager.tutorialSteps.RemoveAll(step => step == null);
            EditorUtility.SetDirty(manager);
        }

        // Берём Player, Panel и текст из уже существующих шагов
        TutorialStep template = null;
        TutorialStep chainEnd = null;
        foreach (TutorialStep existing in manager.tutorialSteps)
        {
            if (existing == null) continue;

            if (template == null && existing.Panel != null)
                template = existing;

            // Шаг без следующего = конец текущей цепочки
            if (existing.nextStep == null)
                chainEnd = existing;
        }

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Create Carving Tutorial");

        TutorialStep[] steps = new TutorialStep[5];

        steps[0] = CreateStep(root, "Step_Carving_1_OpenTable", template,
            "Time to get creative! Go to the carving table and press Space to open it.");
        steps[1] = CreateStep(root, "Step_Carving_2_Face", template,
            "Click a pumpkin in the list on the left. A menu opens on the right - choose a face to carve it.");
        steps[2] = CreateStep(root, "Step_Carving_3_Candle", template,
            "Add some light! Press 'Add candle' to put a candle inside your pumpkin. It uses one candle from your bag.");
        steps[3] = CreateStep(root, "Step_Carving_4_Close", template,
            "Great job! Close the carving window with Esc or the X button.");
        steps[4] = CreateStep(root, "Step_Final", template,
            "That is everything you need to know. Good luck on your pumpkin farm! Press Space to finish.");

        // Цепочка шагов
        for (int i = 0; i < steps.Length - 1; i++)
            steps[i].nextStep = steps[i + 1];

        // Условия
        AddCondition<OpenCarvingCondition>(steps[0]);

        CarvePumpkinCondition faceCondition = AddCondition<CarvePumpkinCondition>(steps[1]);
        SetBool(faceCondition, "requireFaceChange", true);

        CarvePumpkinCondition candleCondition = AddCondition<CarvePumpkinCondition>(steps[2]);
        SetBool(candleCondition, "requireFaceChange", false);
        SetBool(candleCondition, "requireCandleAdded", true);

        AddCondition<CloseCarvingCondition>(steps[3]);
        AddCondition<PressSpaceCondition>(steps[4]);

        // Выдача предметов (объекты выключены, шаг включает их при старте)
        TutorialItemGiver pumpkinGiver = AddGiver(root, steps[0], "Giver_Pumpkin");
        SetBool(pumpkinGiver, "clearInventoryFirst", true);   // перед тыквой сумка очищается
        AddGiver(root, steps[2], "Giver_Candle");

        // Если станок резьбы на сцене выключен, шаг 1 его включит
        PumpkinCarvingStation station =
            Object.FindFirstObjectByType<PumpkinCarvingStation>(FindObjectsInactive.Include);
        if (station != null && !station.gameObject.activeSelf)
            steps[0].objectsToActivateOnStart.Add(station.gameObject);

        // Подключаем к менеджеру
        Undo.RecordObject(manager, "Add carving tutorial steps");
        foreach (TutorialStep step in steps)
            manager.tutorialSteps.Add(step);
        EditorUtility.SetDirty(manager);

        // Присоединяем к концу существующей цепочки
        string hookMessage;
        if (chainEnd != null)
        {
            Undo.RecordObject(chainEnd, "Link carving tutorial");
            chainEnd.nextStep = steps[0];
            EditorUtility.SetDirty(chainEnd);
            hookMessage = "Новые шаги присоединены после шага '" + chainEnd.name + "'.";
        }
        else
        {
            hookMessage = "Не нашёл конец цепочки: в своём последнем шаге укажите Next Step = Step_Carving_1_OpenTable.";
        }

        foreach (TutorialStep step in steps)
            EditorUtility.SetDirty(step);

        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        Selection.activeGameObject = root;

        EditorUtility.DisplayDialog(
            "Carving Tutorial",
            "Готово: создано 5 шагов (объект " + RootName + ").\n\n" + hookMessage +
            "\n\nТеперь заполните предметы в Giver_Pumpkin и Giver_Candle. " +
            "В Giver_Pumpkin включена очистка сумки: добавьте в Items To Keep мотыгу и лейку.",
            "OK");
    }

    private static TutorialStep CreateStep(
        GameObject root, string stepName, TutorialStep template, string text)
    {
        GameObject go = new GameObject(stepName);
        Undo.RegisterCreatedObjectUndo(go, "Create tutorial step");
        go.transform.SetParent(root.transform, false);

        TutorialStep step = Undo.AddComponent<TutorialStep>(go);
        step.TutorialText = text;
        step.canPlayerMove = true;

        if (template != null)
        {
            step.Player = template.Player;
            step.Panel = template.Panel;
            step.TextMeshPro = template.TextMeshPro;
        }

        return step;
    }

    private static T AddCondition<T>(TutorialStep step) where T : TutorialCondition
    {
        T condition = Undo.AddComponent<T>(step.gameObject);
        step.conditions.Add(condition);
        return condition;
    }

    private static TutorialItemGiver AddGiver(GameObject root, TutorialStep step, string giverName)
    {
        GameObject giver = new GameObject(giverName);
        Undo.RegisterCreatedObjectUndo(giver, "Create tutorial giver");
        giver.transform.SetParent(root.transform, false);
        TutorialItemGiver component = Undo.AddComponent<TutorialItemGiver>(giver);
        giver.SetActive(false);

        step.objectsToActivateOnStart.Add(giver);
        return component;
    }

    private static void SetBool(Object target, string fieldName, bool value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.FindProperty(fieldName).boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
