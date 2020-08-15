using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	[Serializable]
	public class GitStatus : GitModule
	{
		internal override ModuleType Type => ModuleType.Status;
		internal override KeyCode KeyCode => KeyCode.S;
		internal override string Name => "Status";

		protected override void OnInit()
		{
			KeyDic.Add(KeyCode.U, ChangeStage);
			// Refresh
			KeyDic.Add(KeyCode.R, item => Refresh(TreeView.GetSelectionIndex()));
			KeyDic.Add(KeyCode.Z, ChangeUndo);
			KeyDic.Add(KeyCode.Return, item => {
				if (item.depth != 1)
					return;
				var param = new DiffParam();
				param.SetFile(item.displayName, item.Status == GitStatusType.Stage);
				GUI.OpenSub(ModuleType.Diff, param);
			});
			KeyDic.Add(KeyCode.T, item =>
			{
				if (item.depth != 1)
					return;

				// スクリプト
				if (item.displayName.EndsWith(".cs"))
				{
					InternalEditorUtility.OpenFileAtLineExternal(item.displayName, 0);

					return;
				}

				var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.displayName);
				Selection.objects = new[] {asset};
			});
		}

		internal override void OnEnter(object o)
		{
			Refresh((int?) o ?? 0);

			TreeView.rowAction += Row;
		}

		private void Refresh(int index = 0)
		{
			TreeView.Set(Status());
			TreeView.ExpandAll();
			var rows = TreeView.GetRows();
			if (rows.Count <= 0)
				return;

			index = Mathf.Clamp(index, 0, rows.Count - 1);
			TreeView.SetSelection(new List<int>{rows[index].id});
		}

		internal override void OnExit()
		{
			TreeView.rowAction = null;
		}

		private void Row(Rect rect, GitTreeViewItem item)
		{
			EditorGUI.LabelField(
				new Rect(4, rect.y, 12, rect.height),
				$"{item.GetStatusText()}",
				ColorLabel.Get(Color.magenta));

			rect.xMin += (item.depth + 1) * TreeView.DepthIndentWidth;

			EditorGUI.LabelField(
				rect,
				item.displayName,
				item.depth == 0 ?
					EditorStyles.boldLabel :
					EditorStyles.label
			);
		}

		internal override void DoubleClick(GitTreeViewItem item)
		{
			KeyEvent(item, KeyCode.U);
		}

		private string GetPath(GitTreeViewItem item)
		{
			// 0なら子供を全部変更する
			return item.depth == 0 && item.children != null
				? string.Join(" ", item.children.Select(i => (i as GitTreeViewItem).StatusFilePath))
				: item.StatusFilePath;
		}

		private void ChangeStage(GitTreeViewItem item)
		{
			var path = GetPath(item);
			switch (item.Status)
			{
				case GitStatusType.UnStage:
				case GitStatusType.UnTrack:
					Add(path);
					break;
				case GitStatusType.Stage:
					Reset(path);
					break;
			}

			Refresh(TreeView.GetSelectionIndex());
		}

		private void ChangeUndo(GitTreeViewItem item)
		{
			var path = GetPath(item);
			if (item.Status == GitStatusType.UnStage)
			{
				Checkout(path);
				Refresh(TreeView.GetSelectionIndex());
			}

			if (item.Status == GitStatusType.UnTrack)
			{
				Clean(path);
				Refresh(TreeView.GetSelectionIndex());
			}
		}

		private List<TreeViewItem> Status()
		{
			var output = GetStatus();

			return new List<TreeViewItem>
			{
				GetStageFiles(output, 0),
				GetUnStageFiles(output, 1000000),
				UntrackFiles(output, 10000000),
			};
		}

		private TreeViewItem GetStageFiles(IEnumerable<string> output, int startIndex)
		{
			var ctc = new GitTreeViewItem(GitStatusType.Stage)
			{
				displayName = "Changes to be committed",
				id = -2,
				depth = 0
			};

			foreach (var i in GetStage(output, 1, startIndex))
				ctc.AddChild(i);

			return ctc;
		}

		private TreeViewItem GetUnStageFiles(IEnumerable<string> output, int startIndex)
		{
			var cns = new GitTreeViewItem(GitStatusType.UnStage)
			{
				displayName = "Changes not staged for commit",
				id = -3,
				depth = 0
			};

			foreach (var i in GetUnStage(output, 1, startIndex))
				cns.AddChild(i);

			return cns;
		}

		private TreeViewItem UntrackFiles(IEnumerable<string> output, int startIndex)
		{
			var uf = new GitTreeViewItem(GitStatusType.UnTrack)
			{
				displayName = "Untracked files",
				id = -4,
				depth = 0
			};
			var items = output
				.Where(l => l[0] == '?')
				.Select((l, i) => new GitTreeViewItem(l,GitStatusType.UnTrack)
				{
					id = startIndex + i,
					depth = 1,
				})
				.Cast<TreeViewItem>();

			foreach (var i in items)
				uf.AddChild(i);

			return uf;
		}
	}
}
