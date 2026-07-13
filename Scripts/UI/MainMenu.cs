using Godot;

public partial class MainMenu : Control
{
    private Button _playButton;

    public override void _Ready()
    {
        _playButton = GetNode<Button>("CenterContainer/VBoxContainer/PlayButton");
        _playButton.Pressed += OnPlayButtonPressed;

        // Evaluamos dinámicamente si el botón debe decir Jugar o Continuar
        if (SaveSystem.HasSavedGame())
        {
            _playButton.Text = "Continuar";
        }
        else
        {
            _playButton.Text = "Jugar";
        }
    }

    private void OnPlayButtonPressed()
    {
        // Dirige al jugador al espacio de juego principal
        GetTree().ChangeSceneToFile("res://Scenes/game_manager.tscn");
    }


	public override void _Input(InputEvent @event)
    {
        // Si es un evento de teclado, la tecla fue presionada, y es F12
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F12)
        {
            // 1. Borramos el archivo físico usando el método que ya teníamos
            SaveSystem.ClearSave();
            
            GD.Print("¡Modo Debug: Partida borrada!");
            
            // 2. Recargamos la escena actual para que el botón vuelva a decir "Jugar"
            GetTree().ReloadCurrentScene();
        }
    }
}
