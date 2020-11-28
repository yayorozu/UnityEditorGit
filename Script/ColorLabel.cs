using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTools.Git
{
	internal static class ColorLabel
	{
		private static Dictionary<Color, GUIStyle> _dic = new Dictionary<Color, GUIStyle>();

		internal static GUIStyle Get(Color color)
		{
			if (!_dic.ContainsKey(color))
			{
				var style = new GUIStyle(EditorStyles.label);
				style.normal.textColor = color;
				_dic.Add(color, style);
			}

			return _dic[color];
		}
	}
}
