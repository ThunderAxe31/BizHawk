﻿using System;
using System.Windows.Forms;
using BizHawk.Emulation.Cores.Atari.A7800Hawk;

namespace BizHawk.Client.EmuHawk
{
	public partial class A7800ControllerSettings : Form
	{
		private readonly IMainFormForConfig _mainForm;
		private readonly A7800Hawk.A7800SyncSettings _syncSettings;

		public A7800ControllerSettings(
			IMainFormForConfig mainForm,
			A7800Hawk.A7800SyncSettings syncSettings)
		{
			_mainForm = mainForm;
			_syncSettings = syncSettings;
			InitializeComponent();
			Icon = Properties.Resources.GameControllerIcon;
		}

		private void IntvControllerSettings_Load(object sender, EventArgs e)
		{
			foreach (var val in A7800HawkControllerDeck.ControllerCtors.Keys)
			{
				Port1ComboBox.Items.Add(val);
				Port2ComboBox.Items.Add(val);
			}

			Port1ComboBox.SelectedItem = _syncSettings.Port1;
			Port2ComboBox.SelectedItem = _syncSettings.Port2;
		}

		private void OkBtn_Click(object sender, EventArgs e)
		{
			bool changed =
				_syncSettings.Port1 != Port1ComboBox.SelectedItem.ToString()
				|| _syncSettings.Port2 != Port2ComboBox.SelectedItem.ToString();

			if (changed)
			{
				_syncSettings.Port1 = Port1ComboBox.SelectedItem.ToString();
				_syncSettings.Port2 = Port2ComboBox.SelectedItem.ToString();

				_mainForm.PutCoreSyncSettings(_syncSettings, _mainForm.GetSettingsAdapterForLoadedCore<A7800Hawk>());
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void CancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
