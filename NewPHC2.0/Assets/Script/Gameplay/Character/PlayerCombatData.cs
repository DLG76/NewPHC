using UnityEngine;

public class PlayerCombatData
{
    public bool IsLocalPlayer { get => _isLocalPlayer; }
    private bool _isLocalPlayer;
    public PlayerCombat Character { get => _character; }
    private PlayerCombat _character;

    public PlayerCombatData(bool isLocalPlayer, PlayerCombat character)
    {
        _isLocalPlayer = isLocalPlayer;
        _character = character;
    }

    public void SetCharacter(PlayerCombat character)
    {
        if (_character != null && _character != character)
            Object.Destroy(_character);

        _character = character;
    }
}
