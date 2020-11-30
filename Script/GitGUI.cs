using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTool.Git
{
	[Serializable]
	public class GitGUI
	{
		[SerializeField]
		private int _main;
		[SerializeField]
		private int _sub;
		[SerializeField]
		private bool _hasSub;

		private GitTreeView _treeView;
		private TreeViewState _state;

		private GitTreeView _subTreeView;
		private TreeViewState _subState;

		[SerializeReference]
		private List<GitModule> _modules;

		private GUIContent _content = new GUIContent();

		private void Init()
		{
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
					.ToList();

				_hasSub = false;
				_sub = -1;
				_main = (int) ModuleType.Log;
				_modules[_main].Enter(null);
			}


			if (_state == null)
				_state = new TreeViewState();
			if (_treeView == null)
			{
				_treeView = new GitTreeView(_state);
				_treeView.KeyEventAction += KeyEvent;
				_treeView.SingleClickAction += SingleClick;
				_treeView.DoubleClickAction += DoubleClick;

				foreach (var m in _modules)
					m.Init(this);

				_modules[_main].TreeView = _treeView;
				_modules[_main].Reload();
			}

			if (_subState == null)
				_subState = new TreeViewState();
			if (_subTreeView == null)
			{
				_subTreeView = new GitTreeView(_subState);
				_subTreeView.KeyEventAction += KeyEvent;
				_subTreeView.SingleClickAction += SingleClick;
				_subTreeView.DoubleClickAction += DoubleClick;
				if (_hasSub)
				{
					_modules[_sub].TreeView = _subTreeView;
					_modules[_sub].Reload();
				}
			}
		}

		public void OnGUI()
		{
			Init();

			var rect = GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue);

			var tr = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
			// Toolbar
			{
				_content.text = "Mode:";
				tr.width = EditorStyles.boldLabel.CalcSize(_content).x;
				EditorGUI.LabelField(tr, _content, EditorStyles.boldLabel);
				tr.x += tr.width + EditorGUIUtility.standardVerticalSpacing;

				foreach (var module in _modules)
				{
					var style = (int) module.Type == _main ? EditorStyles.boldLabel : EditorStyles.label;
					_content.text = $"[{module.KeyCode}:{module.Name}]";
					tr.width = style.CalcSize(_content).x;
					if (GUI.Button(tr, _content, style))
					{
						Transition(module.Type);
						GUIUtility.ExitGUI();
					}
					tr.x += tr.width + EditorGUIUtility.standardVerticalSpacing;
				}

				tr.x += 10;
				_content.text = "ShortCut:";
				tr.width = EditorStyles.boldLabel.CalcSize(_content).x;
				EditorGUI.LabelField(tr, _content, EditorStyles.boldLabel);
				tr.x += tr.width + EditorGUIUtility.standardVerticalSpacing;

				_content.text = string.Join(", ", _modules[_main].ShortCuts
					.Select(l => $"[{l.KeyCode}:{l.Description}]"));
				tr.width = EditorStyles.boldLabel.CalcSize(_content).x;
				EditorGUI.LabelField(tr, _content, EditorStyles.label);
			}

			rect.y += EditorGUIUtility.singleLineHeight;
			rect.height -= EditorGUIUtility.singleLineHeight;

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
				_modules[_sub].Exit();
				_subTreeView.Clear();
			}
			_hasSub = false;

			_modules[_main].Exit();
			_main = (int) type;
			_treeView.Clear();
			_modules[_main].TreeView = _treeView;
			_modules[_main].Enter(obj);
		}

		/// <summary>
		/// サブWindowOpen
		/// </summary>
		internal void OpenSub(ModuleType type, object param = null)
		{
			if (_sub != (int) type)
			{
				CloseSub();
				_sub = (int) type;
			}

			_subTreeView.Clear();
			_modules[_sub].TreeView = _subTreeView;
			_modules[_sub].Enter(param);
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

			_modules[_sub].Exit();
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
			var c = _modules[target].TreeView;
			var item = c.GetSelectionItem();
			if (item == null)
				return;

			_modules[target].KeyEvent(item as GitTreeViewItem, keyCode);
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
