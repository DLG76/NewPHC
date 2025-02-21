using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerManager
{
    public static bool IsSinglePlayer = true;
    public static PlayerCombat RandomCharacter
    {
        get
        {
            if (Characters.Length == 0) return null;

            return Characters[Random.Range(0, Characters.Length)];
        }
    }
    public static PlayerCombat[] Characters { get => _players.Where(p => p.Character != null).Select(p => p.Character).ToArray(); }
    public static PlayerCombatData[] Players { get => _players.ToArray(); }
    private static List<PlayerCombatData> _players = new List<PlayerCombatData>();

    public static void AddPlayer(PlayerCombatData player)
    {
        if (_players.Contains(player)) return;

        _players.Add(player);
    }

    public static void RemovePlayer(PlayerCombatData player)
    {
        if (!_players.Contains(player)) return;

        _players.Remove(player);
    }

    public static PlayerCombatData GetPlayerFromCharacter(PlayerCombat character)
    {
        return _players.Find(p => p.Character == character);
    }
}
