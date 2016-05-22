using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.Coroutines;
using Buddy.Engine;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenseiSkills
{
    static class CombatUtils
    {
        private static ILog Log = LogManager.GetLogger("[SenseiSkills]");

        public static List<Effect> getNpcEffects(Actor target)
        {
            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                Log.Info("Target Effect count: " + targetEffects.Count);

                foreach (Effect effect in targetEffects.ToList())
                {
                    //do stuff with the debuffs
                    Log.InfoFormat("Target {0} Effect==> {1} Stack {2}", target.Name, effect.Name, effect.StackCount);
                    // Log.Info(effect.Dump());
                }

                return targetEffects;
            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return new List<Effect>();

            }

        }

        public static bool hasEffect(Actor target, String effect)
        {
            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => e.Name.Equals(effect)).Count() > 0)
                {
                    return true;
                }
                else { return false; }


            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return false;

            }
        }


        public static Effect getEffect(Actor target, String effect)
        {
            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => e.Name.Equals(effect)).Count() > 0)
                {
                    return targetEffects.Where(e => e.Name.Equals(effect)).First();
                }
                else { return null; }


            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return null;

            }
        }





        public static bool effectInList(Actor target, List<String> effects)
        {





            try
            {
                List<Effect> targetEffects = target.Effects.ToList();

                if (targetEffects.Where(e => { return effects.Contains(e.Name); }).Count() > 0)
                {
                    return true;
                }
                else { return false; }


            }
            catch (Exception ex)
            {

                Log.Error("Effect Issue = " + ex.Message);
                return false;

            }
        }



        public static Actor closestEnemy(bool agro)
        {
            GameEngine.AttachedProcess.Memory.ClearCache();
            using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
            {
                //Log.Info("Getting Actors");
                IEnumerable<Actor> actors = GameManager.Actors.Where(e => e.IsValid && (e.CurrentTargetId == GameManager.LocalPlayer.Id || (GameManager.SummonedMinion.IsValid && e.CurrentTargetId == GameManager.SummonedMinion.Id)) && e.IsHostile);

                actors = actors.OrderBy(e => e.Distance);
                //Log.Info("Sorted Actors by Distance");
                if (actors.Count() > 0)
                {
                    Log.InfoFormat("Got actors {0}", actors.Count());
                    return actors.First();
                }
            }

            return null;

        }

        public static bool keepChannel(String skillName)
        {
          
            try
            {
                Log.InfoFormat("Checking if Skill {0} is being Channeled with player casting", skillName, GameManager.LocalPlayer.IsCasting);

                if (GameManager.LocalPlayer.IsCasting)
                {
                    Log.Info("Hold Channel Skill still in use");
                    return true;
                }

                var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
                var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);
               
               
                Log.Info(skill.Name + " CanCast result: " + castResult + " Range min:" + skill.MinRange + " Max:" + skill.MaxRange);

                if (castResult == SkillUseError.StillUsing || castResult == SkillUseError.IdenticalSkill|| castResult == SkillUseError.StillOnGlobalRecycling)
                {
                    Log.Info("Hold Channel Skill still in use");
                    return true;
                }
                


            }
            catch (Exception ex)
            {
                Log.Info("Problem getting current action");
            }
            return false;

        }

        public static bool keepBlock(Actor target, IEnumerable<SkillInfo> blockList)
        {
            try
            {
                //GameEngine.AttachedProcess.Memory.ClearCache();
                //using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
                //{
                if (target.CurrentTargetId == GameManager.LocalPlayer.Id)
                {
                    //Log.Info("OMG is targetting me ");


                    if (target.IsCasting)
                    {
                        //Log.Info("OMG is casting something...");

                        Buddy.BladeAndSoul.Game.Action myAction = null;


                        foreach (Buddy.BladeAndSoul.Game.Action action in GameManager.LocalPlayer.CurrentActions)
                        {
                            if (blockList.Where(e => e.skillName.Equals(action.SkillName)).Count() > 0)
                            {
                                myAction = action;
                                break;
                            }

                        }
                        //Log.Info("Player Action ++" + myAction.Dump());

                        var targetAction = GameManager.LocalPlayer.CurrentTarget.CurrentActions.First();
                        // Log.Info("Target Action --" + targetAction.Dump());


                        if (targetAction.TimeLeft < myAction.TimeLeft)
                        {
                            Log.InfoFormat("Should hold block as skill can be blocked with skill {0}", myAction.SkillName);
                            return true;
                        }




                    }
                }
            }
            catch (Exception ex)
            {
                Log.Info("Problem getting current action");
            }
            //}
            return false; //we are good for the time being.


        }

        public static bool canBlock(Actor target, String skillName)
        {
            try
            {

                if (target == null && target.IsValid && target.HealthPercent > 0)
                { }
                else
                {
                    Log.Info("Invalid Target for block");
                    return false;
                }


                // GameEngine.AttachedProcess.Memory.ClearCache();
                // using (GameEngine.AttachedProcess.Memory.AcquireFrame(true))
                // {
                if ( target.CurrentTargetId == GameManager.LocalPlayer.Id)
            {


                if (target.IsCasting)
                {


                  

                        var targetAction = GameManager.LocalPlayer.CurrentTarget.CurrentActions.First();
                        //Log.Info("Target Action " + targetAction.Dump());

                        var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
                        int channelTime = skill.Record.ExecDuration1;


                        Log.InfoFormat("Skil Channel Time  {0}>= than skill remaining time {1}", channelTime, targetAction.TimeLeft.Milliseconds);
                        if (channelTime >= targetAction.TimeLeft.Milliseconds)
                        {
                            Log.Info("Try to cast block in time");
                            return true;
                        }
                   



                }



            }
            }
            catch (Exception ex)
            {
                Log.Info("Problem getting current action");
            }
            // }
            return false; //we are good for the time being.


        }

        public static async Task<bool> basicSkillCondition(Skill skill,SkillInfo skillInfo,Actor target)
        {

            var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);
            var castResultSummon = SkillUseError.Unknown;

            if (GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent>0)
             castResultSummon = skill.ActorCanCastResult(GameManager.SummonedMinion);


            // Log.Info("Cast Duration " + skill.CastDuration);

            if (!skillInfo.ignoreSkillError)
            {
                Log.Info(skillInfo.skillName + " CanCast result: " + castResult + " Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
                if ((castResult <= SkillUseError.None) || (castResultSummon <= SkillUseError.None))
                {
                    //Log.Info("Skill Valid " + skillName);
                }
                else
                {
                    if (castResult == SkillUseError.StillOnGlobalRecycling)
                    {
                        await waitGCD(skillInfo.skillName);
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                Log.Warn("Ignoring Skill state");
            }

            //Log.Info("Verifying Range");

            if (target != null && target.IsValid && target.HealthPercent>0 && skill.MaxRange < (target.Distance / 50))
            {
                Log.Info("Outside of range");
                return false;
            }
            else
            {
                //Log.Info("Within range");
            }

            return true;
        }


        public static async Task<bool> validateConditions(SkillInfo skill, Actor target, bool inBlock)
        {

            Log.InfoFormat("Checking Condition for skill {0}", skill.skillName);
            bool rtn = true;

            try
            {


                var skl = GameManager.LocalPlayer.GetSkillByName(skill.skillName);
                if (skl == null)
                {
                    //Log.Info(skillName + " not available");
                    return false;
                }

                if (await CombatUtils.basicSkillCondition(skl, skill, target))
                { }
                else
                {
                    return false;
                }

                    foreach (SkillCondition condition in skill.conditions)
                {

                    if (inBlock && skill.breakBlock == false)
                    {
                        Log.InfoFormat("Player Blocking and skill doest break block");
                        return false;
                    }


                    if (condition.type == ConditionType.NONE)
                    {
                        //rtn=true;
                    }

                    if (condition.type == ConditionType.DONTUSE)
                    {
                        return false;
                    }

                    if (condition.type == ConditionType.DISTLT)
                    {
                        Log.InfoFormat("Dist to TG {0} <= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) <= condition.conditionAmount)
                        { }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DISTGT)
                    {
                        Log.InfoFormat("Dist to GT {0} >= {1}", target.Distance / 50, condition.conditionAmount);
                        if ((target.Distance / 50) >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPGT && !condition.conditionSelf)
                    {
                        Log.InfoFormat("Target HealtPCT to GT {0} >= {1}", target.HealthPercent, condition.conditionAmount);
                        if (target.HealthPercent >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPLT && !condition.conditionSelf)
                    {
                        Log.InfoFormat("Target HealtPCT to LT {0} <= {1}", target.HealthPercent, condition.conditionAmount);
                        if (target.HealthPercent <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPGT && condition.conditionSelf)
                    {
                        Log.InfoFormat("Self HealtPCT to GT {0} >= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
                        if (GameManager.LocalPlayer.HealthPercent >= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.HPLT && condition.conditionSelf)
                    {
                        Log.InfoFormat("Self HealtPCT to LT {0} <= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
                        if (GameManager.LocalPlayer.HealthPercent <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.FOCUSGT)
                    {
                        Log.InfoFormat("Target Focus to LT {0} <= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
                        if (GameManager.LocalPlayer.Focus <= condition.conditionAmount)
                        { }
                        else return false;

                    }


                    if (condition.type == ConditionType.FOCUSLT)
                    {
                        Log.InfoFormat("Self Focus to GT {0} >= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
                        if (GameManager.LocalPlayer.Focus >= condition.conditionAmount)
                        { }
                        else return false;

                    }

                    if (condition.type == ConditionType.STACK && !condition.conditionSelf)
                    {
                        Effect ef = getEffect(target, condition.conditionName);
                        if (ef != null)
                        {
                            Log.InfoFormat("Effect {0} with stacks (tgt) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
                            if (ef.StackCount >= condition.stackCount)
                            {
                                // return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }


                    if (condition.type == ConditionType.STACK && condition.conditionSelf)
                    {
                        Effect ef = getEffect(GameManager.LocalPlayer, condition.conditionName);
                        if (ef != null)
                        {
                            Log.InfoFormat("Effect {0} with stacks (self) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
                            if (ef.StackCount >= condition.stackCount)
                            {
                                //return true;
                            }
                            else
                            {
                                return false;
                            }
                        }

                    }


                    if (condition.type == ConditionType.STANCE)
                    {
                        Log.InfoFormat("Player Stance {0} required Stance {1}", GameManager.LocalPlayer.Stance.ToString(), condition.conditionName);
                        if (condition.conditionName.Equals(GameManager.LocalPlayer.Stance.ToString()))
                        {
                            //return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    if (condition.type == ConditionType.EFFECT && !condition.conditionSelf)
                    {
                        Effect ef = getEffect(target, condition.conditionName);
                        if (ef != null)
                        {
                            Log.InfoFormat("Effect {0}  (tgt) with stacks {1} ", ef.Name, ef.StackCount);

                            //return true;

                        }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.EFFECT && condition.conditionSelf)
                    {
                        Effect ef = getEffect(GameManager.LocalPlayer, condition.conditionName);
                        if (ef != null)
                        {
                            Log.InfoFormat("Effect {0}  (self) with stacks {1} ", ef.Name, ef.StackCount);

                            //return true;

                        }
                        else
                            return false;

                    }

                    if (condition.type == ConditionType.DEADSUMMON)
                    {
                        //Log.InfoFormat("Summon (dead) is valid= {0}", GameManager.SummonedMinion != null);
                        if (GameManager.SummonedMinion == null || (GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent == 0))
                        {
                            //return true;
                        }
                        else
                        {
                            return false;
                        }

                    }

                    if (condition.type == ConditionType.SUMMONALIVE)
                    {
                        //                  Log.InfoFormat("Summon is valid= {0}", GameManager.SummonedMinion!=null);
                        if (GameManager.SummonedMinion != null && GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent > 0)
                        {
                            Log.InfoFormat("Summon Info Name: {0} hp: {1}", GameManager.SummonedMinion.Name, GameManager.SummonedMinion.HealthPercent);

                        }
                        else
                            return false;

                        //return true;


                    }


                }
            }
            catch (Exception ex)
            {
                Log.Error("Invalid Condition" + ex.Message);
                rtn = false;
            }

            return rtn;




        }


        public static void TurnToActor(Actor target)
        {

            if (target != null && target.IsValid)
            {
                target.Face();
            }



        }

        public static async Task<bool> doLoot()
        {

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

        public static async Task<bool> waitGCD(string skillName)
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
                    Log.InfoFormat("Castable {0}", skillName);
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

        /*
            async Task<bool> ExecuteandChainSkill(SkillInfo skill, Actor target = null)
            {
                if (CombatUtils.validateConditions(skill, target, inBlock))
                {
                    if (await ExecuteAndCheck(skill, target))
                    {

                        if (skill.chainSkill.Count > 0)
                        {
                            foreach (SkillInfo sk in skill.chainSkill)
                            {
                                await waitGCD(sk.skillName);
                                Log.Info("Chaining " + sk.skillName + " after " + skill.skillName);
                                if (await ExecuteAndCheck(sk, target))
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

                Log.Info(skillName + " CanCast result: " + castResult + "Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
                if (!(castResult <= SkillUseError.None) && castResult != SkillUseError.LinkFailed)
                    return false;



                Log.Info("+++Casting " + skillName + " on key " + "SS" + " with sleep: " + castDuration);
                InputManager.PressKey(Keys.S);
                await Coroutine.Sleep(100);
                InputManager.PressKey(Keys.S);
                await Coroutine.Sleep(castDuration);
                return true;





            }


            async Task<bool> ExecuteAndCheck(SkillInfo skillInfo, Actor target)
            {

                if (await ExecuteSkill(skillInfo, target))
                {


                    try
                    {
                        GameManager.Memory.ClearCache();
                        GameManager.LocalPlayer.Update();
                        var myAction = GameManager.LocalPlayer.CurrentActions.First();

                        if (myAction.SkillName.Equals(skillInfo.skillName))
                        {

                            Log.InfoFormat("Skill {0} fired", skillInfo.skillName);
                            if (skillInfo.channeledSkill)
                            {
                                Log.InfoFormat("Action {0} time {1}  remaing {2}", myAction.SkillName, myAction.Duration, myAction.TimeLeft);
                                int sleepTime = myAction.TimeLeft.Milliseconds;
                                Log.Info("Action remaing time : " + sleepTime);

                                await Coroutine.Sleep(sleepTime);
                            }
                            return true;
                        }
                        else
                        {
                            Log.InfoFormat("Skill {0} dint fired", skillInfo.skillName);
                            return false;
                        }



                    }
                    catch (Exception ex)
                    {
                        Log.Info("Problem getting current action: " + ex.Message);
                        return false;
                    }

                }
                else
                {
                    return false;
                }



            }





           

            
            */

    
