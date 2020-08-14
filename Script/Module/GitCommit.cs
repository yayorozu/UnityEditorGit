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

		private string _message;

		internal override void OnEnter(object o)
		{
			_message = string.Empty;
			Lock = true;

			var output = GetStatus();
			var list = GetStage(output, 0).Cast<TreeViewItem>().ToList();
			TreeView.Set(list);
		}

		internal override void OnExit()
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

			UnityEngine.GUI.SetNextControlName("CommitMessage");
			_message = EditorGUILayout.TextField("Message", _message);
			using (new EditorGUILayout.HorizontalScope())
			{
				using (new EditorGUI.DisabledScope(TreeView.GetRows().Count <= 0))
				{
					if (GUILayout.Button("Commit"))
					{
						Command.Exec($"git commit -m \"{_message}\"");
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
