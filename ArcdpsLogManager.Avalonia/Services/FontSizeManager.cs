using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.VisualTree;
using GW2Scratch.ArcdpsLogManager.Configuration;

namespace GW2Scratch.ArcdpsLogManager.Avalonia.Services
{
	/// <summary>
	/// Writes the configured font sizes into the application's resource dictionary. Every font size
	/// in the XAML refers to one of these keys through <c>{DynamicResource}</c> (see App.axaml for
	/// the defaults and what each key is for), so replacing the values here restyles the running
	/// application without recreating any windows.
	/// </summary>
	/// <remarks>
	/// Three settings feed this: the base UI size, from which headings and small print are derived
	/// with fixed offsets, and a standalone size each for the log list and the encounter tree.
	/// </remarks>
	public static class FontSizeManager
	{
		public const int MinimumFontSize = 8;
		public const int MaximumFontSize = 32;

		/// <summary>
		/// What "Reset to defaults" restores, kept equal to the persisted defaults by definition.
		/// The literals in App.axaml are the same values again, since XAML cannot reference this;
		/// they only apply between the XAML loading and the first <see cref="Apply"/> call.
		/// </summary>
		public const int DefaultFontSize = StoredSettings.DefaultFontSize;

		public static void Apply(int uiFontSize, int logListFontSize, int encounterTreeFontSize)
		{
			var resources = Application.Current?.Resources;
			if (resources == null)
			{
				return;
			}

			double ui = Clamp(uiFontSize);

			resources["AppFontSize"] = ui;
			// Fluent sizes the content of most control templates (buttons, text boxes, check boxes,
			// combo boxes, …) with this key rather than with the inherited font size.
			resources["ControlContentThemeFontSize"] = ui - 1;
			resources["AppFontSizeSmall"] = ui - 2;
			resources["AppFontSizeMedium"] = ui + 1;
			resources["AppFontSizeLarge"] = ui + 3;
			resources["AppFontSizeHeader"] = ui + 5;

			// The log list's row height is driven by three things that all have to follow the font,
			// or the rows simply refuse to get denser: Fluent's DataGridCell/DataGridColumnHeader
			// control themes pin MinHeight to 32, and the per-row icons have fixed pixel sizes.
			// Below roughly 25pt the text is the shortest of the three, so without these the font
			// setting appeared to do nothing at all to the density.
			double logList = Clamp(logListFontSize);
			resources["LogListFontSize"] = logList;
			resources["LogListRowMinHeight"] = logList + 11;
			resources["LogListIconSize"] = logList + 7;
			resources["LogListSmallIconSize"] = logList + 5;

			// Same reasoning as the log list, with the tree's own row floor and icon size.
			double tree = Clamp(encounterTreeFontSize);
			resources["EncounterTreeFontSize"] = tree;
			resources["EncounterTreeItemMinHeight"] = tree + 11;
			resources["EncounterTreeIconSize"] = tree + 3;

			InvalidateLayout();
		}

		/// <summary>
		/// Marks the layout of every open window as dirty, forcing a full re-measure on the next
		/// layout pass.
		/// </summary>
		/// <remarks>
		/// Changing a font size resource does not reliably resize the controls whose size depends on
		/// it: the text is re-laid-out and drawn at the new size, but the ancestor that sizes itself
		/// to that text keeps the width it was measured at, and since a <c>TextBlock</c> clips to its
		/// own bounds by default the overflow is simply cut off. The tab headers showed this clearly —
		/// enlarging the interface font cut their labels off on the right until the app was
		/// restarted, at which point they measured correctly. Invalidating the whole tree is
		/// deliberately broad rather than aimed at the tabs alone, since anything sized to text is
		/// susceptible; it costs a single extra layout pass on a rare, user-initiated change.
		/// </remarks>
		private static void InvalidateLayout()
		{
			if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
			{
				return;
			}

			// At startup this runs before any window exists, and is simply a no-op.
			foreach (var window in desktop.Windows.ToList())
			{
				window.InvalidateMeasure();
				foreach (var layoutable in window.GetVisualDescendants().OfType<Layoutable>())
				{
					layoutable.InvalidateMeasure();
				}
			}
		}

		public static int Clamp(int fontSize) => Math.Clamp(fontSize, MinimumFontSize, MaximumFontSize);
	}
}
