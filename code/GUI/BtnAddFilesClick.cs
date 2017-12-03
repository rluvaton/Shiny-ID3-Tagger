//-----------------------------------------------------------------------
// <copyright file="BtnAddFilesClick.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Opens the file selection window when pressing "Add files" button</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using System.Windows.Forms;

	public partial class Form1 : Form
	{
		private async void BtnAddFilesClick(object sender, EventArgs e)
		{
			// Add new files
			bool newFiles = await this.AddFiles(null);
			
			// If the setting allows it and new files were added (dialog not canceled or files were already added), continue straight with searching
			if (User.Settings["AutoSearch"] && newFiles)
			{
				this.StartSearching();
			}
		}
	}
}