using Godot;
using System.Threading.Tasks;

public partial class SplashScreen : Control
{
    private Label _statusLabel;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("StatusLabel");
        
        // Llamamos al método de carga apenas arranca la pantalla
        LoadGameDataAsync();
    }

    private async void LoadGameDataAsync()
    {
        _statusLabel.Text = "Conectando al servidor...";

        // --- SUPABASE EN PAUSA PARA DESARROLLO ---
        // APIManager api = GetNode<APIManager>("/root/APIManager");
        // await api.FetchLevelsFromServer();

        _statusLabel.Text = "¡Datos listos!";

        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}