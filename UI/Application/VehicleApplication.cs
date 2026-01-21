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
	internal class VehicleApplication : PageApplication<CursorElement>
	{
		#region String Constants
		const string SCREEN_TITLE = "The Company Store";
		const string INITIAL_SCREEN_DESCRIPTION = "Select the items you wish to purchase.";
		const string CONFIRM_PURCHASE_FORMAT = "Do you wish to purchase the selected item for {0}$ ? \n{1}";
		const string NOT_ENOUGH_CREDITS_ERROR = "You do not have enough Company Credits to purchase the selected item...";
		const string ITEMS_IN_POD_ERROR = "You are unable to purchase vehicles when there are items loaded in the pod...";
		const string VEHICLE_ALREADY_PURCHASED_ERROR = "You already have a vehicle in your possession...";
		const string VEHICLE_IN_DELIVERY_ERROR = "A vehicle is already being delivered to you...";
		#endregion
		BuyableVehicle[] buyableVehicles = null;
		int[] salePercentages = null;

		protected override int GetEntriesPerPage<K>(K[] entries)
		{
			return 10;
		}
		public override void Initialization()
		{
			buyableVehicles = terminal.buyableVehicles;
			salePercentages = terminal.itemSalesPercentages;

			(BuyableVehicle[][], BaseCursorMenu<CursorElement>[], IScreen[]) entries = GetPageEntries(buyableVehicles);
			BuyableVehicle[][] pagesItems = entries.Item1;
			BaseCursorMenu<CursorElement>[] cursorMenus = entries.Item2;
			IScreen[] screens = entries.Item3;

			for (int i = 0; i < pagesItems.Length; i++)
			{
				BuyableVehicle[] itemList = pagesItems[i];
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
					BuyableVehicle item = itemList[j];
					if (item == null) continue;
					int itemIndex = j + (i * itemList.Length);
					int pageIndex = i;
					int index = j;
					VehicleCursorElement cursor = new VehicleCursorElement(vehicle: buyableVehicles[itemIndex], salePercentage: (salePercentages[terminal.buyableItemsList.Length + itemIndex] / 100f), vehicleIndex: itemIndex, terminal: terminal);
					cursor.Action = () => TryBuySelectedItem(cursor, () => SwitchScreen(screens[pageIndex], cursorMenus[pageIndex], previous: true));
					elements[j] = cursor;
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
			if (terminal.numberOfItemsInDropship > 0)
			{
				ErrorMessage(SCREEN_TITLE, ITEMS_IN_POD_ERROR, backAction, "");
				return;
			}
			StringBuilder sb = new StringBuilder();
			VehicleCursorElement cursor = node as VehicleCursorElement;
			int totalCost = cursor.vehicle.creditsWorth;
			if (cursor.controller != null)
			{
				ErrorMessage(SCREEN_TITLE, VEHICLE_ALREADY_PURCHASED_ERROR, backAction, "");
				return;
			}
			if (terminal.groupCredits < totalCost && !terminal.hasWarrantyTicket)
			{
				ErrorMessage(SCREEN_TITLE, NOT_ENOUGH_CREDITS_ERROR, backAction, "");
				return;
			}
			if (terminal.vehicleInDropship)
			{
				ErrorMessage(SCREEN_TITLE, VEHICLE_IN_DELIVERY_ERROR, backAction, "");
				return;
			}
			BuyableVehicle item = cursor.vehicle;

			Confirm(SCREEN_TITLE, string.Format(CONFIRM_PURCHASE_FORMAT, terminal.hasWarrantyTicket ? 0 : totalCost, sb.ToString()), () => PurchaseSelectedItems(item, cursor.vehicleIndex, terminal.hasWarrantyTicket ? 0 : totalCost, backAction), backAction);
		}
		const int PURCHASE_AUDIO_SYNCED_INDEX = 0;
		private void PurchaseSelectedItems(BuyableVehicle vehicle, int vehicleIndex, int totalCost, Action backAction)
		{
			terminal.groupCredits = Mathf.Clamp(terminal.groupCredits - totalCost, 0, 10000000);

			terminal.PlayTerminalAudioServerRpc(PURCHASE_AUDIO_SYNCED_INDEX);
			terminal.orderedVehicleFromTerminal = vehicleIndex;
			terminal.vehicleInDropship = true;
			if (!terminal.IsServer)
			{
				terminal.SyncBoughtVehicleWithServer(terminal.orderedVehicleFromTerminal);
			}
			else
			{
				terminal.hasWarrantyTicket = !terminal.hasWarrantyTicket;
				terminal.BuyVehicleClientRpc(terminal.groupCredits, terminal.hasWarrantyTicket);
			}
			backAction();
		}

		int ApplyInputFunction()
		{
			int result = 0;
			return result;
		}
	}
}
