public interface IUpgradable
{
    int CurrentLevel { get; }
    int MaxLevel { get; }
    int GetUpgradeCost();
    void Upgrade();
    string GetUpgradeDescription();
}