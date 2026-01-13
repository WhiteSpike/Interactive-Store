using InteractiveTerminalAPI.UI.Cursor;
using System;
using System.Text;
using UnityEngine;

namespace InteractiveStore.UI.Cursor
{
	public class ItemCursorElement : CursorOutputElement<int>
	{
		const int NAME_LENGTH = 30;
		const char WHITE_SPACE = ' ';
		public Item item;
		private float salePercentage;
		private Terminal terminal;

		public ItemCursorElement(Item item, float salePercentage, Action pressAction, Terminal terminal)
		{
			this.item = item;
			this.salePercentage = salePercentage;
			this.Action = pressAction;
			this.terminal = terminal;
			this.Counter = 0;
			this.Func = (int x) => x * Mathf.CeilToInt(item.creditsWorth * salePercentage);
			this.terminal = terminal;
		}
		public override string GetText(int availableLength)
		{
			StringBuilder sb = new();
			int groupCredits = terminal.groupCredits;
			//sb.Append(new string(WHITE_SPACE, 2));
			if (!Active(this))
			{
				sb.Append("<color=#66666666>");
			}
			string name = item.itemName.Length > NAME_LENGTH ? item.itemName.Substring(0, NAME_LENGTH) : item.itemName + new string(WHITE_SPACE, Mathf.Max(0, NAME_LENGTH - item.itemName.Length));
			sb.Append(name);

			int price = Mathf.CeilToInt(item.creditsWorth * salePercentage);
			if (price > groupCredits)
			{
				sb.Append(string.Format("<color={0}>", "#8B0000"));
				sb.Append($"{price}$");
				sb.Append("</color>");
			}
			else
			{
				sb.Append($"{price}$");
			}
			if (salePercentage < 1f)
			{
				sb.Append(WHITE_SPACE);
				sb.Append($"({(1f - salePercentage) * 100:F0}% OFF)");
			}

			sb.Append(WHITE_SPACE);
			sb.Append($"⇐{Counter}⇒");
			if (!Active(this)) sb.Append("</color>");
			return sb.ToString();
		}
	}
}
