using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Infrastructure;
using Buddy.Common;
using Buddy.Common.Mvvm;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenseiSkills.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SenseiSkills
{
    public static class Helper
    {
        static string path = @"skills.txt";

        public static void writeToFile(String text)
        {

            path = GameManager.LocalPlayer.Name+".json";
            path = Path.Combine(AppSettings.Instance.FullRoutinesPath, "SenseiSkills", path);
           // Log.Info("Writing: " + path + " " + text);
            File.WriteAllText(path, text);

        }

        private static ILog Log = LogManager.GetLogger("SenseiSkills");

        public static String readFromFile()
        {
            path = GameManager.LocalPlayer.Name + ".json"; ;
            path = Path.Combine(AppSettings.Instance.FullRoutinesPath, "SenseiSkills", path);
            string readText = File.ReadAllText(path);

            //Log.Info("Read: " + path + " " + readText);
            return readText;
        }



        /*
        public static void treeToJson(System.Windows.Controls.TreeView skillTree)
        {
            List<SkillInfo>  skillList = new List<SkillInfo>();


            TreeViewItem root = (TreeViewItem)skillTree.Items.GetItemAt(0);



            TreeViewItem gapClose = (TreeViewItem)root.Items.GetItemAt(0);

            foreach (SkillInfoTree itm in gapClose.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }

            TreeViewItem ccBreak = (TreeViewItem)root.Items.GetItemAt(1);

            foreach (SkillInfoTree itm in ccBreak.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }

            TreeViewItem dps = (TreeViewItem)root.Items.GetItemAt(2);


            foreach (SkillInfoTree itm in dps.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }


            TreeViewItem defaulSkills = (TreeViewItem)root.Items.GetItemAt(3); ;



            foreach (SkillInfoTree itm in defaulSkills.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }


            TreeViewItem evadeSkills = (TreeViewItem)root.Items.GetItemAt(4); ;



            foreach (SkillInfoTree itm in evadeSkills.Items)
            {
                SkillInfo skill = addSkillCombo(itm);
                skillList.Add(skill);

            }



            Log.Info("Saving " + skillList.Count + " Skill Definition");

            SenseiProfile profile = new SenseiProfile();
            //profile.minCastTime = int.Parse(minCast.Text);
            //profile.rangedClass = (bool)rangedClass.IsChecked;
            profile.skillList = skillList;


            String profileData = JsonConvert.SerializeObject(profile, Formatting.Indented);
            Helper.writeToFile(profileData);

        }*/

        public static ObservableCollection<ISkillLeaf> jsonToTree(SenseiProfile profile)
        {


            ObservableCollection<ISkillLeaf> Skills = new ObservableCollection<ISkillLeaf>();
            Log.Info("Loaded " + profile.skillList.Count + " Skill Definition");

            List<SkillInfo> skillList = profile.skillList;

            //rangedClass.IsChecked = profile.rangedClass;
            //minCast.Text = profile.minCastTime.ToString();


            SkillLeaf root = new SkillLeaf();
            root.skillName = "Skill List";



           
            SkillLeaf ccBreak = new SkillLeaf();
            ccBreak.skillName = "CCBREAK";

            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.CCBREAK)).ToList())
            {
                SkillLeaf skillTr = addLeaf(skill);
                ccBreak.Skills.Add(skillTr);

            }


            SkillLeaf dps = new SkillLeaf();
            dps.skillName = "DPS";

            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.DPS)).ToList())
            {
                SkillLeaf skillTr = addLeaf(skill);
                dps.Skills.Add(skillTr);

            }

            SkillLeaf defaulSkills = new SkillLeaf();
            defaulSkills.skillName = "DEFAULT";



            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.DEFAULT)).ToList())
            {
                SkillLeaf skillTr = addLeaf(skill);
                defaulSkills.Skills.Add(skillTr);

            }

            SkillLeaf evadeSkills = new SkillLeaf();
            evadeSkills.skillName = "EVADE";



            foreach (SkillInfo skill in skillList.Where(i => i.type.Equals(SkillType.EVADE)).ToList())
            {
                SkillLeaf skillTr = addLeaf(skill);
                evadeSkills.Skills.Add(skillTr);

            }

            root.Skills.Add(ccBreak);
            root.Skills.Add(dps);
            root.Skills.Add(defaulSkills);
            root.Skills.Add(evadeSkills);


            Skills.Add(root);

            return Skills;

        }

        public static SkillInfo addSkillCombo(SkillLeaf skill)
        {
            SkillInfo sk = skill._skillInfo;


            if (skill.Skills.Count == 1)
            {
                SkillInfo cmb = addSkillCombo((SkillLeaf)skill.Skills.First());
                cmb.type = sk.type;
                sk.chainSkill.Add(cmb);
            }

            return sk;


        }

        public static SkillLeaf addLeaf(SkillInfo skill)
        {
            SkillLeaf skillTr = new SkillLeaf();
            skillTr.skillName = skill.skillName;
            skillTr._skillInfo = skill;
            Log.InfoFormat("Adding skill to tree: {0}", skillTr.skillName);


            if (skill.chainSkill != null)
            {
                foreach (SkillInfo sk in skill.chainSkill)
                {
                    SkillLeaf rtn = addLeaf(sk);
                    skillTr.Skills.Add(rtn);
                }

            }
            return skillTr;

        }


        public static void dumpSkillsnActions()
        {

            Log.Info("Actions=====");
            List<Buddy.BladeAndSoul.Game.Action> actions = GameManager.LocalPlayer.CurrentActions.ToList();

            foreach (Buddy.BladeAndSoul.Game.Action action in actions)
            {
                Log.InfoFormat("Action {0}, Duration {1}", action.SkillName, action.Duration);

            }


            Log.Info("Skills=====");
            List<Buddy.BladeAndSoul.Game.Skill> skills = GameManager.LocalPlayer.CurrentSkills.ToList();

            foreach (Buddy.BladeAndSoul.Game.Skill skill in skills)
            {
                Log.InfoFormat("Skill {0}, Key {1}", skill.Name, skill.ShortcutKeyClassic);
            }
            Log.Info("Done=====");
        }

        public static System.Windows.Forms.Keys textToKey(String key)
        {
            if (key.Equals("Z"))
                return System.Windows.Forms.Keys.Z;
            if (key.Equals("X"))
                return System.Windows.Forms.Keys.X;
            if (key.Equals("C"))
                return System.Windows.Forms.Keys.C;
            if (key.Equals("V"))
                return System.Windows.Forms.Keys.V;
            if (key.Equals("F"))
                return System.Windows.Forms.Keys.F;
            if (key.Equals("Tab"))
                return System.Windows.Forms.Keys.Tab;
            if (key.Equals("1"))
                return System.Windows.Forms.Keys.D1;
            if (key.Equals("2"))
                return System.Windows.Forms.Keys.D2;
            if (key.Equals("3"))
                return System.Windows.Forms.Keys.D3;
            if (key.Equals("4"))
                return System.Windows.Forms.Keys.D4;
            if (key.Equals("5"))
                return System.Windows.Forms.Keys.D5;
            if (key.Equals("6"))
                return System.Windows.Forms.Keys.D6;
            if (key.Equals("7"))
                return System.Windows.Forms.Keys.D7;
            if (key.Equals("8"))
                return System.Windows.Forms.Keys.D8;
            if (key.Equals("R"))
                return System.Windows.Forms.Keys.R;
            if (key.Equals("T"))
                return System.Windows.Forms.Keys.T;


            return System.Windows.Forms.Keys.R;

        }
    }








}
