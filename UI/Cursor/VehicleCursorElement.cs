using InteractiveTerminalAPI.UI.Cursor;
using System;
using System.Text;
using UnityEngine;

namespace InteractiveStore.UI.Cursor
{
	public class VehicleCursorElement : CursorElement
	{
		const int NAME_LENGTH = 20;
		const char WHITE_SPACE = ' ';
		public BuyableVehicle vehicle;
		public float salePercentage;
		public int vehicleIndex = -1;
		private Terminal terminal;

		internal VehicleController controller;

		public VehicleCursorElement(BuyableVehicle vehicle, float salePercentage, int vehicleIndex, Terminal terminal)
		{
			this.vehicle = vehicle;
			this.terminal = terminal;
			this.vehicleIndex = vehicleIndex;
			this.salePercentage = salePercentage;
			Active = (CursorElement x) => IsAvailableForPurchase(x);
			controller = UnityEngine.Object.FindObjectOfType<VehicleController>();
		}

		private bool IsAvailableForPurchase(CursorElement x)
		{
			int totalCost = vehicle.creditsWorth;
			return (terminal.groupCredits >= totalCost || terminal.hasWarrantyTicket) && terminal.numberOfItemsInDropship <= 0 && controller == null && !terminal.vehicleInDropship;
		}

		public override string GetText(int availableLength)
		{
			StringBuilder sb = new();
			int groupCredits = terminal.groupCredits;
			if (!Active(this))
			{
				sb.Append("<color=#66666666>");
			}
			string display = vehicle.vehicleDisplayName + WHITE_SPACE + (terminal.hasWarrantyTicket ? "(WARRANTY)" : "");
			string name = display.Length > NAME_LENGTH ? display.Substring(0, NAME_LENGTH) : display + new string(WHITE_SPACE, Mathf.Max(0, NAME_LENGTH - display.Length));
			sb.Append(name);


			int price = Mathf.CeilToInt(vehicle.creditsWorth);
			if (terminal.hasWarrantyTicket) sb.Append("<s>");
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
			if (terminal.hasWarrantyTicket) sb.Append("</s>");
			if (salePercentage < 1f)
			{
				sb.Append(WHITE_SPACE);
				sb.Append($"({(1f - salePercentage) * 100:F0}% OFF)");
			}
			if (!Active(this)) sb.Append("</color>");
			return sb.ToString();
		}
	}
}
