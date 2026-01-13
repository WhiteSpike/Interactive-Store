using InteractiveTerminalAPI.UI.Cursor;
using System;
using System.Text;
using UnityEngine;

namespace InteractiveStore.UI.Cursor
{
	public class UnlockableCursorElement : CursorElement
	{
		const int NAME_LENGTH = 30;
		const char WHITE_SPACE = ' ';
		public TerminalNode node;
		public UnlockableItem unlockable;
		private Terminal terminal;

		public UnlockableCursorElement(TerminalNode node, Terminal terminal)
		{
			this.node = node;
			this.terminal = terminal;
			unlockable = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
			Active = (CursorElement x) => IsAvailableForPurchase(x);
		}

		private bool IsAvailableForPurchase(CursorElement x)
		{
			int totalCost = node.itemCost;
			return !(terminal.groupCredits < totalCost ||
				((!StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipHasLanded) || StartOfRound.Instance.shipAnimator.GetCurrentAnimatorStateInfo(0).tagHash != Animator.StringToHash("ShipIdle")) ||
				(!terminal.ShipDecorSelection.Contains(node) && !unlockable.alwaysInStock && (!node.buyUnlockable || unlockable.shopSelectionNode == null)) ||
				(unlockable.hasBeenUnlockedByPlayer || unlockable.alreadyUnlocked));
		}

		public override string GetText(int availableLength)
		{
			StringBuilder sb = new();
			int groupCredits = terminal.groupCredits;
			if (!Active(this))
			{
				if (unlockable.hasBeenUnlockedByPlayer || unlockable.alreadyUnlocked)
					sb.Append("<color=#026440>");
				else
					sb.Append("<color=#66666666>");
			}
			string name = node.creatureName.Length > NAME_LENGTH ? node.creatureName.Substring(0, NAME_LENGTH) : node.creatureName + new string(WHITE_SPACE, Mathf.Max(0, NAME_LENGTH - node.creatureName.Length));
			sb.Append(name);

			int price = Mathf.CeilToInt(node.itemCost);
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
			if (!Active(this)) sb.Append("</color>");
			return sb.ToString();
		}
	}
}
