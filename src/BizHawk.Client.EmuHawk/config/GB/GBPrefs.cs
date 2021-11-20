﻿using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Cores.Nintendo.Gameboy;

namespace BizHawk.Client.EmuHawk
{
	public partial class GBPrefs : Form, IDialogParent
	{
		public IDialogController DialogController { get; }

		private GBPrefs(IDialogController dialogController)
		{
			DialogController = dialogController;
			InitializeComponent();
			gbPrefControl1.DialogParent = this;
			Icon = Properties.Resources.GambatteIcon;
		}

		public static void DoGBPrefsDialog(
			Config config,
			IDialogParent dialogParent,
			IGameInfo game,
			IMainFormForConfig mainForm,
			IMovieSession movieSession,
			Gameboy gb)
		{
			var s = gb.GetSettings();
			var ss = gb.GetSyncSettings();

			using var dlg = new GBPrefs(dialogParent.DialogController);
			dlg.gbPrefControl1.PutSettings(config, game, movieSession, s, ss);
			dlg.gbPrefControl1.ColorGameBoy = gb.IsCGBMode() || gb.IsSgb;
			if (dialogParent.ShowDialogAsChild(dlg).IsOk())
			{
				dlg.gbPrefControl1.GetSettings(out s, out ss);
				gb.PutSettings(s);
				if (dlg.gbPrefControl1.SyncSettingsChanged)
				{
					mainForm.GetSettingsAdapterForLoadedCore<Gameboy>().PutCoreSyncSettings(ss);
				}
			}
		}
	}
}
