using UnityEditor;
using UnityEngine;

namespace Yayorozu.EditorTools.Git
{
	internal class GitWindow : EditorWindow
	{
		[MenuItem("Tools/Git")]
		private static void ShowWindow()
		{
			var window = GetWindow<GitWindow>();
			window.titleContent = new GUIContent("Git");
			window.Show();
		}

		[SerializeField]
		private GitGUI _gui;

		private void Init()
		{
			if (_gui == null)
				_gui = new GitGUI();
		}

		private void OnGUI()
		{
			Init();
			_gui.OnGUI(GUILayoutUtility.GetRect(0, float.MaxValue, 0, float.MaxValue));
		}
	}
}

