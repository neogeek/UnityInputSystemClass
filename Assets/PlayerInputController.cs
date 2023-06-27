using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, PlayerInput.ICharacterActions
{

    private PlayerInput _playerInput;

    private Vector2 _movement;

    private bool _isFiring;

    public int playerIndex { get; set; }

    public HashSet<int> deviceIds { get; set; }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _playerInput.Character.SetCallbacks(this);
    }

    private void OnEnable()
    {
        _playerInput.Character.Enable();
    }

    private void OnDisable()
    {
        _playerInput.Character.Disable();
    }

    private void Update()
    {
        gameObject.transform.Translate(_movement * (Time.deltaTime * 10));
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!deviceIds.Contains(context.control.device.deviceId)) return;

        if (!context.performed)
        {
            _isFiring = false;

            return;
        }

        _isFiring = context.ReadValueAsButton();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!deviceIds.Contains(context.control.device.deviceId)) return;

        if (!context.performed)
        {
            _movement = Vector2.zero;

            return;
        }

        _movement = context.ReadValue<Vector2>();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        var text = $"Player {playerIndex}";

        var position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        var textSize = GUI.skin.label.CalcSize(new GUIContent(text));

        GUI.color = Color.black;
        GUI.Label(new Rect(position.x, Screen.height - position.y, textSize.x, textSize.y), text);
    }
#endif

}
