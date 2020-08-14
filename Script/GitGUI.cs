using System;
using System.Linq;
using Boo.Lang;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	[Serializable]
	public class GitGUI
	{
		private int _main;
		private int _sub;
		private bool _hasSub;

		private GitTreeView _treeView;
		private TreeViewState _state;

		private GitTreeView _subTreeView;
		private TreeViewState _subState;

		[SerializeField]
		private GitModule[] _modules;

		private void Init()
		{
			if (_state == null)
				_state = new TreeViewState();
			if (_treeView == null)
			{
				_treeView = new GitTreeView(_state);
				_treeView.KeyEventAction += KeyEvent;
				_treeView.SingleClickAction += SingleClick;
				_treeView.DoubleClickAction += DoubleClick;
			}

			if (_subState == null)
				_subState = new TreeViewState();
			if (_subTreeView == null)
			{
				_subTreeView = new GitTreeView(_subState);
				_subTreeView.KeyEventAction += KeyEvent;
				_subTreeView.SingleClickAction += SingleClick;
				_subTreeView.DoubleClickAction += DoubleClick;
			}

			if (_modules == null)
			{
				var type = typeof(GitModule);
				_modules = type.Assembly.GetTypes()
					.Where(t => !t.IsAbstract && t.IsSubclassOf(type))
					.Select(t =>
					{
						var i = Activator.CreateInstance(t, true) as GitModule;
						i.Init(this);

						return i;
					})
					.OrderBy(i => (int) i.Type)
					.ToArray();

				_hasSub = false;
				_sub = -1;
				_main = (int) ModuleType.Log;
				_treeView.Clear();
				_modules[_main].TreeView = _treeView;
				_modules[_main].OnEnter(null);
			}
		}


		public void OnGUI(Rect rect)
		{
			Init();

			if (_hasSub)
				rect.width /= 2;

			_modules[_main].OnGUI(rect);

			if (!_hasSub)
				return;

			rect.x += rect.width;
			_modules[_sub].OnGUI(rect);
		}

		internal void Transition(ModuleType type, object obj = null)
		{
			if (_main == (int) type)
				return;

			if (_hasSub)
			{
				_modules[_sub].OnExit();
				_subTreeView.Clear();
			}
			_hasSub = false;

			_modules[_main].OnExit();
			_main = (int) type;
			_treeView.Clear();
			_modules[_main].TreeView = _treeView;
			_modules[_main].OnEnter(obj);
		}

		/// <summary>
		/// サブWindowOpen
		/// </summary>
		internal void OpenSub(ModuleType type, object param = null)
		{
			if (_sub == (int) type)
				return;

			CloseSub();

			_sub = (int) type;
			_subTreeView.Clear();
			_modules[_sub].TreeView = _subTreeView;
			_modules[_sub].OnEnter(param);
			_hasSub = true;
			_subTreeView.SetFocus();
		}

		/// <summary>
		/// サブWindowClose
		/// </summary>
		internal void CloseSub()
		{
			if (!_hasSub)
				return;

			_modules[_sub].OnExit();
			_sub = -1;
			_hasSub = false;
			_treeView.SetFocus();
		}

		private void KeyEvent(KeyCode keyCode)
		{
			if (_hasSub && keyCode == KeyCode.Q)
			{
				CloseSub();
				return;
			}

			if (!_modules[_main].Lock)
			{
				if (keyCode == KeyCode.Q && _main != (int) ModuleType.Log)
				{
					Transition(ModuleType.Log);
					return;
				}
				foreach (var module in _modules)
				{
					if (keyCode == module.KeyCode && (int) module.Type != _main)
					{
						Transition(module.Type);
						return;
					}
				}
			}

			var target = _hasSub && _subTreeView.HasFocus() ? _sub : _main;
			_modules[target].KeyEvent(keyCode);
		}

		private void SingleClick(GitTreeViewItem item)
		{
			var index = _hasSub && _subTreeView.HasFocus() ? _sub : _main;
			_modules[index].SingleClick(item);
		}

		private void DoubleClick(GitTreeViewItem item)
		{
			var index = _hasSub && _subTreeView.HasFocus() ? _sub : _main;
			_modules[index].DoubleClick(item);
		}
	}
}
