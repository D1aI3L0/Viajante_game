using System.Collections.Generic;

public class Inventory
{
    private List<Item> items = new();

    public void Add(Item item)
    {
        items.Add(item);
    }

    public void Remove(Item item)
    {
        items.Remove(item);
    }

    public void AddRange(Inventory otherInventory)
    {
        items.AddRange(otherInventory.items);
    }

    public void GetItems(out List<Rune> runes)
    {
        runes = new();
        for(int i = 0; i < items.Count; i++)
        {
            if(items[i] is Rune rune)
                runes.Add(rune);
        }
    }

    public void GetItems(out List<ArmorCore> armorCores)
    {
        armorCores = new();
        for(int i = 0; i < items.Count; i++)
        {
            if(items[i] is ArmorCore armorCore)
                armorCores.Add(armorCore);
        }
    }
}