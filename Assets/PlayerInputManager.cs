using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputManager : MonoBehaviour
{

    [SerializeField]
    private GameObject _prefab;

    [SerializeField]
    private int _maxPlayers = 6;

    private PlayerInputController[] _players;

    private readonly Dictionary<int, int> _deviceIdPlayerIndexMap = new();

    private void Awake()
    {
        _players = new PlayerInputController[_maxPlayers];
    }

    private void Start()
    {
        InputSystem.onAnyButtonPress
            .Call(inputControl =>
            {
                if (PlayerCanJoin())
                {
                    PlayerJoin(inputControl.device);
                }
            });
    }

    private void InputSystemOnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Enabled:
            case InputDeviceChange.Reconnected:
                if (PlayerCanJoin())
                {
                    PlayerJoin(device);
                }

                break;
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disabled:
            case InputDeviceChange.Disconnected:
                PlayerLeave(device);

                break;
        }
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += InputSystemOnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= InputSystemOnDeviceChange;
    }

    private void PlayerJoin(InputDevice device)
    {
        if (_deviceIdPlayerIndexMap.ContainsKey(device.deviceId)) return;

        var deviceIds =
            device == Keyboard.current || device == Mouse.current
                ? new HashSet<int> { Keyboard.current.deviceId, Mouse.current.deviceId }
                : new HashSet<int> { device.deviceId };

        var playerIndex = GetNextPlayerIndex(device);

        var playerInputController = Instantiate(_prefab).GetComponent<PlayerInputController>();

        playerInputController.playerIndex = playerIndex;

        foreach (var deviceId in deviceIds)
        {
            _deviceIdPlayerIndexMap.Add(deviceId, playerIndex);
        }

        playerInputController.deviceIds = deviceIds;

        _players[playerIndex] = playerInputController;
    }

    private bool PlayerCanJoin()
    {
        for (var i = 0; i < _players.Length; i += 1)
        {
            if (_players[i] == null)
            {
                return true;
            }
        }

        return false;
    }

    private void PlayerLeave(InputDevice device)
    {
        var deviceId = device.deviceId;

        if (!_deviceIdPlayerIndexMap.ContainsKey(deviceId)) return;

        var playerIndex = _deviceIdPlayerIndexMap[deviceId];

        var playerInputController = _players[playerIndex];

        RemoveDeviceIdForPlayerIndex(playerInputController.deviceIds);

        Destroy(playerInputController.gameObject);

        _players[playerIndex] = null;

    }

    private int GetNextPlayerIndex(InputDevice device)
    {
        {
            if (_deviceIdPlayerIndexMap.TryGetValue(device.deviceId, out var playerIndex))
            {
                return playerIndex;
            }
        }

        for (var i = 0; i < _players.Length; i += 1)
        {
            if (_players[i] == null)
            {
                return i;
            }
        }

        throw new IndexOutOfRangeException();
    }

    private void RemoveDeviceIdForPlayerIndex(HashSet<int> deviceIds)
    {
        foreach (var deviceId in deviceIds)
        {
            _deviceIdPlayerIndexMap.Remove(deviceId);
        }
    }

}
