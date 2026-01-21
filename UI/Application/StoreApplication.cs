using InteractiveStore.UI.Cursor;
using InteractiveTerminalAPI.UI;
using InteractiveTerminalAPI.UI.Application;
using InteractiveTerminalAPI.UI.Cursor;
using InteractiveTerminalAPI.UI.Screen;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractiveStore.UI.Application
{
	internal class StoreApplication : CounterPageApplication<CursorOutputElement<int>>
	{
		#region String Constants
		const string SCREEN_TITLE = "The Company Store";
		const string INITIAL_SCREEN_DESCRIPTION = "Select the amount of items you wish to purchase from each entry.";
		const string COLORED_OUTPUT_ITEM_SCREEN_FORMAT = "<color={0}>{1}/{2}</color>";
		const string OUTPUT_SCREEN_ITEM_FORMAT = "{0}/{1}";
		const string OUTPUT_SCREEN_CREDIT_FORMAT = "{0}$";
		const string COLORED_OUTPUT_SCREEN_CREDIT_FORMAT = "<color={0}>{1}$</color>";
		const string CONFIRM_PURCHASE_FORMAT = "Do you wish to purchase the selected {0} items for {1}$ ? \n {2}";
		const string PURCHASE_LISTING_FORMAT = "{0}x {1} ({2}$)\n";
		const string NO_ITEMS_SELECTED_ERROR = "No items were selected for purchasing...";
		const string NOT_ENOUGH_CREDITS_ERROR = "You do not have enough Company Credits to purchase the selected items...";
		const string MAXIMUM_CAPACITY_ERROR = "The amount of items selected and already purchased surpasses the maximum capacity of the item dropship...";
		const string VEHICLE_IN_POD_ERROR = "You are unable to purchase items when a vehicle is loaded into the drop pod...";
		#endregion
		Item[] buyableItems = null;
		int[] salePercentages = null;

		CursorOutputElement<int>[] itemCursorElements = null;

		protected override int GetEntriesPerPage<K>(K[] entries)
		{
			return 10;
		}
		public override void Initialization()
		{
			buyableItems = terminal.buyableItemsList;
			salePercentages = terminal.itemSalesPercentages;

			itemCursorElements = new CursorOutputElement<int>[buyableItems.Length];
			int index = 0;
			(Item[][], BaseCursorMenu<CursorOutputElement<int>>[], IScreen[]) entries = GetPageEntries(buyableItems);
			Item[][] pagesItems = entries.Item1;
			BaseCursorMenu<CursorOutputElement<int>>[] cursorMenus = entries.Item2;
			IScreen[] screens = entries.Item3;

			for(int i = 0; i < pagesItems.Length; i ++)
			{
				Item[] itemList = pagesItems[i];
				CursorOutputElement<int>[] elements = new CursorOutputElement<int>[itemList.Length];
				cursorMenus[i] = CursorMenu<CursorOutputElement<int>>.Create(startingCursorIndex: 0, elements: elements, sorting:
				[
					CompareName,
					CompareDescendingPrice,
					CompareAscendingPrice
				]);
				cursorMenus[i].sortingIndex = -1;
				BaseCursorMenu<CursorOutputElement<int>> cursorMenu = cursorMenus[i];
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
					Item item = itemList[j];
					if (item == null) continue;
					int itemIndex = j + (i * itemList.Length);
					int pageIndex = i;
					elements[j] = new ItemCursorElement(item: buyableItems[itemIndex], salePercentage: (salePercentages[itemIndex] / 100f), pressAction: () => TryBuySelectedItems(() => SwitchScreen(screens[pageIndex], cursorMenus[pageIndex], previous: true)), terminal: terminal);
					itemCursorElements[index++] = elements[j];
				}
			}
			currentPage = initialPage;
			currentCursorMenu = initialPage.GetCurrentCursorMenu();
			currentScreen = initialPage.GetCurrentScreen();
		}
		int CompareName(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			ItemCursorElement element = cursor1 as ItemCursorElement;
			ItemCursorElement element2 = cursor2 as ItemCursorElement;
			string name1 = element.item.itemName;
			string name2 = element2.item.itemName;
			return name1.CompareTo(name2);
		}
		int CompareDescendingPrice(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			ItemCursorElement element = cursor1 as ItemCursorElement;
			ItemCursorElement element2 = cursor2 as ItemCursorElement;
			int price1 = element.item.creditsWorth;
			int price2 = element2.item.creditsWorth;
			return price1.CompareTo(price2);
		}
		int CompareAscendingPrice(CursorElement cursor1, CursorElement cursor2)
		{
			if (cursor1 == null) return 1;
			if (cursor2 == null) return -1;
			ItemCursorElement element = cursor1 as ItemCursorElement;
			ItemCursorElement element2 = cursor2 as ItemCursorElement;
			int price1 = element.item.creditsWorth;
			int price2 = element2.item.creditsWorth;
			return price2.CompareTo(price1);
		}

		private string ApplyOutputFunction(int x)
		{
			StringBuilder sb = new();
			int totalItems = 0;
			for (int i = 0; i < itemCursorElements.Length; i++)
			{
				ItemCursorElement itemCursor = itemCursorElements[i] as ItemCursorElement;
				int counter = itemCursor.Counter;
				if (counter == 0) continue;
				totalItems += counter;
			}
			if (totalItems + terminal.numberOfItemsInDropship == 12)
			{
				sb.Append(string.Format(COLORED_OUTPUT_ITEM_SCREEN_FORMAT, "#FFFF00", totalItems + terminal.numberOfItemsInDropship, 12));
				//sb.Append($"<color=#FFFF00>{totalItems+terminal.numberOfItemsInDropship}/{12}</color> ");
			}
			else if (totalItems + terminal.numberOfItemsInDropship > 12)
			{
				sb.Append(string.Format(COLORED_OUTPUT_ITEM_SCREEN_FORMAT, "#8B0000", totalItems + terminal.numberOfItemsInDropship, 12));
				//sb.Append($"<color=#8B0000>{totalItems+terminal.numberOfItemsInDropship}/{12}</color> ");
			}
			else
			{
				sb.Append(string.Format(OUTPUT_SCREEN_ITEM_FORMAT, totalItems+terminal.numberOfItemsInDropship, 12));
			}
			sb.Append(' ');
			if (terminal.groupCredits < x)
			{
				sb.Append(string.Format(COLORED_OUTPUT_SCREEN_CREDIT_FORMAT, "#8B0000", x));
			}
			else
			{
				sb.Append(string.Format(OUTPUT_SCREEN_CREDIT_FORMAT, x));
			}
			int currentSort = currentCursorMenu.sortingIndex;
			string sort = currentSort switch
			{
				0 => $"Sort: Alphabetical [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				1 => $"Sort: Price (Des.) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				2 => $"Sort: Price (Asc.) [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
				_ => $"Sort: None [{InteractiveTerminalAPI.Compat.InputUtils_Compat.ChangeApplicationSortingKey.GetBindingDisplayString()}]",
			};
			return $"{sort}|{sb.ToString()}";
		}
		void TryBuySelectedItems(Action backAction)
		{
			if (terminal.vehicleInDropship)
			{
				ErrorMessage(SCREEN_TITLE, VEHICLE_IN_POD_ERROR, backAction, "");
				return;
			}
			int totalItems = 0;
			int totalCost = 0;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < itemCursorElements.Length; i++)
			{
				ItemCursorElement itemCursor = itemCursorElements[i] as ItemCursorElement;
				int counter = itemCursor.Counter;
				if (counter == 0) continue;
				totalItems += counter;
				totalCost += itemCursor.ApplyFunction();
				sb.Append(string.Format(PURCHASE_LISTING_FORMAT, itemCursor.Counter, itemCursor.item.itemName, itemCursor.ApplyFunction()));
			}
			if (totalItems <= 0)
			{
				ErrorMessage(SCREEN_TITLE, NO_ITEMS_SELECTED_ERROR, backAction, "");
				return;
			}
			if (terminal.groupCredits < totalCost)
			{
				ErrorMessage(SCREEN_TITLE, NOT_ENOUGH_CREDITS_ERROR, backAction, "");
				return;
			}
			if (terminal.numberOfItemsInDropship + totalItems > 12)
			{
				ErrorMessage(SCREEN_TITLE, MAXIMUM_CAPACITY_ERROR, backAction, "");
				return;
			}
			Confirm(SCREEN_TITLE, string.Format(CONFIRM_PURCHASE_FORMAT, totalItems, totalCost, sb.ToString()), () => PurchaseSelectedItems(totalItems, totalCost, backAction), backAction);
		}
		const int PURCHASE_AUDIO_SYNCED_INDEX = 0;
		private void PurchaseSelectedItems(int totalItems, int totalCost, Action backAction)
		{
			terminal.groupCredits = Mathf.Clamp(terminal.groupCredits - totalCost, 0, 10000000);

			while (totalItems > 0)
			{
				for(int i = 0; i < itemCursorElements.Length && totalItems > 0 ;i++)
				{
					ItemCursorElement itemCursor = itemCursorElements[i] as ItemCursorElement;
					if (itemCursor == null) continue;
					int counter = itemCursor.Counter;
					if (counter == 0) continue;
					while ( counter > 0) 
					{
						terminal.orderedItemsFromTerminal.Add(i);
						terminal.numberOfItemsInDropship++;
						counter--;
						totalItems--;
					}
					itemCursor.Counter = 0;
				}
			}

			terminal.PlayTerminalAudioServerRpc(PURCHASE_AUDIO_SYNCED_INDEX);
			if (!terminal.IsServer)
			{
				terminal.SyncBoughtItemsWithServer(terminal.orderedItemsFromTerminal.ToArray(), terminal.numberOfItemsInDropship);
			}
			else
			{
				terminal.SyncGroupCreditsClientRpc(terminal.groupCredits, terminal.numberOfItemsInDropship);
			}
			backAction();
		}

		int ApplyInputFunction()
		{
			int result = 0;
			for(int i = 0; i < itemCursorElements.Length; i++)
			{
				result += itemCursorElements[i].ApplyFunction();
			}
			return result;
		}
		protected void Confirm(string title, string description, Action confirmAction, Action declineAction, string additionalMessage = "")
		{
			CursorOutputElement<int>[] cursorElements =
				[
					CursorOutputElement<int>.Create(name: "Confirm", action: confirmAction, showCounter: false),
					CursorOutputElement<int>.Create(name: "Cancel", action: declineAction, showCounter: false),
				];
			CursorMenu<CursorOutputElement<int>> cursorMenu = CursorMenu<CursorOutputElement<int>>.Create(elements: cursorElements);

			ITextElement[] elements =
				[
					TextElement.Create(description),
					TextElement.Create(" "),
					TextElement.Create(additionalMessage),
					cursorMenu
				];
			IScreen screen = BoxedScreen.Create(title: title, elements: elements);

			SwitchScreen(screen, cursorMenu, false);
		}
		protected void ErrorMessage(string title, string description, Action backAction, string error)
		{
			CursorOutputElement<int>[] cursorElements = [CursorOutputElement<int>.Create(name: "Back", action: backAction, showCounter: false)];
			CursorMenu<CursorOutputElement<int>> cursorMenu = CursorMenu<CursorOutputElement<int>>.Create(startingCursorIndex: 0, elements: cursorElements);
			ITextElement[] elements =
				[
					TextElement.Create(text: description),
					TextElement.Create(" "),
					TextElement.Create(text: error),
					TextElement.Create(" "),
					cursorMenu
				];
			IScreen screen = BoxedScreen.Create(title: title, elements: elements);
			SwitchScreen(screen, cursorMenu, false);
		}
	}
}
