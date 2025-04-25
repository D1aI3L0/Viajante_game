using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    // Основные характеристики персонажа
    public CharacterClass characterClass;
    public int maxHP;
    public int currentHP;
    public int DEF;
    public int EVA;
    public int PROV;
    public int SPD;
    public int SP;
    public int SPreg;
    public int SPmovecost;

    // Выбранные подклассы
    public int[] selectedSubclassIndices;  // Должен хранить 2 значения

    // Характеристики оружий (два оружия)
    public WeaponParameters[] weaponParameters;  // Ожидается массив из 2-х элементов

    // Наборы навыков для оружий
    public WeaponSkillSet[] weaponSkills;        // Массив из 2-х элементов

    // Выбранные индексы дополнительных навыков (индексы начиная с 1, т.к. 0 – базовая атака)
    public int[] selectedSkillIndices;           // Ожидается массив из 3-х элементов

    // Метод инициализации. При вызове мы копируем все значения из переданного runtime объекта.
    public void Init(CharacterRuntimeParameters runtimeParams)
    {
        // Класс персонажа (например, Воин, Мастер, Следопыт и т.д.)
        characterClass = runtimeParams.characterClass;

        // Копирование характеристик
        maxHP = runtimeParams.maxHP;
        currentHP = runtimeParams.currentHP;
        DEF = runtimeParams.DEF;
        EVA = runtimeParams.EVA;
        PROV = runtimeParams.PROV;
        SPD = runtimeParams.SPD;
        SP = runtimeParams.SP;
        SPreg = runtimeParams.SPreg;
        SPmovecost = runtimeParams.SPmovecost;

        // Копирование выбранных индексов подклассов и навыков
        selectedSubclassIndices = (int[]) runtimeParams.selectedSubclassIndices.Clone();
        selectedSkillIndices = (int[]) runtimeParams.selectedSkillIndices.Clone();

        // Подразумеваем, что для оружий и навыков массивы уже заполнены в runtime данных.
        weaponParameters = runtimeParams.weaponParameters;
        weaponSkills = runtimeParams.weaponSkills;

        // Можно добавить вывод в консоль для проверки:
        Debug.LogFormat("BattleCharacter initialized: Class = {0}, HP = {1}/{2}", characterClass, currentHP, maxHP);
    }

    // Пример метода, который будет использоваться для логики получения урона
    public void TakeDamage(int damage)
    {
        // Пример простой формулы: защита снижает входящий урон до минимального значения 1
        int effectiveDamage = Mathf.Max(damage - DEF, 1);
        currentHP -= effectiveDamage;
        Debug.Log(gameObject.name + " получил урон: " + effectiveDamage + ". Текущее HP = " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // Метод, вызываемый при смерти персонажа
    private void Die()
    {
        Debug.Log(gameObject.name + " погиб.");
        // Здесь можно добавить проигрывание анимации, уведомление менеджера боя,
        // удаление объекта или иной необходимый функционал.
        Destroy(gameObject);
    }
}
