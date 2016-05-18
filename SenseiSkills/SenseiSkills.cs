
using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.BladeAndSoul.Infrastructure;
using Buddy.BotCommon;
using Buddy.Engine;
using log4net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using UserControl = System.Windows.Controls.UserControl;
using Application = System.Windows.Application;
using Buddy.BladeAndSoul;
using Buddy.BladeAndSoul.ViewModels;
using Buddy.Coroutines;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Buddy.BladeAndSoul.Game.DataTables;
using SenseiSkills.Settings;

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
        public override Version Version { get { return new Version(0, 1, 1); } }

        public object GameOptions { get; private set; }

        public override async Task Heal()
        {

            //Log.InfoFormat("Player HP {0} MAX HP {1} PCTHP {2}", GameManager.LocalPlayer.Health, GameManager.LocalPlayer.MaxHealth, GameManager.LocalPlayer.HealthPercent);
            //Log.Info("Heal Called===============");
            await doLoot();
            //await doDumpling();

        }


        public override async Task Rest()
        {



            //Log.Info("Rest Called===============");
            if (profile.attackTarget)
            {
                //PULL
                Actor target = null;
                try
                {
                    target = GameManager.LocalPlayer.CurrentTarget;

                    if (target == null)
                    {
                        target = CombatUtils.closestEnemy(true);
                         TurnToActor(target);
                    }


                    Log.Info("Init Combat to " + target.Name + " Dis:" + (target.Distance / 50));

                    if (target != null && (target.Distance / 50) < profile.gapCloseRange)
                    {
                        Log.Info("Try to PULL Close for range " + (target.Distance / 50));
                        foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.PULL)).ToList())
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

                    //Log.Error("NO TARGET FOUND (pull)= " + ex.Message);
                    return;
                }
            }
        }


        /// <summary>
        ///     Called while the player is in combat.
        /// </summary>
        /// <returns></returns>
        /// 



        


        List<String> cclist = new List<String> { "fuckedup" };


        public override async Task Combat()
        {
            Log.Info("Combat Called===============");

            GameManager.LocalPlayer.Update();

            Log.InfoFormat("Current Stance {0}", GameManager.LocalPlayer.Stance);

            //await doPot();

            bool breakCC = false;
            List<Effect> targetEffects = new List<Effect>();
            List<Effect> selfEffects = new List<Effect>();



            Actor target = null;
            try
            {
                target = GameManager.LocalPlayer.CurrentTarget;



                Log.Info("Killing " + target.Name + " Dis:" + (target.Distance / 50));

                try
                {
                    //float facing = MovementManager.CalculateFacing(GameManager.LocalPlayer.Position, target.Position);

                    //Log.Info("Facing=> "+facing);

                    Log.InfoFormat("Creature Type: " + target.CreatureType);
                     TurnToActor(target);


                }
                catch (Exception ex)
                {
                    Log.Error("Problem moving = " + ex.Message);
                    Log.Error(ex);
                    return;
                }


            }
            catch (Exception ex)
            {

                //Log.Error("NO TARGET FOUND (combat) = "+ex.Message);
                return;
            }



            try
            {
                selfEffects = GameManager.LocalPlayer.Effects.ToList();

                Log.Info("Self Effect count: " + selfEffects.Count);


                breakCC = CombatUtils.effectInList(GameManager.LocalPlayer, cclist);

                /*
                foreach (Effect effect in selfEffects.ToList())
                {
                    Log.InfoFormat("Self Effect==> {0} Stack {1}",  effect.Name, effect.StackCount);


                   // Log.Info(effect.Dump());
                }*/

            }
            catch (Exception ex)
            {

                Log.Error("PROBLEM GETTING EFECTS = " + ex.Message);
            }



            try
            {
                Log.Info("Try to EVADE");
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.EVADE)).ToList())
                {

                    if (skill.skillName.Equals("Backstep") && CombatUtils.validateConditions(skill, target))
                    {
                        if (await ExecuteSS(skill.skillName))
                        {
                            return;
                        }

                    }
                    else
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
                Log.Error("Problem Performing EVADE");
            }


           

            try
            {

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
            }
            catch (Exception ex)
            {
                Log.Error("Problem Performing BreakCC");
            }

            //REGULAR


            try
            {
                Log.Info("Try to Regular DPS");
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DPS)).ToList())
                {
                    
                    if (await ExecuteandChainSkill(skill, target))
                    {
                        return;
                    }

                }


            }
            catch (Exception ex)
            {
                Log.Error("Problem Performing Regular DPS");
            }

            //DEFAULT

            try
            {
                Log.Info("Try to DEFAULT DPS");
                foreach (SkillInfo skill in profile.skillList.Where(i => i.type.Equals(SkillType.DEFAULT)).ToList())
                {
                    if (await ExecuteandChainSkill(skill))
                    {
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Error("Problem Performing Defautl");
            }

        }



        private static ILog Log = LogManager.GetLogger("[SenseiSkills]");

        #region skillcast


        void TurnToActor(Actor target)
        {

           
            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                if (target != null && target.IsValid)
                {
                    Log.InfoFormat("Turning to target {0}", target.Name);
                     MovementManager.Face(target);
                }
            }

           
        }

        async Task<bool> ExecuteandChainSkill(SkillInfo skill, Actor target = null)
        {
            if(CombatUtils.validateConditions(skill,target))
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
            //Keys hotkey = Keys.R;
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
            //Keys hotkey = Keys.R;
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

            if (!ignoreState)
            {
                Log.Warn(skillName + " CanCast result: " + castResult + " Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
                if (!(castResult <= SkillUseError.None))
                    return false;
            }
            else
            {
                Log.Warn("Ignoring Skill state");
            }

            //Log.Info("Verifying Range");

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



            skill.Cast();

            Log.Info("+++Casting " + skillName + " with sleep: " + castDuration);
            //InputManager.PressKey(hotkey);



            /*
            while (GameManager.LocalPlayer.IsCasting)
            {
                Log.InfoFormat("Waiting while casting {0}", GameManager.LocalPlayer.IsCasting);
                await Coroutine.Sleep(100);
            }*/


            await Coroutine.Sleep(castDuration);
            return true;
        }


        public async Task<bool> doLoot()
        {


            //
            /*
            foreach (var context in GameManager.SkillsContext.AllAvailable)
            {
                Log.Info(context.Type + " -> " + context.TargetId.ToString("X") + " -> " + (context.Skill.IsValid ? context.Skill.Name : context.Actor?.Name));
            }*/

            List<SkillsContext.SkillContext> varlist = GameManager.SkillsContext.AllAvailable.Where(obj => obj.Type.Equals(SkillsContext.SkillBarType.NpcManipulate) || obj.Type.Equals(SkillsContext.SkillBarType.FieldItemOpen15) || obj.Type.Equals(SkillsContext.SkillBarType.PickUpFieldItemAuto) || obj.Type.Equals(SkillsContext.SkillBarType.PickUpFieldItemAutoAll)).ToList();

            //Log.Info(context.Type + " -> " + context.TargetId.ToString("X") + " -> " + (context.Skill.IsValid ? context.Skill.Name : context.Actor?.Name)+"  -> "+context.Skill.ShortcutKey.Dump());
            if (varlist.Count > 0)
            {
                Log.Info("Trying to autoloot");
                InputManager.PressKey(Keys.F);
                await Coroutine.Sleep(100);

            }






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

        #endregion




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
