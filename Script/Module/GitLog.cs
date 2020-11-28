using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTools.Git
{
	[Serializable]
	public class GitLog : GitModule
	{
		internal override ModuleType Type => ModuleType.Log;
		internal override KeyCode KeyCode => KeyCode.L;
		internal override string Name => "Log";

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

		protected override void OnInit()
		{
			KeyDic.Add(new ShortCut(KeyCode.Return, "Show Diff", item =>
			{
				var param = new DiffParam();
				param.SetHash(item.displayName);
				GUI.OpenSub(ModuleType.Diff, param);
			}));
			KeyDic.Add(new ShortCut(KeyCode.R, "Reload", item => OnEnter(CurrentBranch())));
			KeyDic.Add(new ShortCut(KeyCode.P, "Push", item =>
			{
				var branch = CurrentBranch();
				if (EditorUtility.DisplayDialog("Warning", $"Try to Push \"{branch}\" ?", "Yes", "No"))
				{
					Push(branch);
					OnEnter(branch);
				}
			}));
			KeyDic.Add(new ShortCut(KeyCode.F, "Pull", item =>
			{
				var cb = CurrentBranch();
				if (cb.StartsWith("origin/"))
					return;

				var output = GetStatus();
				// 差分があった場合
				if (output.Count() - GetUntrack(output).Count() > 0)
				{
					if (!EditorUtility.DisplayDialog(
						"Warning",
						"There is a diff, want to pull rebase?",
						"Yes",
						"No"))
						return;

					var o2 = Stash();
					EditorUtility.DisplayDialog("Info", o2, "Ok");
				}

				var o3 = PullRebase(cb);
				EditorUtility.DisplayDialog("Info", o3, "Ok");
				OnEnter(cb);
			}));
		}

		protected override void OnEnter(object o)
		{
			var items = GetLog(o != null ? (string) o : "")
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

			TreeView.SetRowAction(RowAction);
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
	}
}
