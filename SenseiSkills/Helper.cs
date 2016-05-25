using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Infrastructure;

using log4net;

using SenseiSkills.Settings;

namespace SenseiSkills
{
	public static class Helper
	{
		static string path = @"skills.txt";

		private static readonly ILog Log = LogManager.GetLogger("SenseiSkills");

		public static void WriteToFile(string text)
		{
			path = GameManager.LocalPlayer.Name + ".json";
			path = Path.Combine(AppSettings.Instance.FullRoutinesPath, "SenseiSkills", path);
			// Log.Info("Writing: " + path + " " + text);
			File.WriteAllText(path, text);
		}

		public static string ReadFromFile()
		{
			path = GameManager.LocalPlayer.Name + ".json";
			path = Path.Combine(AppSettings.Instance.FullRoutinesPath, "SenseiSkills", path);
			string readText = File.ReadAllText(path);

			//Log.Info("Read: " + path + " " + readText);
			return readText;
		}

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
				SkillInfo cmb = addSkillCombo((SkillLeaf) skill.Skills.First());
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
			List<Action> actions = GameManager.LocalPlayer.CurrentActions.ToList();

			foreach (Action action in actions)
			{
				Log.InfoFormat("Action {0}, Duration {1}", action.SkillName, action.Duration);
			}

			Log.Info("Skills=====");
			List<Skill> skills = GameManager.LocalPlayer.CurrentSkills.ToList();

			foreach (Skill skill in skills)
			{
				Log.InfoFormat("Skill {0}, Key {1}", skill.Name, skill.ShortcutKeyClassic);
			}
			Log.Info("Done=====");
		}
	}
}