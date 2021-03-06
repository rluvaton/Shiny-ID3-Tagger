﻿//-----------------------------------------------------------------------
// <copyright file="EnableUI.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Enables or disables UI elements which could potentially interrupt add/search/write methods</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;

	public partial class Form1
	{
		private void EnableUI(bool enable)
		{
			if (enable)
			{
				this.btnAddFiles.Enabled = true;
				this.btnWrite.Enabled = true;
				this.btnSearch.Enabled = true;
				this.menuStrip1.Enabled = true;
			}
			else
			{
				this.btnAddFiles.Enabled = false;
				this.btnWrite.Enabled = false;
				this.btnSearch.Enabled = false;
				this.menuStrip1.Enabled = false;
			}
		}
	}
}
