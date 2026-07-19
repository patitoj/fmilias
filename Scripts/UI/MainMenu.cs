using Godot;

public partial class MainMenu : Control
{
    private Button _playButton;
    private Tween _buttonTween;
    private Vector2 _originalScale = Vector2.One;

    public override void _Ready()
    {
        // Enlazamos el botón (ajustá la ruta si tu jerarquía es distinta)
        _playButton = GetNode<Button>("MainContainer/PlayButton");
        _playButton.PivotOffset = _playButton.Size / 2.0f;

        // Conectamos las señales nativas del botón a nuestros métodos
        _playButton.MouseEntered += OnButtonHoverEnter;
        _playButton.MouseExited += OnButtonHoverExit;
        _playButton.ButtonDown += OnButtonPressDown;
        _playButton.ButtonUp += OnButtonPressUp;
    }

    // Método central para animar la escala de forma fluida
    private void AnimateButtonScale(Vector2 targetScale, float duration)
    {
        // Si hay una animación reproduciéndose, la matamos para que no se superpongan
        if (_buttonTween != null && _buttonTween.IsValid())
        {
            _buttonTween.Kill();
        }

        // Creamos la nueva animación con un efecto de transición "Back" (da ese pequeño rebote)
        _buttonTween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        _buttonTween.TweenProperty(_playButton, "scale", targetScale, duration);
    }

    // --- SEÑALES ---

    // Cuando el mouse/dedo entra: se agranda un 5%
    private void OnButtonHoverEnter()
    {
        AnimateButtonScale(_originalScale * 1.05f, 0.2f);
    }

    // Cuando el mouse/dedo sale: vuelve a la normalidad
    private void OnButtonHoverExit()
    {
        AnimateButtonScale(_originalScale, 0.2f);
    }

    // Cuando se aprieta el botón: se encoge un 5% (se hunde)
    private void OnButtonPressDown()
    {
        AnimateButtonScale(_originalScale * 0.95f, 0.1f);
    }

    // Cuando se suelta el botón: vuelve al tamaño de Hover y arranca el juego
    private void OnButtonPressUp()
    {
        // Vuelve a estar un poco grande porque el mouse sigue encima
        AnimateButtonScale(_originalScale * 1.05f, 0.2f);
        
        // ¡Acá llamaremos al cambio de escena al GameManager!
        GetTree().ChangeSceneToFile("res://Scenes/game_manager.tscn");
    }
}