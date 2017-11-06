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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using libgitface.ActionProviders;

namespace libgitface
{
	public abstract class ActionCentre : IActionCentre
	{
		public ObservableCollection<IActionProvider> ActionProviders {
			get;
		}

		public SynchronizationContext SyncContext {
			get;
		}

		protected ActionCentre ()
			: this (null)
		{
		}

		protected ActionCentre (SynchronizationContext syncContext)
		{
			ActionProviders = new ObservableCollection<IActionProvider> ();
			ActionProviders.CollectionChanged += HandleActionProvidersChanged;
			SyncContext = syncContext;
		}

		protected virtual void OnAvailableActionsChanged (IAction[] availableActions)
		{

		}

		void HandleActionProvidersChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add) {
				foreach (IActionProvider provider in e.NewItems)
					provider.StatusChanged += HandleActionProviderStateChanged;
			} else if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (IActionProvider provider in e.NewItems)
					provider.StatusChanged -= HandleActionProviderStateChanged;
			} else {
				throw new NotSupportedException ();
			}
		}

		void HandleActionProviderStateChanged (object sender, EventArgs e)
		{
			var actions = ActionProviders.SelectMany (t => t.Actions).ToArray ();
			if (SyncContext == null)
				OnAvailableActionsChanged (actions);
			else
				SyncContext.Post (t => OnAvailableActionsChanged (actions), null);
		}

		public async Task Refresh ()
		{
			foreach (IActionProvider provider in ActionProviders) {
				try {
					await provider.Refresh ();
				} catch (Exception ex) {
					Console.WriteLine ("Failed to refresh '{0}' due to: {1}", provider.GetType ().Name, ex);
				}
			}
		}
	}
}
