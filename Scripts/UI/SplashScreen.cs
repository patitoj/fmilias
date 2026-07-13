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

        // 1. Buscamos el cerebro de la red (nuestro Autoload)
        APIManager api = GetNode<APIManager>("/root/APIManager");

        // 2. Ejecutamos la tarea de descarga y ESPERAMOS (await) a que termine
        await api.FetchLevelsFromServer();

        // 3. Cuando la línea de arriba termina, el código continúa acá
        _statusLabel.Text = "¡Datos listos!";

        // Hacemos una micropausa de medio segundo para que no sea un pantallazo tan brusco
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        // 4. Cambiamos a la escena del menú principal
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}