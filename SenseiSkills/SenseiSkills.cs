using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.DataTables;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.BotCommon;
using Buddy.Coroutines;
using Buddy.Engine;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using TestBuddy;

namespace SenseiSkills
{
    public class SenseiSkills : CombatRoutineBase, IUIButtonProvider
    {

        public string ButtonText
        {
            get { return "Skill Sensei"; }
        }

        public void OnButtonClicked(object sender)
        {
            Log.Info("Launching SkillAdminSS");


            Window window = new Window
            {
                Title = "Skill Master",
                Height = 400,
                Width = 540,
                Content = new SkillAdminSS()

            };

            window.ShowDialog();
        }

        public override void OnRegistered()
        {
            base.OnRegistered();

            Log.Info("Sensei Skills Loaded");
            String profileStr = Helper.readFromFile();
            //profile = ((JArray)JsonConvert.DeserializeObject(profile)).ToObject<SenseiProfile>();

            profile = ((JObject)JsonConvert.DeserializeObject(profileStr)).ToObject<SenseiProfile>();

            Log.Info("Loaded " + profile.skillList.Count + " Skill Definition");
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
        public override Version Version { get { return new Version(0, 1, 0); } }

        public object GameOptions { get; private set; }

        public override async Task Heal()
        {


        }


        public override async Task Rest()
        {
            //GAP CLOSE
            Actor target = null;
            try
            {
                target = GameManager.LocalPlayer.CurrentTarget;

                Log.Info("Init Combat to " + target.Name + " Dis:" + (target.Distance / 50));

                if (target != null && (target.Distance / 50) < GeneralSettings.gapCloseRange)
                {
                    Log.Info("Try to Gap Close for range " + (target.Distance / 50));
                    SkillInfo skill = profile.skillList.Where(i => i.type.Equals(SkillType.GAPCLOSER)).First();
                    if (skill != null)
                    {
                        if (await ExecuteandChainSkill(skill, target))
                        {
                            return;
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                //  Log.Error("NO TARGET FOUND = " + ex.Message);
                return;
            }
        }


        /// <summary>
        ///     Called while the player is in combat.
        /// </summary>
        /// <returns></returns>
        /// 



        public override async Task Combat()
        {

            GameManager.LocalPlayer.Update();



            bool breakCC = false;


            Log.Info("Combat Called===============");


            Actor target = null;
            try
            {
                target = GameManager.LocalPlayer.CurrentTarget;

                target.Update();

                Log.Info("Killing " + target.Name + " Dis:" + (target.Distance / 50));

                try
                {
                    List<Effect> effects = target.Effects.ToList();
                    Log.Info("Target Effect count: " + effects.Count);

                    foreach (Effect effect in effects.ToList())
                    {
                        //do stuff with the debuffs
                        Log.Info("Target Effect" + effect.Name + "==>" + effect.ToString());
                    }
                }
                catch (Exception ex)
                {

                    Log.Error("Effect Issue = " + ex.Message);

                }
            }
            catch (Exception ex)
            {

                //Log.Error("NO TARGET FOUND = "+ex.Message);
                return;
            }



            try
            {
                List<Effect> effects = GameManager.LocalPlayer.Effects.ToList();


                foreach (Effect effect in effects.Where(i => i.BuffType.Equals(EffectBuffType.Debuff)).ToList())
                {
                    //do stuff with the debuffs
                    Log.Info(effect.Dump());
                }

            }
            catch (Exception ex)
            {

                // Log.Error("PROBLEM GETTING EFECTS = " + ex.Message);
            }


            if (profile.rangedClass && (target.Distance / 50) < 5)
            {
                Log.Info("Try to EVADE for range " + (target.Distance / 50));
                Log.Info("EVADE CANT:" + profile.skillList.Where(i => i.type.Equals(SkillType.EVADE)).ToList().Count);
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.EVADE)).ToList())
                {
                    if (skill.skillName.Equals("Backstep"))
                    {
                        if (await ExecuteSS(skill.skillName))
                        {
                            return;
                        }
                    }

                }
            }

            if (target != null && (target.Distance / 50) < GeneralSettings.gapCloseRange)

            {
                Log.Info("Try to Gap Close for range " + (target.Distance / 50));
                Log.Info("GAP CLOSER CANT:" + profile.skillList.Where(i => i.type.Equals(SkillType.GAPCLOSER)).ToList().Count);
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.GAPCLOSER)).ToList())
                {
                    if (await ExecuteandChainSkill(skill, target))
                    {
                        return;
                    }

                }
            }


