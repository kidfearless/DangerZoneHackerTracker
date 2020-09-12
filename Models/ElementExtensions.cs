using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DangerZoneHackerTracker.Models
{
	public static class ElementExtensions
	{
		public static void SetGridColumn(this UIElement element, int column) => Grid.SetColumn(element, column);
		public static void SetGridRow(this UIElement element, int column) => Grid.SetRow(element, column);
	}
}
