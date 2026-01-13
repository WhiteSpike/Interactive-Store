using InteractiveStore.UI.Cursor;
using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Screen;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Audio.Handle;

namespace InteractiveStore.UI.Application
{
	internal class UnlockableStoreApplication : PageApplication<CursorElement>
	{
		#region String Constants
		const string SCREEN_TITLE = "The Company Store";
		const string INITIAL_SCREEN_DESCRIPTION = "Select the items you wish to purchase.";
		const string CONFIRM_PURCHASE_FORMAT = "Do you wish to purchase the selected item for {0}$ ? \n{1}";
		const string NOT_ENOUGH_CREDITS_ERROR = "You do not have enough Company Credits to purchase the selected items...";
		const string NO_PURCHASE_DURING_FLIGHT = "You cannot purchase furniture items while landing/departing...";
		const string UNLOCKABLE_NOT_AVAILABLE = "This item is currently not available for purchase...";
		const string UNLOCKABLE_ALREADY_PURCHASED = "This item was already purchased for your assigned ship...";
		#endregion

		protected override int GetEntriesPerPage<K>(K[] entries)
		{
			return 10;
		}
		public override void Initialization()
		{
			List<TerminalNode> temp = new(terminal.ShipDecorSelection);
			UnlockableItem unlockable = null;
			foreach (TerminalKeyword keyword in terminal.terminalNodes.allKeywords)
			{
				if (keyword.compatibleNouns != null)
				{
					foreach (CompatibleNoun noun in keyword.compatibleNouns)
					{
						TerminalNode node = noun.result;
						if (node == null || node.shipUnlockableID < 0) continue;
						unlockable = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
						if (unlockable.alwaysInStock && !temp.Contains(node))
							temp.Add(node);
					}
				}
				TerminalNode specialKeywordResult = keyword.specialKeywordResult;
				if (specialKeywordResult == null || specialKeywordResult.shipUnlockableID < 0) continue;
				unlockable = StartOfRound.Instance.unlockablesList.unlockables[specialKeywordResult.shipUnlockableID];
				if (unlockable.alwaysInStock && !temp.Contains(specialKeywordResult))
					temp.Add(specialKeywordResult);
			}
			TerminalNode[] currentShipUnlockables = temp.ToArray();

			(TerminalNode[][], BaseCursorMenu<CursorElement>[], IScreen[]) entries = GetPageEntries(currentShipUnlockables);
			TerminalNode[][] pagesItems = entries.Item1;
			BaseCursorMenu<CursorElement>[] cursorMenus = entries.Item2;
			IScreen[] screens = entries.Item3;

			for(int i = 0; i < pagesItems.Length; i ++)
			{
				TerminalNode[] itemList = pagesItems[i];
				CursorElement[] elements = new CursorElement[itemList.Length];
				cursorMenus[i] = CursorMenu<CursorElement>.Create(startingCursorIndex: 0, elements: elements, sorting:
				[
					CompareName,
					CompareDescendingPrice,
					CompareAscendingPrice
				]);
				cursorMenus[i].sortingIndex = -1;
				BaseCursorMenu<CursorElement> cursorMenu = cursorMenus[i];
				ITextElement[] textElements =
				[
					TextElement.Create(text: INITIAL_SCREEN_DESCRIPTION),
					TextElement.Create(text: " "),
					cursorMenu
				];
				screens[i] = new BoxedOutputScreen<int, string>()
				{
					Title = SCREEN_TITLE,
					elements = textElements,
					Input = () => ApplyInputFunction(),
					Output = (int x) => ApplyOutputFunction(x),
				};

				for (int j = 0; j < itemList.Length; j++)
				{
					TerminalNode item = itemList[j];
					if (item == null) continue;
					int itemIndex = j + (i * itemList.Length);
					int pageIndex = i;
					int index = j;
					UnlockableCursorElement cursor = new UnlockableCursorElement(node: currentShipUnlockables[itemIndex], terminal: terminal);
					cursor.Action = () => TryBuySelectedItem(cursor, () => SwitchScreen(screens[pageIndex], cursorMenus[pageIndex], previous: true));
					elements[j] = cursor;
				}
			}
			/*
			for (int i = 0; i < itemCursorElements.Length; i++)
			{
				itemCursorElements[i] = new ItemCursorElement(item: buyableItems[i], salePercentage: (salePercentages[i] / 100f), pressAction: () => TryBuySelectedItems(() => SwitchScreen(screen, menu, previous: true)), terminal: terminal);
			}
			*/
			currentPage = initialPage;
			currentCursorMenu = initialPage.GetCurrentCursorMenu();
			currentScreen = initialPage.GetCurrentScreen();
		}

