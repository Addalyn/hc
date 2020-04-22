using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Loot
{
	public List<InventoryItem> Items;

	public Dictionary<int, Karma> Karmas;

	public Loot()
	{
		Karmas = new Dictionary<int, Karma>();
		Items = new List<InventoryItem>();
	}

	public void AddItem(InventoryItem item)
	{
		Items.Add(item);
	}

	public void AddItems(List<InventoryItem> items)
	{
		Items.AddRange(items);
	}

	public bool HasItem(int itemTemplateId)
	{
		return Items.Exists((InventoryItem i) => i.TemplateId == itemTemplateId);
	}

	public IEnumerable<int> GetItemTemplateIds()
	{
		List<InventoryItem> items = Items;
		if (_003C_003Ef__am_0024cache0 == null)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			_003C_003Ef__am_0024cache0 = ((InventoryItem i) => i.TemplateId);
		}
		return items.Select(_003C_003Ef__am_0024cache0);
	}

	public void AddKarma(Karma karma)
	{
		if (Karmas.ContainsKey(karma.TemplateId))
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Karmas[karma.TemplateId].Quantity += karma.Quantity;
					return;
				}
			}
		}
		Karmas[karma.TemplateId] = new Karma
		{
			TemplateId = karma.TemplateId,
			Quantity = karma.Quantity
		};
	}

	public Karma GetKarma(int karmaTemplateId)
	{
		Karmas.TryGetValue(karmaTemplateId, out Karma value);
		return value;
	}

	public int GetKarmaQuantity(int karmaTemplateId)
	{
		int result = 0;
		Karma karma = GetKarma(karmaTemplateId);
		if (karma != null)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			result = karma.Quantity;
		}
		return result;
	}

	public void MergeItems(Loot loot)
	{
		AddItems(loot.Items);
	}
}
