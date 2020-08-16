using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	[Serializable]
	public class GitCommit : GitModule
	{
		internal override ModuleType Type => ModuleType.Commit;
		internal override KeyCode KeyCode => KeyCode.C;
		internal override string Name => "Commit";

		private string _message;
		private bool _isAmend;

		protected override void OnEnter(object o)
		{
			_message = string.Empty;
			_isAmend = false;
			Lock = true;

			var output = GetStatus();
			var list = GetStage(output, 0).Cast<TreeViewItem>().ToList();
			TreeView.Set(list);
		}

		protected override void OnExit()
		{
		}

		internal override void OnGUI(Rect rect)
		{
			var height = rect.height;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(rect, "Changes to be committed Files", EditorStyles.boldLabel);

			rect.y += EditorGUIUtility.singleLineHeight;
			rect.height = height - EditorGUIUtility.singleLineHeight;

			TreeView.OnGUI(rect);

			using (new EditorGUILayout.HorizontalScope())
			{
				UnityEngine.GUI.SetNextControlName("CommitMessage");
				_message = EditorGUILayout.TextField("Message", _message);
				_isAmend = EditorGUILayout.ToggleLeft("Amend", _isAmend, GUILayout.Width(70));
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(TreeView.GetRows().Count <= 0 || string.IsNullOrEmpty(_message)))
				{
					if (GUILayout.Button("Commit"))
					{
						Command.Exec(_isAmend
							? $"git commit --amend -m \"{_message}\""
							: $"git commit -m \"{_message}\"");
						GUI.Transition(ModuleType.Log);
					}
				}

				if (GUILayout.Button("Cancel"))
				{
					GUI.Transition(ModuleType.Log);
				}
			}
		}
	}
}
