using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace Yorozu.EditorTools.Git
{
	internal class DiffParam
	{
		/// <summary>
		/// Log が Showか
		/// </summary>
		public bool isLog;
		public string Str;
		public string PrevFileName;
		public bool IsStaged;

		internal void SetHash(string hash)
		{
			isLog = true;
			Str = hash;
		}

		internal void SetFile(string file, bool isStaged)
		{
			isLog = false;
			IsStaged = isStaged;

			Str = file;
			PrevFileName = "";
			var index = file.IndexOf("->");
			// Rename
			if (index >= 0)
			{
				PrevFileName = file.Substring(0, index - 1);
				Str = file.Substring(index + 3);
			}
		}
	}

	[Serializable]
	public class GitDiff : GitModule
	{
		internal override ModuleType Type => ModuleType.Diff;
		internal override KeyCode KeyCode => KeyCode.D;
		internal override string Name => "Diff";

		protected override void OnInit()
		{
			KeyDic.Add(new ShortCut(KeyCode.Return, "Jump", Jump));
		}

		protected override void OnEnter(object o)
		{
			if (o == null)
			{
				GUI.Transition(ModuleType.Log);
				return;
			}

			var param = (DiffParam) o;
			var list = param.isLog ?
					GetLogList(param) :
					GetDiffList(param);

			TreeView.SetRowAction(RowAction);
			TreeView.Set(list);
			TreeView.SetSelection(new List<int>());
			TreeView.SetFocusAndEnsureSelectedItem();
		}

		private List<TreeViewItem> GetLogList(DiffParam param)
		{
			var mode = 0;
			var filePath = "";
			var line = 0;

			return GetShow(param.Str)
				.Select((l, i) =>
				{
					if (mode == 0)
					{
						if (!l.Contains("files changed,") && !l.Contains("file changed,"))
						{
							var index = l.IndexOf("|");
							filePath = l.Substring(0, index).Trim();
						}
						else
						{
							mode++;
						}
					}
					else if (mode == 1)
					{
						filePath = "";
						mode++;
					}
					else if (mode == 2)
					{
						if (l.StartsWith("+++ b/"))
						{
							var index = l.IndexOf("+++ b/") + 7;
							filePath = l.Substring(index);
							line = 0;
						}
						else if (l.StartsWith("@@"))
						{
							var index = l.IndexOf("+");
							var s = l.Substring(index + 1);
							index = s.IndexOf(",");
							if (int.TryParse(s.Substring(0, index), out line))
							{
								line--;
							}
						}
						else if (!l.StartsWith("-"))
						{
							line++;
						}
					}

					return new GitTreeViewItem()
					{
						id = i,
						displayName = l,
						depth = 0,
						FilePath = filePath,
						FileLine = line,
					};
				})
				.Cast<TreeViewItem>()
				.ToList();

		}

		private List<TreeViewItem> GetDiffList(DiffParam param)
		{
			var line = 0;
			var filePath = param.Str;
			return GetDiff("\'" + param.Str + "\'", "\'" + param.PrevFileName + "\'", param.IsStaged)
				.Select((l, i) =>
				{
					if (l.StartsWith("@@"))
					{
						var index = l.IndexOf("+");
						var s = l.Substring(index + 1);
						index = s.IndexOf(",");
						if (int.TryParse(s.Substring(0, index), out line))
						{
							line--;
						}
					}
					else if (!l.StartsWith("-"))
					{
						line++;
					}
					return new GitTreeViewItem()
					{
						id = i,
						displayName = l,
						depth = 0,
						FilePath = filePath,
						FileLine = line,
					};
				})
				.Cast<TreeViewItem>()
				.ToList();
		}

		private void RowAction(Rect rect, GitTreeViewItem item)
		{
			rect.x = 0;
			EditorGUI.LabelField(rect, item.displayName, GetStyle(item.displayName));
		}

		private GUIStyle GetStyle(string text)
		{
			if (text.StartsWith("+++") ||
			    text.StartsWith("---") ||
			    text.StartsWith("diff ") ||
			    text.StartsWith("Date:"))
			{
				return ColorLabel.Get(Color.yellow);
			}

			if (text.StartsWith("index ") ||
			    text.StartsWith("Author:") ||
			    text.StartsWith("commit "))
			{
				return ColorLabel.Get(Color.blue);
			}

			if (text.StartsWith("+"))
			{
				return ColorLabel.Get(new Color(0, 0.7f, 0.1f));
			}

			if (text.StartsWith("-"))
			{
				return ColorLabel.Get(Color.red);
			}

			if (text.StartsWith("@@"))
			{
				return ColorLabel.Get(Color.magenta);
			}


			return ColorLabel.Get(Color.black);
		}

		private void Jump(GitTreeViewItem item)
		{
			if (string.IsNullOrEmpty(item.FilePath))
				return;

			// TODO 存在しないファイルの場合
			InternalEditorUtility.OpenFileAtLineExternal(item.FilePath, item.FileLine);
		}
	}
}
