using Buddy.BladeAndSoul.Infrastructure;
using Buddy.BotCommon;
using Buddy.Engine;
using log4net;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

using UserControl = System.Windows.Controls.UserControl;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SenseiSkills.Settings;
using SenseiSkills.CombatHandler;

namespace SenseiSkills
{
    public class SenseiSkillsDLL : CombatRoutineBase, IUIButtonProvider
    {


        public string ButtonText
        {
            get { return "Skill Sensei"; }
        }



        public override void OnRegistered()
        {
            base.OnRegistered();

            Log.Info("Sensei Skills Loaded");
            String profileStr = Helper.ReadFromFile();
            //profile = ((JArray)JsonConvert.DeserializeObject(profile)).ToObject<SenseiProfile>();

            profile = ((JObject)JsonConvert.DeserializeObject(profileStr)).ToObject<SenseiProfile>();

            Log.Info("Loaded " + profile.skillList.Count + " Skill Definition");


            _combatMachine = new GenericCombatHandler(profile);
        }



        SenseiProfile profile = new SenseiProfile();

        /// <summary>
        ///     The name of this authored object.
        /// </summary>
        public override string Name { get { return "Sensei Skills"; } }

        /// <summary>
        ///     The author of this object.
        /// </summary>
        public override string Author { get { return "Theonn"; } }

        /// <summary>
        ///     The version of this object implementation.
        /// </summary>
        public override Version Version { get { return new Version(0,2, 0); } }

        public object GameOptions { get; private set; }

        public override async Task Heal()
        {

            if (_combatMachine != null)
                await _combatMachine.Loot();
            return;

        }


        public override async Task Rest()
        {

            if (_combatMachine != null)
                await _combatMachine.Pull();
            return;
        }


        /// <summary>
        ///     Called while the player is in combat.
        /// </summary>
        /// <returns></returns>
        /// 



        private ICombatHandler _combatMachine;




        public override async Task Combat()
        {
            if (_combatMachine != null)
                await _combatMachine.Combat();
            return;
        }



        private static ILog Log = LogManager.GetLogger("[SenseiSkills]");

     




        #region guicontrols

        private Window _gui;
        private UserControl _windowContent;

        private static object ContentLock = new object();


        public void OnButtonClicked(object sender)
        {
            try
            {
                if (_gui == null)
                {
                    _gui = new Window
                    {
                        DataContext = new SuperSettings(),
                        Content = WPFUtils.LoadWindowContent(Path.Combine(AppSettings.Instance.FullRoutinesPath, "SenseiSkills", "SenseiSkills", "GUI")),
                        MinHeight = 400,
                        MinWidth = 200,
                        Title = "SenseiSkills Settings",
                        ResizeMode = ResizeMode.CanResizeWithGrip,

                        //SizeToContent = SizeToContent.WidthAndHeight,
                        SnapsToDevicePixels = true,
                        Topmost = false,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        WindowStyle = WindowStyle.SingleBorderWindow,
                        Owner = null,
                        Width = 550,
                        Height = 650,
                    };
                    _gui.Closed += WindowClosed;

                }
            }
            catch { }

            _gui.Show();
        }

        /// <summary>Call when Config Window is closed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void WindowClosed(object sender, EventArgs e)
        {
            var context = _gui.DataContext as SuperSettings;
            if (context != null)
            {
                Log.Info("Save settings!");
                context.Save();
            }
            else
            {
                Log.InfoFormat("context == null");
            }
            _gui = null;
        }





        #endregion


    }
}
