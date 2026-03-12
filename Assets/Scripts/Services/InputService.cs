using UnityEngine;
using Object = UnityEngine.Object;

public class InputService
{
    // Можна легко змінити на New Input System
    private InputActionPlayer _actionPlayer;
    public Vector2 MoveDirection { get; private set; }
    public bool IsInputEnabled { get; set; } = true;
    
    public InputService()
    {
        _actionPlayer = new InputActionPlayer();
        _actionPlayer.Enable();
        // Створюємо MonoBehaviour для Update
        var inputGO = new GameObject("[Input]");
        var updater = inputGO.AddComponent<InputUpdater>();
        updater.Initialize(this);
        Object.DontDestroyOnLoad(inputGO);
    }
    
    public void UpdateInput()
    {
        if (!IsInputEnabled)
        {
            MoveDirection = Vector2.zero;
            return;
        }
        
        // Рух
        MoveDirection = _actionPlayer.Player.Movememnt.ReadValue<Vector2>();
        
    }
}
