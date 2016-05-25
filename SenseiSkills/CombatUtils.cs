using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using Buddy.BladeAndSoul.Game;
using Buddy.BladeAndSoul.Game.Objects;
using Buddy.Coroutines;
using Buddy.Engine;

using log4net;

namespace SenseiSkills
{
	static class CombatUtils
	{
		private static readonly ILog Log = LogManager.GetLogger("[SenseiSkills][CombatUtils]");

		public static List<Effect> GetNpcEffects(Actor target)
		{
			try
			{
				List<Effect> targetEffects = target.Effects.ToList();

				Log.Info("Target Effect count: " + targetEffects.Count);

				foreach (Effect effect in targetEffects)
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

		public static bool HasEffect(Actor target, string effect)
		{
			try
			{
				if (target.Effects.Any(e => e.Name.Equals(effect, StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("Effect Issue = " + ex.Message);
				return false;
			}
		}


		public static Effect GetEffect(Actor target, string effect)
		{
			try
			{
				return target.Effects.FirstOrDefault(e => e.Name.Equals(effect, StringComparison.OrdinalIgnoreCase));
			}
			catch (Exception ex)
			{
				Log.Error("Effect Issue = " + ex.Message);
				return null;
			}
		}

		public static bool EffectInList(Actor target, List<string> effects)
		{
			try
			{
				if (target.Effects.Any(e => effects.Contains(e.Name)))
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("Effect Issue = " + ex.Message);
				return false;
			}
		}


		public static Actor ClosestEnemy(bool agro)
		{
			//Log.Info("Getting Actors");
			var actors =
				GameManager.Actors.Where(
					e =>
						e.IsValid && 
						(e.CurrentTargetId == GameManager.LocalPlayer.Id || (GameManager.SummonedMinion.IsValid && e.CurrentTargetId == GameManager.SummonedMinion.Id)) 
						&& e.IsHostile).OrderBy(e => e.Distance).ToList();
			//Log.Info("Sorted Actors by Distance");
			if (actors.Count != 0)
			{
				Log.InfoFormat("Got actors {0}", actors.Count);
				return actors[0];
			}

			return null;
		}

		public static bool KeepChannel(string skillName)
		{
			try
			{
				Log.InfoFormat("Checking if Skill {0} is being Channeled with player casting", skillName);

				if (GameManager.LocalPlayer.IsCasting)
				{
					Log.Info("Hold Channel Skill still in use");
					return true;
				}

				var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
				var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);

				Log.Info(skill.Name + " CanCast result: " + castResult + " Range min:" + skill.MinRange + " Max:" + skill.MaxRange);

				switch (castResult)
				{
					case SkillUseError.StillUsing:
					case SkillUseError.IdenticalSkill:
					case SkillUseError.StillOnGlobalRecycling:
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

		public static bool KeepBlock(Actor target, IEnumerable<SkillInfo> blockList)
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
							if (blockList.Any(e => e.skillName.Equals(action.SkillName)))
							{
								myAction = action;
								break;
							}
						}
						if (myAction == null)
							return false;
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

		public static bool CanBlock(Actor target, string skillName)
		{
			try
			{
				if (target == null || !target.IsValid || !(target.HealthPercent > 0))
				{
					Log.Info("Invalid Target for block");
					return false;
				}

				
				if (target.CurrentTargetId == GameManager.LocalPlayer.Id)
				{
					if (target.IsCasting)
					{
						var targetAction = GameManager.LocalPlayer.CurrentTarget.CurrentActions.First();
						//Log.Info("Target Action " + targetAction.Dump());

						var skill = GameManager.LocalPlayer.GetSkillByName(skillName);
						int channelTime = skill.Record.PassiveSkill.ExecDuration1;

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
			return false; //we are good for the time being.
		}


		public static bool ValidateTarget(Actor actor)
		{
			if (actor != null && actor.IsValid && actor.HealthPercent > 0)
			{
				Log.InfoFormat("Actor {0} is valid and alive", actor.Name);
				return true;
			}
			return false;
		}

		public static async Task<bool> BasicSkillCondition(Skill skill, SkillInfo skillInfo, Actor target)
		{
			var castResult = skill.ActorCanCastResult(GameManager.LocalPlayer);
			var castResultSummon = SkillUseError.Unknown;

			if (GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent > 0)
				castResultSummon = skill.ActorCanCastResult(GameManager.SummonedMinion);


			// Log.Info("Cast Duration " + skill.CastDuration);

			if (!skillInfo.ignoreSkillError)
			{
				Log.Info(skillInfo.skillName + " CanCast result: " + castResult + " Range min:" + skill.MinRange + " Max:" + skill.MaxRange);
				if ((castResult > SkillUseError.None) && (castResultSummon > SkillUseError.None))
				{
					if (castResult != SkillUseError.StillOnGlobalRecycling)
						return false;
					await WaitGcd(skillInfo.skillName);
					return true;
				}
				else
				{
					//Log.Info("Skill Valid " + skillName);
				}
			}
			else
			{
				Log.Warn("Ignoring Skill state");
			}

			//Log.Info("Verifying Range");

			if (target != null && target.IsValid && target.HealthPercent > 0 && skill.MaxRange < (target.Distance / 50))
			{
				Log.Info("Outside of range");
				return false;
			}

			return true;
		}


		public static async Task<bool> ValidateConditions(SkillInfo skill, Actor target, bool inBlock)
		{
			Log.InfoFormat("Checking Condition for skill {0}", skill.skillName);
			bool rtn = true;

			try
			{
				if (!ValidateTarget(GameManager.LocalPlayer))
				{
					return false;
				}

				var skl = GameManager.LocalPlayer.GetSkillByName(skill.skillName);
				if (skl == null)
				{
					//Log.Info(skillName + " not available");
					return false;
				}

				if (!await BasicSkillCondition(skl, skill, target))
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

					switch (condition.type)
					{
						case ConditionType.NONE:
							//rtn=true;
							break;
						case ConditionType.DONTUSE:
							return false;
						case ConditionType.DISTLT:
							Log.InfoFormat("Dist to TG {0} <= {1}", target.Distance / 50, condition.conditionAmount);
							if ((target.Distance / 50) <= condition.conditionAmount)
							{
							}
							else
								return false;
							break;
						case ConditionType.DISTGT:
							Log.InfoFormat("Dist to GT {0} >= {1}", target.Distance / 50, condition.conditionAmount);
							if ((target.Distance / 50) < condition.conditionAmount)
							{
								return false;
							}
							break;
						case ConditionType.HPGT:
							if (!condition.conditionSelf)
							{
								Log.InfoFormat("Target HealtPCT to GT {0} >= {1}", target.HealthPercent, condition.conditionAmount);
								if (target.HealthPercent < condition.conditionAmount)
								{
									return false;
								}
							}
							else
							{
								Log.InfoFormat("Self HealtPCT to GT {0} >= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
								if (GameManager.LocalPlayer.HealthPercent < condition.conditionAmount)
								{
									return false;
								}
							}
							break;
						case ConditionType.HPLT:
							if (!condition.conditionSelf)
							{
								Log.InfoFormat("Target HealtPCT to LT {0} <= {1}", target.HealthPercent, condition.conditionAmount);
								if (target.HealthPercent > condition.conditionAmount)
								{
									return false;
								}
							}
							else
							{
								Log.InfoFormat("Self HealtPCT to LT {0} <= {1}", GameManager.LocalPlayer.HealthPercent, condition.conditionAmount);
								if (GameManager.LocalPlayer.HealthPercent > condition.conditionAmount)
								{
									return false;
								}
							}
							break;
						case ConditionType.FOCUSGT:
							Log.InfoFormat("Target Focus to LT {0} <= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
							if (GameManager.LocalPlayer.Focus > condition.conditionAmount)
							{
								return false;
							}
							break;
						case ConditionType.FOCUSLT:
							Log.InfoFormat("Self Focus to GT {0} >= {1}", GameManager.LocalPlayer.Focus, condition.conditionAmount);
							if (GameManager.LocalPlayer.Focus < condition.conditionAmount)
							{
								return false;
							}
							break;
						case ConditionType.STACK:
							if (!condition.conditionSelf)
							{
								Effect ef = GetEffect(target, condition.conditionName);
								if (ef != null)
								{
									Log.InfoFormat("Effect {0} with stacks (tgt) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
									if (ef.StackCount < condition.stackCount)
									{
										return false;
									}
								}
							}
							else
							{
								Effect ef = GetEffect(GameManager.LocalPlayer, condition.conditionName);
								if (ef != null)
								{
									Log.InfoFormat("Effect {0} with stacks (self) {1} >= {2}", ef.Name, ef.StackCount, condition.stackCount);
									if (ef.StackCount < condition.stackCount)
									{
										return false;
									}
								}
							}
							break;
						case ConditionType.STANCE:
							Log.InfoFormat("Player Stance {0} required Stance {1}", GameManager.LocalPlayer.Stance, condition.conditionName);
							if (!condition.conditionName.Equals(GameManager.LocalPlayer.Stance.ToString()))
							{
								return false;
							}
							break;
						case ConditionType.EFFECT:
							if (!condition.conditionSelf)
							{
								Effect ef = GetEffect(target, condition.conditionName);
								if (ef == null)
									return false;
								Log.InfoFormat("Effect {0}  (tgt) with stacks {1} ", ef.Name, ef.StackCount);
							}
							else
							{
								Effect ef = GetEffect(GameManager.LocalPlayer, condition.conditionName);
								if (ef == null)
									return false;
								Log.InfoFormat("Effect {0}  (self) with stacks {1} ", ef.Name, ef.StackCount);
							}
							break;
						case ConditionType.DEADSUMMON:
							//Log.InfoFormat("Summon (dead) is valid= {0}", GameManager.SummonedMinion != null);
							if (GameManager.SummonedMinion == null || (GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent == 0))
							{
								//return true;
							}
							else
							{
								return false;
							}
							break;
						case ConditionType.SUMMONALIVE:
							//                  Log.InfoFormat("Summon is valid= {0}", GameManager.SummonedMinion!=null);
							if (GameManager.SummonedMinion != null && GameManager.SummonedMinion.IsValid && GameManager.SummonedMinion.HealthPercent > 0)
							{
								Log.InfoFormat("Summon Info Name: {0} hp: {1}", GameManager.SummonedMinion.Name, GameManager.SummonedMinion.HealthPercent);
							}
							else
								return false;
							break;

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

		public static async Task<bool> DoLoot()
		{
			List<SkillsContext.SkillContext> varlist =
				GameManager.SkillsContext.AllAvailable.Where(
					obj =>
						obj.Type.Equals(SkillsContext.SkillBarType.NpcManipulate) || obj.Type.Equals(SkillsContext.SkillBarType.FieldItemOpen15) ||
						obj.Type.Equals(SkillsContext.SkillBarType.PickUpFieldItemAuto) || obj.Type.Equals(SkillsContext.SkillBarType.PickUpFieldItemAutoAll)).ToList();

			//Log.Info(context.Type + " -> " + context.TargetId.ToString("X") + " -> " + (context.Skill.IsValid ? context.Skill.Name : context.Actor?.Name)+"  -> "+context.Skill.ShortcutKey.Dump());
			if (varlist.Count > 0)
			{
				Log.Info("Trying to autoloot");
				InputManager.PressKey(Keys.F);
				await Coroutine.Sleep(100);
			}

			return true;
		}

		public static async Task<bool> WaitGcd(string skillName)
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
				if (castResult == SkillUseError.StillOnGlobalRecycling)
				{
					await Coroutine.Sleep(100);
				}
				else
				{
					return false;
				}
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