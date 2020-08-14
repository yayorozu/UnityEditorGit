using System;
using System.Collections;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace Yayorozu.EditorTools.Git
{
	internal enum GitStatusType
	{
		UnTrack,
		Stage,
		UnStage,
	}

	internal class GitTreeViewItem : TreeViewItem, IEqualityComparer
	{
		internal GitStatusType Status { get; }

		private string _tree;
		private string _date;
		private string _author;
		private string _message;
		private string _branch;

		private string StatusText;

		internal string FilePath;
		internal int FileLine;

		internal string this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return _tree;
					case 1:
						return displayName;
					case 2:
						return _date;
					case 3:
						return _author;
					case 4:
						return _message;
					case 5:
						return _branch;
				}

				return string.Empty;
			}
		}

		internal string StatusFilePath =>
			// ファイル名が変わった場合はArrowが表示される
			!displayName.Contains("->") ? displayName : displayName.Substring(displayName.IndexOf("->") + 3);

		internal GitTreeViewItem()
		{
		}

		/// <summary>
		/// git Log
		/// </summary>
		internal GitTreeViewItem(string log)
		{
			// ブランチのnffの場合
			if (!log.Contains("hash={"))
			{
				_tree = log;
				return;
			}

			var b = log.IndexOf("hash={");
			var e = log.IndexOf("}");

			_tree = log.Substring(0, b);

			b += 6;
			displayName = log.Substring(b, e - b);

			log = log.Substring(e + 2);

			b = log.IndexOf("date={") + 6;
			e = log.IndexOf("}");

			_date = log.Substring(b, e - b - 6);

			log = log.Substring(e + 2);

			b = log.IndexOf("author={") + 8;
			e = log.IndexOf("}");

			_author = log.Substring(b, e - b);

			log = log.Substring(e + 2);


			b = log.IndexOf("branch={") + 8;
			e = log.IndexOf("}");

			_branch = log.Substring(b, e - b);

			log = log.Substring(e + 2);

			b = log.IndexOf("commit=") + 7;
			_message = log.Substring(b);
		}

		internal GitTreeViewItem(GitStatusType status)
		{
			Status = status;
		}

		internal GitTreeViewItem(string statusLog, GitStatusType status)
		{
			Status = status;
			StatusText = statusLog.Substring(0, 2);
			displayName = statusLog.Substring(3);
		}

		internal char GetStatusText()
		{
			if (string.IsNullOrEmpty(StatusText))
				return ' ';

			switch (Status)
			{
				case GitStatusType.UnTrack:
					return '?';
				case GitStatusType.Stage:
					return StatusText[0];
				case GitStatusType.UnStage:
					return StatusText[1];
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public new bool Equals(object x, object y)
		{
			return (x as GitTreeViewItem).displayName == (y as GitTreeViewItem).displayName;
		}

		public int GetHashCode(object obj)
		{
			return id;
		}
	}
}
