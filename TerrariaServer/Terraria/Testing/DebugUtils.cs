using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Testing.ChatCommands;
using Terraria.UI.Chat;

namespace Terraria.Testing;

public static class DebugUtils
{
	private static char[] _slopeIcons = new char[5] { '⬓', '◣', '◢', '◤', '◥' };

	internal static string GetTileDescription(int x, int y)
	{
		Tile tile = Main.tile[x, y];
		if (tile == null)
		{
			return "";
		}
		Point point = (Main.LocalPlayer.Bottom + new Vector2(-8f, 8f)).ToTileCoordinates();
		if (!TileID.Search.TryGetName(tile.type, out var name))
		{
			name = "Unknown";
		}
		if (!WallID.Search.TryGetName(tile.wall, out var name2))
		{
			name2 = "Unknown";
		}
		string text = "   ";
		return text + "Pos: " + x + ", " + y + "\n" + text + "Type: " + tile.type + ((tile.blockType() == 0) ? "" : (" " + _slopeIcons[tile.blockType() - 1])) + " (" + name + ")\n" + text + "Frame: " + tile.frameX + ", " + tile.frameY + "\n" + text + "FrameImportant: " + Main.tileFrameImportant[tile.type].ToString() + "\n" + text + "Liquid: " + tile.liquid + " (" + tile.liquidType() + ")\n" + text + "Wall: " + tile.wall + " (" + name2 + ")\n" + text + "Compare Spot: " + point.X + ", " + point.Y + "\n" + text + "Chunk: " + x / 200 + ", " + y / 150 + "\n" + text + "Paints: " + tile.color() + ", " + tile.wallColor();
	}

	internal static bool PracticeModeReset(Player player, PlayerDeathReason damageSource)
	{
		if (!DebugOptions.PracticeMode)
		{
			return false;
		}
		if (!NPC.AnyDanger(quickBossNPCCheck: false, ignorePillarsAndMoonlordCountdown: true))
		{
			return false;
		}
		player.statLife = player.statLifeMax2;
		for (int i = 0; i < Player.maxBuffs; i++)
		{
			if (player.buffTime[i] > 0)
			{
				int num = player.buffType[i];
				if (Main.debuff[num] && (num == 21 || !BuffID.Sets.NurseCannotRemoveDebuff[num]))
				{
					player.DelBuff(i);
					i = -1;
				}
			}
		}
		for (int j = 0; j < BuffID.Count; j++)
		{
			if (Main.debuff[j])
			{
				player.buffImmune[j] = true;
			}
		}
		string text = "unknown source";
		damageSource.TryGetCausingEntity(out var entity);
		if (entity is NPC)
		{
			text = ((NPC)entity).TypeName;
		}
		else if (entity is Projectile)
		{
			text = ((Projectile)entity).Name;
		}
		else if (entity is Player)
		{
			text = ((Player)entity).name;
		}
		Main.NewText("Lethal damage dealt by " + text, byte.MaxValue, byte.MaxValue, 0);
		if (Main.netMode != 0)
		{
			return true;
		}
		for (int k = 0; k < 1000; k++)
		{
			Projectile projectile = Main.projectile[k];
			if (projectile.active && projectile.hostile)
			{
				projectile.active = false;
			}
		}
		for (int l = 0; l < Main.maxNPCs; l++)
		{
			NPC nPC = Main.npc[l];
			if (nPC.active && !nPC.friendly && !nPC.isLikeATownNPC)
			{
				nPC.active = false;
			}
		}
		return true;
	}

	public static void QuickSPMessage(string message)
	{
		ChatManager.DebugCommands.Process(new DebugMessage((byte)Main.myPlayer, message));
	}
}
