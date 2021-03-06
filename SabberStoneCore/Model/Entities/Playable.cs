﻿using System.Collections.Generic;
using System.Linq;
using SabberStoneCore.Enums;
using SabberStoneCore.Enchants;

namespace SabberStoneCore.Model.Entities
{
	public interface IPlayable : ITargeting
	{
		bool IsPlayable { get; }
		bool IsPlayableByPlayer { get; }
		bool IsPlayableByCardReq { get; }
		bool IsIgnoreDamage { get; set; }
		bool Combo { get; }
		int Cost { get; set; }
		int ZonePosition { get; set; }
		int NumTurnsInPlay { get; set; }
		IPlayable Destroy();
		bool ToBeDestroyed { get; }
		bool TurnStart { get; set; }
		void ApplyEnchantments(EnchantmentActivation activation, Zone zoneType, IPlayable target = null);
		void SetOrderOfPlay(string type);
		bool IsSummoned { get; set; }
		bool JustPlayed { get; set; }
		int Overload { get; set; }
		int CardTarget { get; set; }

		IPlayable[] ChooseOnePlayables { get; }

		List<Enchantment> Enchantments { get; set; }
	}

	public abstract partial class Playable<T> : Targeting, IPlayable where T : Entity
	{
		protected Playable(Controller controller, Card card, Dictionary<GameTag, int> tags)
			: base(controller, card, tags)
		{

			if (Card.Enchantments != null)
			{
				Enchantments.AddRange(Card.Enchantments);
			}
		}

		public IPlayable[] ChooseOnePlayables { get; } = new IPlayable[2];

		public List<Enchantment> Enchantments { get; set; } = new List<Enchantment>();

		public virtual void ApplyEnchantments(EnchantmentActivation activation, Zone zoneType, IPlayable target = null)
		{
			var removeEnchantments = new List<Enchantment>();

			Enchantments.ForEach(p =>
			{
				if (p.Activation == activation && (Zone == null || Zone.Type == zoneType))
				{
					p.Activate(Controller, this, target);
					if (p.RemoveAfterActivation)
					{
						removeEnchantments.Add(p);
					}
				}
			});

			removeEnchantments.ForEach(p => Enchantments.Remove(p));
		}

		public void SetOrderOfPlay(string type)
		{
			if (type.Equals("PLAY")
			 || type.Equals("SECRET_OR_QUEST")
			 || type.Equals("WEAPON"))
			{
				OrderOfPlay = Game.NextOop;
			}
		}

		public IPlayable Destroy()
		{
			ToBeDestroyed = true;
			Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"{this} just got set to be destroyed.");
			return this;
		}

		public virtual bool IsPlayable => IsPlayableByPlayer && IsPlayableByCardReq;

		public virtual bool IsPlayableByPlayer
		{
			get
			{
				// check if player is on turn
				if (Controller != Game.CurrentPlayer)
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable",
						$"{this} isn't playable, because player not on turn.");
					return false;
				}

