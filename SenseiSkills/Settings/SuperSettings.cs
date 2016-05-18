using Buddy.Common;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Newtonsoft.Json.Linq;
using log4net;
using Buddy.Common.Mvvm;
using System.Windows.Input;

namespace SenseiSkills.Settings
{
    public class SuperSettings : JsonSettings, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        private static ILog Log = LogManager.GetLogger("[SenseiSkills]");
        private static SuperSettings _instance;
        internal static SuperSettings Instance { get { return _instance ?? (_instance = new SuperSettings()); } }

        public SuperSettings() : base(GetSettingsFilePath(SettingsPath, "SenseiSkills.json"))
        {



            String jsonText = Helper.readFromFile();

            Log.Info("Loading Skills.txt");

            Profile = ((JObject)JsonConvert.DeserializeObject(jsonText)).ToObject<SenseiProfile>();

            Log.Info("Profile Loaded");

            Log.Info("Got List for " + Profile.skillList.Count);


            Skills = Helper.jsonToTree(Profile);

            
            DumpBackpackCommand = new RelayCommand(
                  parameter =>
                  {
                      
                          Log.Info("");
                        
                 });

        }

        public ICommand DumpBackpackCommand { get; private set; }


        private SenseiProfile profile;


        [JsonIgnore]
        private ObservableCollection<ISkillLeaf> _skills;
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

        public SenseiProfile Profile
        {
            get
            {
                return profile;
            }

            set
            {
                profile = value;
            }
        }


        /// <summary>
        /// Called when property changed.
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