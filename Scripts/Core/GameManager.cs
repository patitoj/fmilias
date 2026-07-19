using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;


public partial class GameManager : Node2D
{
    private UIManager _uiManager;

    private List<LevelData> _levels;
    private LevelData _currentLevel;
    private int _currentLevelIndex = 0;
    private int _currentMistakes = 0;
    private char[] _userAnswer;

    private ColorRect _bgColor;

    public override void _Ready()
    {
        _uiManager = GetNode<UIManager>("CanvasLayer/MainUI");
        _uiManager.OnLetterPressedAction += OnKeyPressed;
        _uiManager.OnDeletePressedAction += OnDeletePressed;
        _uiManager.OnBackButtonPressedAction += GoToMainMenu;
        _bgColor = GetNode<ColorRect>("CanvasLayer/BgColor");

        // ==========================================
        // CÓDIGO DE PRODUCCIÓN (SUPABASE) - EN PAUSA
        // ==========================================
        /*
        APIManager api = GetNode<APIManager>("/root/APIManager");
        _levels = api.DownloadedLevels;
        */

        // ==========================================
        // CÓDIGO DE DESARROLLO (LOCAL) - ACTIVO
        // ==========================================
        LoadLevelsFromLocalJson();

        // Comprobamos si hay datos guardados para reanudar o empezar de cero
        if (SaveSystem.HasSavedGame())
        {
            var (savedLevel, savedMistakes, savedAnswer) = SaveSystem.LoadGameState();
            StartLevel(savedLevel, savedMistakes, savedAnswer);
        }
        else
        {
            StartLevel(0, 0, "");
        }
    }

    private void GoToMainMenu()
    {
        GetTree().ChangeSceneToFile("res://Scenes/main_menu.tscn");
    }

    // --- NUEVO MÉTODO DE CARGA LOCAL ---
    private void LoadLevelsFromLocalJson()
    {
        string filePath = "res://levels.json"; 

        if (!FileAccess.FileExists(filePath))
        {
            GD.PrintErr($"CRÍTICO: No se encontró el archivo {filePath}");
            return;
        }

        using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string jsonString = file.GetAsText();

        try
        {
            // PropertyNameCaseInsensitive permite que C# enlace el "FamilyName" de tu JSON
            // con el [JsonPropertyName("family_name")] que ya tenías preparado para Supabase
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _levels = JsonSerializer.Deserialize<List<LevelData>>(jsonString, options);
            
            GD.Print($"ÉXITO: Se cargaron {_levels.Count} niveles desde el archivo local.");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error al leer el JSON: {e.Message}");
        }
    }

    // Adaptamos el método para recibir parámetros opcionales de reconstrucción
    private void StartLevel(int index, int mistakes = 0, string savedAnswer = "")
    {
        if (index >= _levels.Count)
        {
            GD.Print("¡Juego Terminado! Completaste todos los niveles.");
            SaveSystem.ClearSave();
            return;
        }

        _currentLevelIndex = index;
        _currentLevel = _levels[index];
        _currentMistakes = mistakes;
        _userAnswer = new char[_currentLevel.FamilyName.Length];

        // UpdateBackgroundColor(_currentLevel.FamilyName);

        // Si hay una respuesta guardada y coincide en longitud, la restauramos
        if (!string.IsNullOrEmpty(savedAnswer) && savedAnswer.Length == _currentLevel.FamilyName.Length)
        {
            for (int i = 0; i < _userAnswer.Length; i++)
            {
                _userAnswer[i] = savedAnswer[i] == '_' ? '\0' : savedAnswer[i];
            }
        }

        _uiManager.UpdateLevelLabel(index + 1);
        UpdateGameState();
        _uiManager.GenerateKeyboard();
        
        // Registramos el estado en el disco inmediatamente
        SaveCurrentProgress();
    }

    private void UpdateGameState()
    {
        _uiManager.GenerateLetterSlots(_currentLevel.FamilyName.Length, _userAnswer);
        _uiManager.GenerateObjectHints(_currentLevel.Objects, _currentMistakes);
    }

    private void OnKeyPressed(string letter)
    {
        int emptyIndex = Array.IndexOf(_userAnswer, '\0');
        
        if (emptyIndex != -1)
        {
            _userAnswer[emptyIndex] = letter[0];
            UpdateGameState();
            SaveCurrentProgress(); // Guarda la letra recién puesta

            if (Array.IndexOf(_userAnswer, '\0') == -1)
            {
                CheckAnswer();
            }
        }
    }

    private void OnDeletePressed()
    {
        for (int i = _userAnswer.Length - 1; i >= 0; i--)
        {
            if (_userAnswer[i] != '\0')
            {
                _userAnswer[i] = '\0';
                UpdateGameState();
                SaveCurrentProgress(); // Guarda el cambio al borrar
                break;
            }
        }
    }

    private void CheckAnswer()
    {
        string fullAnswer = new string(_userAnswer);

        if (fullAnswer == _currentLevel.FamilyName)
        {
            GD.Print("¡Correcto!");
            // Siguiente nivel inicia limpio (0 errores, sin texto previo)
            StartLevel(_currentLevelIndex + 1, 0, ""); 
        }
        else
        {
            GD.Print("Incorrecto.");
            _currentMistakes++;

            for (int i = 0; i < _userAnswer.Length; i++)
            {
                _userAnswer[i] = '\0';
            }

            if (2 + _currentMistakes <= _currentLevel.Objects.Count)
            {
                UpdateGameState();
                SaveCurrentProgress(); // Guarda el nuevo error cometido y el reset de letras
            }
            /*else
            {
                GD.Print("Game Over. Reiniciando nivel.");
                StartLevel(_currentLevelIndex, 0, ""); // Si pierde el nivel completo, se limpia ese nivel
            }*/
        }
    }

    // Convierte el estado actual del juego en una estructura almacenable
    private void SaveCurrentProgress()
    {
        char[] saveChars = new char[_userAnswer.Length];
        for (int i = 0; i < _userAnswer.Length; i++)
        {
            saveChars[i] = _userAnswer[i] == '\0' ? '_' : _userAnswer[i];
        }
        string answerStr = new string(saveChars);

        SaveSystem.SaveGameState(_currentLevelIndex, _currentMistakes, answerStr);
    }
    /*private void UpdateBackgroundColor(string categoryName)
    {
        // Asignamos un color pastel de la paleta según la familia de la palabra
        switch (categoryName.ToUpper())
        {
            case "COCINA":
                _bgColor.Color = new Color("#FFB7B2"); // Rosa salmón
                break;
            case "GIMNASIO":
                _bgColor.Color = new Color("#B5EAD7"); // Verde menta
                break;
            case "HARDWARE":
                _bgColor.Color = new Color("#C7CEEA"); // Lavanda pastel
                break;
            case "LITERATURA":
                _bgColor.Color = new Color("#FFDAC1"); // Durazno suave
                break;
            default:
                _bgColor.Color = new Color("#FDF6E3"); // Crema por defecto
                break;
        }
    }*/

    public override void _Input(InputEvent @event)
    {
        // Detectamos si la tecla presionada es F12
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.F12)
            {
                GD.Print("F12 presionado: Reiniciando al nivel 0...");
                
                // Borramos el progreso guardado para asegurar que no reanude donde quedó
                SaveSystem.ClearSave();
                
                // Reiniciamos al nivel 0, con 0 errores y sin letras en la respuesta
                StartLevel(0, 0, "");
            }
        }
    }
}