				// check if entity is in hand to be played
				if (Zone != Controller.HandZone && !(this is HeroPower))
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable",
						$"{this} isn't playable, because card not in hand.");
					return false;
				}

				// check if player has enough mana to play card
				if (Controller.RemainingMana < Cost)
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable",
						$"{this} isn't playable, because not enough mana to pay cost.");
					return false;
				}

				// check if we got a slot on board for minions
				if (Controller.BoardZone.IsFull && this is Minion)
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable",
						$"{this} isn't playable, because not enough place on board.");
					return false;
				}

				// check if we can play this secret
				var spell = this as Spell;
				if (spell != null && spell.IsSecret && Controller.SecretZone.GetAll.Exists(p => p.Card.Id == spell.Card.Id))
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable",
						$"{this} isn't playable, because secret already active on controller.");
					return false;
				}

				return true;
			}
		}

		public virtual bool IsPlayableByCardReq
		{
			get
			{
				// check if we need a target and there are some
				if (Card.RequiresTarget && !ValidPlayTargets.Any())
				{
					Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"{this} isn't playable, because need valid target and we don't have one.");
					return false;
				}

				// check requirments on cards here 
				foreach (var item in Card.PlayRequirements)
				{
					var req = item.Key;
					var param = item.Value;

					Game.Log(LogLevel.DEBUG, BlockType.PLAY, "Playable", $"{this} check PlayReq {req} ... !");

					switch (req)
					{
						case PlayReq.REQ_NUM_MINION_SLOTS:
							if (Controller.BoardZone.IsFull)
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"Board is full can't summon new minion.");
								return false;
							}
							break;
						case PlayReq.REQ_ENTIRE_ENTOURAGE_NOT_IN_PLAY:
							var ids = Controller.BoardZone.GetAll.Select(p => p.Card.Id).ToList();
							var containsAll = true;
							Card.Entourage.ForEach(p => containsAll &= ids.Contains(p));
							if (containsAll)
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"All ready all entourageing cards summoned.");
								return false;
							}
							break;
						case PlayReq.REQ_WEAPON_EQUIPPED:
							if (Controller.Hero.Weapon == null)
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"Need a weapon to play this card.");
								return false;
							}
							break;
						case PlayReq.REQ_MINIMUM_ENEMY_MINIONS:
							if (Controller.Opponent.BoardZone.Count < param)
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"Need at least {param} enemy minions to play this card.");
								return false;
							}
							break;
						case PlayReq.REQ_MINIMUM_TOTAL_MINIONS:
							if (Controller.BoardZone.Count + Controller.Opponent.BoardZone.Count < param)
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"Need at least {param} minions to play this card.");
								return false;
							}
							break;
						case PlayReq.REQ_STEADY_SHOT:
							if (!Controller.Hero.Power.Card.Id.Equals("DS1h_292"))
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"Need steady shoot to be used.");
								return false;
							}
							break;

						case PlayReq.REQ_FRIENDLY_MINION_DIED_THIS_GAME:
							if (!Controller.GraveyardZone.GetAll.Exists(p => p is Minion))
							{
								Game.Log(LogLevel.VERBOSE, BlockType.PLAY, "Playable", $"No friendly minions died this game.");
								return false;
							}
							break;

						// implemented in Targeting
						case PlayReq.REQ_TARGET_FOR_COMBO:
						case PlayReq.REQ_FROZEN_TARGET:
						case PlayReq.REQ_MINION_OR_ENEMY_HERO:
						case PlayReq.REQ_TARGET_MAX_ATTACK:
						case PlayReq.REQ_MINION_TARGET:
						case PlayReq.REQ_FRIENDLY_TARGET:
						case PlayReq.REQ_ENEMY_TARGET:
						case PlayReq.REQ_UNDAMAGED_TARGET:
						case PlayReq.REQ_DAMAGED_TARGET:
						case PlayReq.REQ_TARGET_WITH_RACE:
						case PlayReq.REQ_MUST_TARGET_TAUNTER:
						case PlayReq.REQ_TARGET_MIN_ATTACK:
						case PlayReq.REQ_TARGET_WITH_DEATHRATTLE:
						case PlayReq.REQ_TARGET_WITH_BATTLECRY:
						case PlayReq.REQ_TARGET_IF_AVAILABLE_AND_DRAGON_IN_HAND:
						case PlayReq.REQ_TARGET_IF_AVAILABE_AND_ELEMENTAL_PLAYED_LAST_TURN:
						case PlayReq.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_MINIONS:
						case PlayReq.REQ_TARGET_IF_AVAILABLE_AND_MINIMUM_FRIENDLY_SECRETS:
						case PlayReq.REQ_NONSELF_TARGET:
							break;

						// already implemented ... card.RequiresTarget and RequiresTargetIfAvailable
						case PlayReq.REQ_TARGET_TO_PLAY:
						case PlayReq.REQ_TARGET_IF_AVAILABLE:
							break;

						default:
							Game.Log(LogLevel.ERROR, BlockType.PLAY, "Playable", $"PlayReq {req} not in switch needs to be added (Playable)!");
							break;
					}
				}

				return true;

			}
		}

	}
}