﻿namespace SabberStoneCore.Enums
{
	/// <summary>
	/// Indicates the event that must happen in order to trigger the effect
	/// of the attached enchantment.
	/// 
	/// eg: <see cref="DEATHRATTLE"/>: The entity has to be destroyed in order
	/// for the effect to execute.
	/// </summary>
	public enum EnchantmentActivation
	{
		BATTLECRY,
		DEATHRATTLE,
		WEAPON,
		SPELL,
		SECRET_OR_QUEST,

		BOARD_ZONE,
		HAND_ZONE,
		DECK_ZONE,
		SETASIDE_ZONE,
		NONE
	}

	/// <summary>
	/// Indicates which (group of) entities the effect will influence.
	/// </summary>
	public enum EnchantmentArea
	{
		/// <summary>
		/// There will be no effect.
		/// </summary>
		NONE,

		/// <summary>
		/// The effect will be triggered by the selected target (target chosen by player).
		/// </summary>
		TARGET,

		/// <summary>
		/// The effect will be triggered by the hero of the controller.
		/// </summary>
		HERO,

		/// <summary>
		/// The effect will be triggered by the hero of the opponent controller.
		/// </summary>
		OP_HERO,

		/// <summary>
		/// The effect will be triggered by the hero of all controllers.
		/// </summary>
		HEROES,

		/// <summary>
		/// The effect will be triggered by the board of the controller.
		/// </summary>
		BOARD,

		/// <summary>
		/// The effect will be triggered by the board of the opponent controller.
		/// </summary>
		OP_BOARD,

		/// <summary>
		/// The effect will be triggered by the board of all controllers.
		/// </summary>
		BOARDS,

		/// <summary>
		/// The effect will be triggered by the hand of the controller.
		/// </summary>
		HAND,

		/// <summary>
		/// The effect will be triggered by the hand of the opponent controller.
		/// </summary>
		OP_HAND,

		/// <summary>
		/// The effect will be triggered by the hand of all controllers.
		/// </summary>
		HANDS,

		/// <summary>
		/// The effect will be triggered by the entity itself.
		/// </summary>
		SELF,

		/// <summary>
		/// The effect will be triggered by the GAME entity.
		/// </summary>
		GAME,

		/// <summary>
		/// The effect will be triggered by the controller entity.
		/// </summary>
		CONTROLLER,

		/// <summary>
		/// The effect will be triggered by the opponent controller entity.
		/// </summary>
		OP_CONTROLLER,

		/// <summary>
		/// The effect will be triggered by all controller entities.
		/// </summary>
		CONTROLLERS,

		/// <summary>
		/// The effect will be triggered by the secrets of the controller.
		/// </summary>
		SECRET,

		/// <summary>
		/// The effect will be triggered by the HAND and BOARD of the controller.
		/// </summary>
		HAND_AND_BOARD,

		/// <summary>
		/// The effect will be triggered by the HAND and BOARD of the opponent controller.
		/// </summary>
		OP_BOARD_AND_OP_HERO,

		/// <summary>
		/// The effect will be triggered by the HERO and BOARD of all controllers.
		/// </summary>
		BOARDS_HEROES,
	}
}
