// MIT License
// 
// Copyright (c) 2017 Alan McGovern
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using AppKit;
using libgitface;

namespace GityMcGitface
{
	public class ApplicationIconActionCenter : ActionCentre
	{
		NSStatusItem StatusItem {
			get;
		}

		public ApplicationIconActionCenter (NSStatusItem statusItem, SynchronizationContext syncContext)
			: base (syncContext)
		{
			StatusItem = statusItem;
			StatusItem.Button.Image = NSImage.ImageNamed (NSImageName.StatusAvailable);
		}

		protected override void OnAvailableActionsChanged (IAction[] availableActions)
		{
			if (availableActions.Length == 0) {
				StatusItem.Button.Image = NSImage.ImageNamed (NSImageName.StatusAvailable);
			} else {
				StatusItem.Button.Image = NSImage.ImageNamed (NSImageName.StatusUnavailable);
			}
			StatusItem.Menu = CreateMenu (availableActions);
		}

		NSMenu CreateMenu (IAction[] actions)
		{
			var refresher = new NSMenuItem ("Refresh");
			refresher.Activated += (o, e) => Refresh ();

			return new NSMenu {
				Delegate = new ActionMenuDelegate (actions, 0, refresher)
			};
		}

		class ActionMenuDelegate : NSMenuDelegate
		{
			int Depth {
				get;
			}

			IGrouping<string[], IAction>[] Grouping {
				get;
			}

			NSMenuItem Refresher {
				get;
			}

			public ActionMenuDelegate (IAction[] actions, int depth)
				: this (actions, depth, null)
			{
			}

			public ActionMenuDelegate (IAction[] actions, int depth, NSMenuItem refresher)
			{
				Depth = depth;
				Grouping = actions.GroupBy (t => t.Grouping.Skip (depth).ToArray (), Groupings.GroupingComparer).OrderBy (g => g.Key.FirstOrDefault ()).ToArray ();
				Refresher = refresher;
			}

			public override void NeedsUpdate (NSMenu menu)
			{
				menu.RemoveAllItems ();
				if (Refresher != null) {
					menu.AddItem (Refresher);
					menu.AddItem (NSMenuItem.SeparatorItem);
				}

				foreach (var group in Grouping) {
					if (group.Key.Length == 0) {
						foreach (var action in group.OrderBy (t => t.ShortDescription)) {
							var menuItem = new NSMenuItem (action.ShortDescription);
							menuItem.ToolTip = action.Tooltip;
							menuItem.Activated += (sender, e) => action.Execute ();
							menu.AddItem (menuItem);
						}
					} else {
						var subMenu = new NSMenu {
							Delegate = new ActionMenuDelegate (group.ToArray (), Depth + 1)
						};
						var submenuItem = new NSMenuItem (group.Key.FirstOrDefault () ?? "Misc");
						menu.SetSubmenu (subMenu, submenuItem);
						menu.AddItem (submenuItem);
					}
				}
			}

			public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
			{
				// No-op
			}
		}
	}
}
