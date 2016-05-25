using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

using Buddy.Common;
using Buddy.Common.Mvvm;

using log4net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SenseiSkills.Settings
{
	public class SuperSettings : JsonSettings, INotifyPropertyChanged
	{
		private static readonly ILog Log = LogManager.GetLogger("[SenseiSkills]");
		private static SuperSettings _instance;


		[JsonIgnore]
		private ObservableCollection<ISkillLeaf> _skills;


		public SuperSettings() : base(GetSettingsFilePath(SettingsPath, "SenseiSkills.json"))
		{
			string jsonText = Helper.ReadFromFile();

			Log.Info("Loading Skills.txt");

			Profile = ((JObject) JsonConvert.DeserializeObject(jsonText)).ToObject<SenseiProfile>();

			Log.Info("Profile Loaded");

			Log.Info("Got List for " + Profile.skillList.Count);

			Skills = Helper.jsonToTree(Profile);

			DumpBackpackCommand = new RelayCommand(
				parameter => { Log.Info(""); });
		}

		internal static SuperSettings Instance { get { return _instance ?? (_instance = new SuperSettings()); } }

		public ICommand DumpBackpackCommand { get; private set; }

		[JsonIgnore]
		public ObservableCollection<ISkillLeaf> Skills
		{
			get
			{
				if (_skills == null)
				{
					_skills = new ObservableCollection<ISkillLeaf>();
				}
				return _skills;
			}
			set
			{
				_skills = value;
				OnPropertyChanged("Skills");
			}
		}

		public SenseiProfile Profile { get; set; }

		#region INotifyPropertyChanged Members

		/// <summary>
		///     Occurs when property changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		/// <summary>
		///     Called when property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		public void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}