using System.Windows.Forms;
using TombIDE.Shared;

namespace TombIDE.ProjectMaster
{
	public partial class SectionProjectSettings : UserControl
	{
		public SectionProjectSettings()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			settings_SpecialFunctions.Initialize(ide);
			settings_ProjectInfo.Initialize(ide);
			settings_GameIcon.Initialize(ide);
			settings_SplashScreen.Initialize(ide);
			settings_StartupImage.Initialize(ide);
			settings_Logo.Initialize(ide);
		}
	}
}
