using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	[Serializable]
	public abstract class GitModule
	{
		internal GitTreeView TreeView;
		[SerializeField]
		protected  GitGUI GUI;

		internal bool Lock;

		protected Dictionary<KeyCode, Action<GitTreeViewItem>> KeyDic = new Dictionary<KeyCode, Action<GitTreeViewItem>>();

		internal IEnumerable<KeyCode> ShortCuts => KeyDic.Keys;

		internal void Init(GitGUI window)
		{
			GUI = window;
			OnInit();
		}

		protected virtual void OnInit()
		{
		}

		internal abstract ModuleType Type { get; }
		internal abstract string Name { get; }

		internal virtual void OnGUI(Rect rect)
		{
			TreeView.OnGUI(rect);
		}

		internal abstract void OnEnter(object o);
		internal abstract void OnExit();

		internal abstract KeyCode KeyCode { get; }

		internal virtual void SingleClick(GitTreeViewItem item) { }

		internal virtual void DoubleClick(GitTreeViewItem item)
		{
			KeyEvent(item, KeyCode.Return);
		}

		internal void KeyEvent(GitTreeViewItem item, KeyCode keyCode)
		{
			if (!KeyDic.ContainsKey(keyCode))
				return;

			KeyDic[keyCode].Invoke(item);
		}

		protected void Add(string path)
		{
			Command.Exec($"git add {path}");
		}

		protected void Fetch()
		{
			Command.Exec($"git fetch --prune");
		}

		protected void Reset(string path)
		{
			Command.Exec($"git reset HEAD {path}");
		}

		protected void Checkout(string path)
		{
			Command.Exec($"git checkout -- {path}");
		}

		protected void Clean(string path)
		{
			Command.Exec($"git clean -fd {path}");
		}

		protected void Switch(string branch)
		{
			Command.Exec($"git checkout {branch}");
		}

		protected void CreateBranch(string branch)
		{
			if (string.IsNullOrEmpty(branch))
				return;

			Command.Exec($"git checkout -b {branch}");
		}

		protected string Stash()
		{
			return Command.Exec($"git stash -u");
		}

		protected string CurrentBranch()
		{
			return Command.Exec("git symbolic-ref --short HEAD");
		}

		protected IEnumerable<string> GetShow(string hash)
		{
			return string.Join("\n",
					Command.Exec($"git show {hash} --stat --pretty=\"\""),
					Command.Exec($"git show {hash}")
				)
				.Split('\n');
		}

		protected IEnumerable<string> GetDiff(string path, string prev, bool isStaged)
		{
			var command = isStaged ? $"git diff --staged -- {prev} {path}" : $"git diff -- {prev} {path}";
			//return Command.Exec($"git diff HEAD -M -- {prev} {path}")
			return Command.Exec(command)
				.Split('\n')
				.Where(l => !string.IsNullOrEmpty(l));
		}

		protected IEnumerable<string> GetStatus()
		{
			return Command.Exec("git status -s -u")
				.Split('\n')
				.Where(l => !string.IsNullOrEmpty(l));
		}

		protected IEnumerable<string> GetBranches()
		{
			return Command.Exec("git branch -a")
				.Split('\n')
				.Where(l => !string.IsNullOrEmpty(l));
		}

		protected IEnumerable<TreeViewItem> GetStage(IEnumerable<string> output, int depth = 1, int startIndex = 0)
		{
			return output
				.Where(l => l[0] == 'M' || l[0] == 'D' || l[0] == 'A' || l[0] == 'R')
				.Select((l, i) => new GitTreeViewItem(l, GitStatusType.Stage)
				{
					id = startIndex + i,
					depth = depth,
				});
		}

		protected IEnumerable<TreeViewItem> GetUnStage(IEnumerable<string> output, int depth = 1, int startIndex = 0)
		{
			return output
				.Where(l => l[1] == 'M' || l[1] == 'D' || l[1] == 'A')
				.Select((l, i) => new GitTreeViewItem(l, GitStatusType.UnStage)
				{
					id = startIndex + i,
					depth = depth,
				});
		}

		protected IEnumerable<TreeViewItem> GetUntrack(IEnumerable<string> output, int depth = 1, int startIndex = 0)
		{
			return output
				.Where(l => l[0] == '?')
				.Select((l, i) => new GitTreeViewItem(l, GitStatusType.UnTrack)
				{
					id = startIndex + i,
					depth = depth,
				});
		}

		protected IEnumerable<string> GetLog(string branch = "")
		{
			var command = "git log " + branch + " --graph --format=\"hash={%h} date={%ad} author={%an} branch={%d} commit=%s\" --date=iso -100";
			return Command.Exec(command)
				.Split('\n')
				.Where(l => !string.IsNullOrEmpty(l));
		}


	}
}
