using Godot;
using System.Threading.Tasks;

public partial class SplashScreen : Control
{
    private Label _statusLabel;
    private TextureRect _gato; // Tu placeholder del gato
    private Tween _rotationTween;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
        _gato = GetNode<TextureRect>("VBoxContainer/Gato");

        // Forzamos al gato a centrarse dentro de su propio contenedor
        _gato.PivotOffset = _gato.Size / 2.0f;

        // Animación de rotación
        _rotationTween = CreateTween().SetLoops();
        _rotationTween.TweenProperty(_gato, "rotation_degrees", 360, 2f)
                      .AsRelative()
                      .SetTrans(Tween.TransitionType.Linear);

        LoadGameDataAsync();
    }

    private async void LoadGameDataAsync()
    {
        _statusLabel.Text = "Conectando al servidor...";

        // --- Simulamos la carga de datos (o descomenta tu lógica real) ---
        // await Task.Run(() => tu_logica_de_carga());
        await ToSignal(GetTree().CreateTimer(2.0f), SceneTreeTimer.SignalName.Timeout); 

        _statusLabel.Text = "¡Datos listos!";

        // 3. Detenemos la animación y esperamos un breve momento antes de cambiar
        _rotationTween.Kill();
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }
}