		int CompareName(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			UnlockableCursorElement element = cursor1 as UnlockableCursorElement;
			UnlockableCursorElement element2 = cursor2 as UnlockableCursorElement;
			string name1 = element.node.creatureName;
			string name2 = element2.node.creatureName;
			return name1.CompareTo(name2);
		}
		int CompareDescendingPrice(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			UnlockableCursorElement element = cursor1 as UnlockableCursorElement;
			UnlockableCursorElement element2 = cursor2 as UnlockableCursorElement;
			int price1 = element.node.itemCost;
			int price2 = element2.node.itemCost;
			return price1.CompareTo(price2);
		}
		int CompareAscendingPrice(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			UnlockableCursorElement element = cursor1 as UnlockableCursorElement;
			UnlockableCursorElement element2 = cursor2 as UnlockableCursorElement;
			int price1 = element.node.itemCost;
			int price2 = element2.node.itemCost;
			return price2.CompareTo(price1);
		}

		private string ApplyOutputFunction(int x)
		{
			StringBuilder sb = new();
			int currentSort = currentCursorMenu.sortingIndex;
			string sort = currentSort switch
			{
				0 => $"Sort: Alphabetical [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				1 => $"Sort: Price (Des.) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				2 => $"Sort: Price (Asc.) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				_ => $"Sort: None [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
			};
			return $"{sort}";
		}
		void TryBuySelectedItem(CursorElement node, Action backAction)
		{
			StringBuilder sb = new StringBuilder();
			UnlockableCursorElement cursor = node as UnlockableCursorElement;
			int totalCost = cursor.node.itemCost;
			if (terminal.groupCredits < totalCost)
			{
				ErrorMessage(SCREEN_TITLE, NOT_ENOUGH_CREDITS_ERROR, backAction, "");
				return;
			}
			if ((!StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipHasLanded) || StartOfRound.Instance.shipAnimator.GetCurrentAnimatorStateInfo(0).tagHash != Animator.StringToHash("ShipIdle"))
			{
				ErrorMessage(SCREEN_TITLE, NO_PURCHASE_DURING_FLIGHT, backAction, "");
				return;
			}
			UnlockableItem item = cursor.unlockable;
			if (!terminal.ShipDecorSelection.Contains(cursor.node) && !item.alwaysInStock && (!cursor.node.buyUnlockable || item.shopSelectionNode == null))
			{
				ErrorMessage(SCREEN_TITLE, UNLOCKABLE_NOT_AVAILABLE, backAction, "");
				return;
			}
			if (item.hasBeenUnlockedByPlayer || item.alreadyUnlocked)
			{
				ErrorMessage(SCREEN_TITLE, UNLOCKABLE_ALREADY_PURCHASED, backAction, "");
				return;
			}

			Confirm(SCREEN_TITLE, string.Format(CONFIRM_PURCHASE_FORMAT, totalCost, sb.ToString()), () => PurchaseSelectedItems(cursor.node, totalCost, backAction), backAction);
		}
		const int PURCHASE_AUDIO_SYNCED_INDEX = 0;
		private void PurchaseSelectedItems(TerminalNode node, int totalCost, Action backAction)
		{
			terminal.groupCredits = Mathf.Clamp(terminal.groupCredits - totalCost, 0, 10000000);

			terminal.PlayTerminalAudioServerRpc(PURCHASE_AUDIO_SYNCED_INDEX);
			StartOfRound.Instance.BuyShipUnlockableServerRpc(node.shipUnlockableID, terminal.groupCredits);
			backAction();
		}

		int ApplyInputFunction()
		{
			int result = 0;
			return result;
		}
	}
}
