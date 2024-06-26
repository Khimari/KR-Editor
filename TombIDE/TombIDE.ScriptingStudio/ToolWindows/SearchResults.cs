﻿using DarkUI.Controls;
using DarkUI.Docking;
using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Controls;
using TombIDE.Shared;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.ToolWindows
{
	public partial class SearchResults : DarkToolWindow
	{
		private EditorTabControl _targetTabControl;

		public SearchResults(EditorTabControl targetTabControl)
		{
			InitializeComponent();
			DockText = Strings.Default.SearchResults;

			_targetTabControl = targetTabControl;
		}

		public void UpdateResults(FindReplaceEventArgs e)
		{
			treeView.Nodes.Clear();

			foreach (FindReplaceSource source in e.SourceCollection)
			{
				var sourceNode =
					new DarkTreeNode(string.Format(Strings.Default.MatchSourceNodeText, source.Name, source.Count))
					{
						Tag = source.Name
					};

				foreach (FindReplaceItem item in source)
					sourceNode.Nodes.Add(
						new DarkTreeNode(string.Format(Strings.Default.SingleMatchNodeText, item.LineNumber, item.LineText))
						{
							Tag = item
						});

				sourceNode.Expanded = true;
				treeView.Nodes.Add(sourceNode);
			}
		}

		private void treeView_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (IsRootNode())
				return;

			if (treeView.SelectedNodes.Count == 0)
				return;

			var item = treeView.SelectedNodes[0].Tag as FindReplaceItem;

			if (_targetTabControl != null)
			{
				string sourceFilePath = treeView.SelectedNodes[0].ParentNode.Tag.ToString();
				TabPage tab = _targetTabControl.FindTabPage(sourceFilePath, EditorType.Text);

				if (tab != null)
				{
					_targetTabControl.SelectTab(tab);
					HandleJump(_targetTabControl.CurrentEditor as TextEditorBase, item);
				}
			}
		}

		private void HandleJump(TextEditorBase textEditor, FindReplaceItem item)
		{
			try
			{
				DocumentLine line = textEditor.Document.GetLineByNumber(item.LineNumber);
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);

				MatchCollection matches = Regex.Matches(lineText, item.MatchSegmentText);

				if (item.MatchSegmentIndex > matches.Count)
				{
					textEditor.Select(line.Offset, 0);
					textEditor.ScrollToLine(line.LineNumber);
				}
				else
				{
					Match match = matches[item.MatchSegmentIndex];

					textEditor.Select(line.Offset + match.Index, match.Length);
					textEditor.ScrollToLine(line.LineNumber);
				}
			}
			catch { }
		}

		private bool IsRootNode()
		{
			foreach (DarkTreeNode node in treeView.Nodes)
				if (treeView.SelectedNodes[0] == node)
					return true;

			return false;
		}
	}
}
