using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	public class GitBranch : GitModule
	{
		protected override void OnInit()
		{
			KeyDic.Add(KeyCode.W, SwitchBranch);

			// Reload
			KeyDic.Add(KeyCode.R, item =>
			{
				Fetch();
				OnEnter(null);
			});

			KeyDic.Add(KeyCode.Return, item => GUI.OpenSub(ModuleType.Log, item.displayName));
		}

		internal override ModuleType Type => ModuleType.Branch;
		internal override KeyCode KeyCode => KeyCode.B;
		internal override string Name => "Branch";

		private string _current;
		private string _newBranchName;

		internal override void OnEnter(object o)
		{
			_newBranchName = string.Empty;
			var branches = GetBranches();
			_current = CurrentBranch().Trim();

			var list = branches
				.Select((l, i) => new GitTreeViewItem
				{
					id = i,
					displayName = l.Replace("remotes/", "").Substring(2).Trim(),
					depth = 0
				}).Cast<TreeViewItem>()
				.ToList();

			TreeView.Set(list);
			TreeView.rowAction += RowAction;
		}

		internal override void OnExit()
		{
			TreeView.rowAction = null;
		}

		private void RowAction(Rect rect, GitTreeViewItem item)
		{
			rect.xMin += TreeView.DepthIndentWidth;
			EditorGUI.LabelField(rect, item.displayName, GetStyle(item));
		}

		private GUIStyle GetStyle(GitTreeViewItem item)
		{
			if (item.displayName == _current)
			{
				return EditorStyles.boldLabel;
			}

			if (item.displayName.StartsWith("origin/"))
			{
				return ColorLabel.Get(Color.yellow);
			}

			return ColorLabel.Get(Color.blue);
		}

		internal override void OnGUI(Rect rect)
		{
			rect.height -= EditorGUIUtility.singleLineHeight;
			base.OnGUI(rect);

			rect.y += rect.height;
			rect.height = EditorGUIUtility.singleLineHeight;

			rect.width /= 2;
			_newBranchName = EditorGUI.TextField(rect, "Switch New Branch", _newBranchName);
			rect.x += rect.width;
			if (UnityEngine.GUI.Button(rect, "Create"))
			{
				CreateBranch(_newBranchName.Replace("origin/", ""));
				OnEnter(null);
			}
		}

		/// <summary>
		/// ブランチ切り替え
		/// </summary>
		private void SwitchBranch(GitTreeViewItem item)
		{
			if (_current == item.displayName)
				return;

			// 差分がある
			if (GetStatus().Any())
			{
				if (!EditorUtility.DisplayDialog(
					"Warning",
					"There is a diff, want to switch branch?",
					"Yes",
					"No"))
					return;

				var output = Stash();
				EditorUtility.DisplayDialog("Info", output, "Ok");
			}
			else
			{
				if (!EditorUtility.DisplayDialog("Info", $"Switch {item.displayName}?", "Yes", "No"))
					return;
			}

			Switch(item.displayName);
			OnEnter(null);
		}
	}
}