            //CC BREAK
            if (breakCC)
            {
                Log.Info("Try to Break CC");
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.CCBREAK)).ToList())
                {
                    if (await ExecuteandChainSkill(skill))
                    {
                        return;
                    }

                }

            }

            //REGULAR

            Log.Info("Try to Regular DPS");
            foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DPS)).ToList())
            {
                if (await ExecuteandChainSkill(skill, target))
                {
                    return;
                }

            }

            //DEFAULT
            Log.Info("Try to DEFAULT DPS");
            foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DEFAULT)).ToList())
            {
                if (await ExecuteandChainSkill(skill))
                {
                    return;
                }

            }

        }



        private static ILog Log = LogManager.GetLogger("SenseiSkills");

        async Task<bool> ExecuteandChainSkill(SkillInfo skill, Actor target = null)
        {
            if (await ExecuteSkill(skill, target))
            {

                if (skill.chainSkill.Count > 0)
                {
                    foreach (SkillInfo sk in skill.chainSkill)
                    {
                        await waitGCD(sk.skillName);
                        Log.Info("Chaining " + sk.skillName + " after " + skill.skillName);
                        if (await ExecuteandChainSkill(sk))
                        {
                            return true;
                        }
                    }
                }

                return true;
            }
            return false;
        }



        async Task<bool> ExecuteSkill(SkillInfo skill, Actor target = null)
        {
            return await ExecuteSkill(skill.skillName, target, skill.ignoreSkillError);
        }

        async Task<bool> ExecuteSS(string skillName)
        {
            //dumpSkillsnActions();
            Keys hotkey = Keys.R;
            int castDuration = profile.minCastTime;

            //Log.Info("Cheking skill: " + skillName);

            var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
            if (skill == null)
            {
                Log.Info(skillName + " not available");
                return false;
            }

            var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);

            // Log.Info("Cast Duration " + skill.CastDuration);

            Log.Warn(skillName + " CanCast result: " + castResult + "Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
            if (!(castResult <= SkillUseError.None) && castResult != SkillUseError.LinkFailed)
                return false;




            Log.Info("+++Casting " + skillName + " on key " + "SS" + " with sleep: " + castDuration);
            InputManager.PressKey(Keys.S);
            await Coroutine.Sleep(100);
            InputManager.PressKey(Keys.S);
            await Coroutine.Sleep(castDuration);
            return true;





        }

        async Task<bool> ExecuteSkill(string skillName, Actor target = null, Boolean ignoreState = false)
        {
            //dumpSkillsnActions();
            Keys hotkey = Keys.R;
            int castDuration = profile.minCastTime;
            if (!ignoreState)
            {
                //Log.Info("Cheking skill: " + skillName);

                var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
                if (skill == null)
                {
                    Log.Info(skillName + " not available");
                    return false;
                }

                var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);

                // Log.Info("Cast Duration " + skill.CastDuration);

                Log.Warn(skillName + " CanCast result: " + castResult + "Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
                if (!(castResult <= SkillUseError.None) && castResult != SkillUseError.LinkFailed)
                    return false;

                Log.Info("Verifying Range");

                if (target != null && skill.MaxRange < (target.Distance / 50))
                {
                    Log.Info("Outside of range");
                    return false;
                }
                else
                {
                    Log.Info("Within range");
                }

                if (skill.CastDuration > castDuration)
                {
                    castDuration = skill.CastDuration;
                }


                Log.Info("Getting Key");
                //Log.Info(skill.Name + " [" + skill.Id + "] " + " => " + skill.Alias);
                var shortcutKey = skill.ShortcutKeyClassic;
                //Log.Info("\t" + shortcutKey);
                KeyCommandRecord rec = DataTables.KeyCommand.GetRecord((int)shortcutKey);

                //GameOptions.FindOptionByShortcut(skill.ShortcutKeyClassic, GameOptions.GameOptionType.KeybindData));

                try
                {
                    hotkey = Helper.textToKey(rec.DefaultKeycap.Split(',')[0]);
                }
                catch (Exception ex)
                {
                    Log.Info("Invalid key: " + rec.DefaultKeycap);
                    return false;
                }
                //Log.Info("\t" + rec.Dump());

            }
            else
            {
                Log.Warn("Ignoring Skill state");
            }

            Log.Info("+++Casting " + skillName + " on key " + hotkey + " with sleep: " + castDuration);
            InputManager.PressKey(hotkey);

            /*
            while (GameManager.LocalPlayer.IsCasting)
            {
                Log.InfoFormat("Waiting while casting {0}", GameManager.LocalPlayer.IsCasting);
                await Coroutine.Sleep(100);
            }*/


            await Coroutine.Sleep(castDuration);
            return true;
        }

        public async Task<bool> waitGCD(string skillName)
        {
            var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
            if (skill == null)
            {
                Log.Info(skillName + " not available");
                return false;

            }

            int maxRetry = 0;

            while (true)
            {
                Log.InfoFormat("Waiting for GCD {0}", skillName);

                if (maxRetry >= 10)
                {
                    return false;
                }
                maxRetry++;

                var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);
                Log.InfoFormat("SkillError {0} for skill {1}", castResult, skillName);

                if (castResult <= SkillUseError.None)
                {
                    Log.InfoFormat("Castable{0}", skillName);
                    return true;
                }
                else if (castResult == SkillUseError.StillOnGlobalRecycling)
                {
                    await Coroutine.Sleep(100);
                }
                else { return false; }

            }


        }

    } 
}
