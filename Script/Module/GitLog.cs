using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	[Serializable]
	public class GitLog : GitModule
	{
		internal override ModuleType Type => ModuleType.Log;
		internal override KeyCode KeyCode => KeyCode.L;

		private float[] MaxWidth = {0f, 0f, 0f, 0f, 0f, 0f};
		private Color[] colors =
		{
			Color.black,
			Color.magenta,
			Color.blue,
			new Color(0, 0.7f, 0.1f),
			Color.black,
			Color.black,
		};

		internal override void OnEnter(object o)
		{
			var items = GetLog()
				.Select((l, index) => new GitTreeViewItem(l)
				{
					id = index
				});

			TreeView.Set(items.Cast<TreeViewItem>().ToList());

			var c = new GUIContent();
			var w = 0f;
			foreach (var item in items)
			{
				for (var i = 0; i < MaxWidth.Length; i++)
				{
					c.text = item[i];
					w = EditorStyles.label.CalcSize(c).x;
					if (w > MaxWidth[i])
						MaxWidth[i] = w;
				}
			}

			TreeView.rowAction += RowAction;
		}

		internal override void OnExit()
		{
			TreeView.rowAction = null;
		}

		private void RowAction(Rect rect, GitTreeViewItem item)
		{
			for (var i = 0; i < MaxWidth.Length; i++)
			{
				rect.width = MaxWidth[i] + EditorGUIUtility.standardVerticalSpacing * 4;
				EditorGUI.LabelField(rect, item[i], ColorLabel.Get(colors[i]));
				rect.xMin += rect.width;
			}
		}

		internal override void KeyEvent(KeyCode keyCode)
		{
			if (keyCode == KeyCode.Return)
			{
				var item = TreeView.GetSelectionItem();
				if (item != null)
				{
					var param = new DiffParam();
					param.SetHash(item.displayName);
					GUI.OpenSub(ModuleType.Diff, param);
				}
			}
		}
	}
}