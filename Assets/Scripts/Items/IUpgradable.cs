public interface IUpgradable
{
    int CurrentLevel { get; }
    int GetUpgradeCost();
    void Upgrade(out Upgrade upgrade);
    string GetUpgradeDescription();
}