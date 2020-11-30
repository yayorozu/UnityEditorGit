using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTool.Git
{
	internal class GitTreeView : TreeView
	{
		private List<TreeViewItem> _list;

		private Action<Rect, GitTreeViewItem> rowAction;
		internal float DepthIndentWidth => depthIndentWidth;

		internal Action<GitTreeViewItem> SingleClickAction;
		internal Action<GitTreeViewItem> DoubleClickAction;
		internal Action<KeyCode> KeyEventAction;

		internal GitTreeView(TreeViewState state) : base(state)
		{
			showBorder = true;
			showAlternatingRowBackgrounds = true;
			_list = new List<TreeViewItem>();
			rowAction = null;

			Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			var root = new TreeViewItem(-1, -1);
			SetupParentsAndChildrenFromDepths(root, _list);
			return root;
		}

		/// <summary>
		/// 子供をセット
		/// </summary>
		internal void Set(List<TreeViewItem> list)
		{
			_list = list;
			Reload();
		}

		internal void Clear()
		{
			_list.Clear();
			SetSelection(new List<int>());
			Reload();
		}

		internal TreeViewItem GetSelectionItem()
		{
			var selectionIds = GetSelection();
			if (selectionIds.Count <= 0)
				return null;

			var rows = FindRows(selectionIds);

			if (rows.Count <= 0)
				return null;

			return rows.First();
		}

		/// <summary>
		/// 選択しているIndex を取得する
		/// </summary>
		/// <returns></returns>
		internal int GetSelectionIndex()
		{
			var selectionIds = GetSelection();
			if (selectionIds.Count <= 0)
				return -1;

			var item = FindRows(selectionIds).First();
			var rows = GetRows();

			return rows.IndexOf(item);
		}

		protected override bool CanChangeExpandedState(TreeViewItem item)
		{
			return false;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			if (rowAction != null)
			{
				rowAction.Invoke(args.rowRect, args.item as GitTreeViewItem);
				return;
			}

			base.RowGUI(args);
		}

		protected override void SingleClickedItem(int id)
		{
			SingleClickAction?.Invoke(FindRows(new []{id}).First() as GitTreeViewItem);
		}

		protected override void DoubleClickedItem(int id)
		{
			DoubleClickAction?.Invoke(FindRows(new []{id}).First() as GitTreeViewItem);
		}

		protected override void KeyEvent()
		{
			var ev = Event.current;
			if (ev.type != EventType.KeyDown)
				return;

			if (ev.keyCode == KeyCode.DownArrow ||
			    ev.keyCode == KeyCode.UpArrow ||
			    ev.keyCode == KeyCode.J ||
			    ev.keyCode == KeyCode.K)
			{
				var selectionIds = GetSelection();

				if (selectionIds.Count > 0)
				{
					var item = FindRows(selectionIds).First();
					var rows = GetRows();
					var index = rows.IndexOf(item);
					if (ev.keyCode == KeyCode.UpArrow || ev.keyCode == KeyCode.K)
						index--;
					else
						index++;

					if (index < 0)
						index = rows.Count - 1;
					if (index >= rows.Count)
						index = 0;

					SetSelection(new List<int> {rows[index].id});
					SetFocusAndEnsureSelectedItem();
					Reload();
				}
				else if (GetRows().Count > 0)
				{
					SetSelection(new List<int> {GetRows().First().id});
					SetFocusAndEnsureSelectedItem();
					Reload();
				}
			}
			else
			{
				KeyEventAction?.Invoke(ev.keyCode);
			}
			ev.Use();
		}

		protected override bool CanMultiSelect(TreeViewItem item)
		{
			return false;
		}

		internal void SetRowAction(Action<Rect, GitTreeViewItem> action = null)
		{
			rowAction = action;
		}
	}
}
