using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node2D
{
    // Referencias de la UI
	private Label _levelLabel;
    private GridContainer _imagesGrid;
    private HBoxContainer _lettersBox;
    private VBoxContainer _keyboardLayout; // Ahora usamos un VBoxContainer

	private const string SavePath = "user://savegame.cfg";

    // Datos del juego
    private List<LevelData> _levels;
    private LevelData _currentLevel;
    private int _currentLevelIndex = 0;
    private int _currentMistakes = 0;

    // Estado de la jugada actual
    private char[] _userAnswer;

    public override void _Ready()
    {
        _imagesGrid = GetNode<GridContainer>("CanvasLayer/MainUI/MainLayout/ImagesGrid");
        _lettersBox = GetNode<HBoxContainer>("CanvasLayer/MainUI/MainLayout/LettersBox");
        // Conectamos el nuevo nodo
        _keyboardLayout = GetNode<VBoxContainer>("CanvasLayer/MainUI/MainLayout/KeyboardLayout");
		_levelLabel = GetNode<Label>("CanvasLayer/MainUI/MainLayout/LevelLabel"); 

        LoadLevels();
		LoadGame();
        //StartLevel(_currentLevelIndex);
    }

    private void LoadLevels()
    {
        _levels = new List<LevelData>()
        {
            new LevelData("COCINA", new List<string> { "Cuchillo", "Horno", "Sartén", "Licuadora", "Espátula" }),
            new LevelData("GIMNASIO", new List<string> { "Mancuerna", "Barra", "Polea", "Colchoneta", "Pesa" })
        };
    }

    private void StartLevel(int index)
    {
        if (index >= _levels.Count)
        {
            GD.Print("¡Juego Terminado! Completaste todos los niveles.");
            _levelLabel.Text = "FIN DEL JUEGO";
            return;
        }

        _currentLevelIndex = index;
        _currentLevel = _levels[index];
        _currentMistakes = 0;
        
        _userAnswer = new char[_currentLevel.FamilyName.Length];

        // NUEVO: Actualizamos el texto en pantalla (+1 porque los índices arrancan en 0)
        _levelLabel.Text = $"NIVEL {index + 1}";

        UpdateGameState();
        GenerateKeyboard();
    }

    private void UpdateGameState()
    {
        GenerateLetterSlots();
        GenerateObjectHints();
    }

    private void GenerateLetterSlots()
    {
        foreach (Node child in _lettersBox.GetChildren())
        {
            child.QueueFree();
        }

        for (int i = 0; i < _currentLevel.FamilyName.Length; i++)
        {
            Button slotButton = new Button();
            slotButton.CustomMinimumSize = new Vector2(60, 60);
            
            slotButton.Text = _userAnswer[i] == '\0' ? "_" : _userAnswer[i].ToString();
            
            // Desactivamos el botón superior para que no haga nada al tocarlo, 
            // ya que ahora controlamos el borrado con la tecla 'Backspace'
            slotButton.Disabled = true; 

            _lettersBox.AddChild(slotButton);
        }
    }

    private void GenerateObjectHints()
    {
        foreach (Node child in _imagesGrid.GetChildren())
        {
            child.QueueFree();
        }

        int objectsToShow = 2 + _currentMistakes;
        if (objectsToShow > _currentLevel.Objects.Count) objectsToShow = _currentLevel.Objects.Count;

        for (int i = 0; i < objectsToShow; i++)
        {
            Label hintLabel = new Label();
            hintLabel.Text = _currentLevel.Objects[i];
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hintLabel.VerticalAlignment = VerticalAlignment.Center;
            hintLabel.CustomMinimumSize = new Vector2(200, 80);
            _imagesGrid.AddChild(hintLabel);
        }
    }

    private void GenerateKeyboard()
    {
        // Limpiamos el teclado anterior
        foreach (Node child in _keyboardLayout.GetChildren())
        {
            child.QueueFree();
        }

        // Definimos las filas del teclado QWERTY
        string[] keyboardRows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

        for (int r = 0; r < keyboardRows.Length; r++)
        {
            // Creamos un contenedor horizontal para cada fila de teclas
            HBoxContainer rowContainer = new HBoxContainer();
            rowContainer.Alignment = BoxContainer.AlignmentMode.Center;
            rowContainer.AddThemeConstantOverride("separation", 4); // Espaciado entre teclas
            
            _keyboardLayout.AddChild(rowContainer);

            // Generamos los botones de las letras
            foreach (char letter in keyboardRows[r])
            {
                Button keyButton = new Button();
                keyButton.Text = letter.ToString();
                
                // Un tamaño un poco más estrecho para que entren 10 teclas en la pantalla
                keyButton.CustomMinimumSize = new Vector2(45, 60); 

                string letterStr = letter.ToString();
                keyButton.Pressed += () => OnKeyPressed(letterStr);

                rowContainer.AddChild(keyButton);
            }

            // Si estamos en la última fila ("ZXCVBNM"), agregamos el botón de borrar al final
            if (r == 2)
            {
                Button deleteButton = new Button();
                deleteButton.Text = "⌫"; 
                deleteButton.CustomMinimumSize = new Vector2(60, 60); // Un poco más ancho
                deleteButton.Pressed += OnDeletePressed;
                rowContainer.AddChild(deleteButton);
            }
        }
    }

    private void OnKeyPressed(string letter)
    {
        int emptyIndex = Array.IndexOf(_userAnswer, '\0');
        
        if (emptyIndex != -1)
        {
            _userAnswer[emptyIndex] = letter[0];
            UpdateGameState();

            if (Array.IndexOf(_userAnswer, '\0') == -1)
            {
                CheckAnswer();
            }
        }
    }

    private void OnDeletePressed()
    {
        // Recorremos el array de derecha a izquierda para borrar la última letra ingresada
        for (int i = _userAnswer.Length - 1; i >= 0; i--)
        {
            if (_userAnswer[i] != '\0')
            {
                _userAnswer[i] = '\0';
                UpdateGameState(); // Actualizamos la UI para que muestre el "_"
                break; // Salimos del bucle apenas borramos una letra
            }
        }
    }

    private void CheckAnswer()
    {
        string fullAnswer = new string(_userAnswer);

        if (fullAnswer == _currentLevel.FamilyName)
        {
            GD.Print("¡Correcto!");
            // NUEVO: Guardamos el progreso antes de saltar al siguiente nivel
            SaveGame(_currentLevelIndex + 1); 
            StartLevel(_currentLevelIndex + 1);
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
            }
            else
            {
                GD.Print("Game Over. Reiniciando nivel.");
                StartLevel(_currentLevelIndex);
            }
        }
    }

	private void SaveGame(int levelToSave)
    {
        ConfigFile config = new ConfigFile();
        // Guardamos en la sección "Progreso", la clave "Nivel", con el valor levelToSave
        config.SetValue("Progreso", "Nivel", levelToSave);
        config.Save(SavePath);
    }

    private void LoadGame()
    {
        ConfigFile config = new ConfigFile();
        
        // Intentamos cargar el archivo.
        Error err = config.Load(SavePath);
        
        if (err == Error.Ok) // Si el archivo existe (no es la primera vez que abre la app)
        {
            // Recuperamos el nivel (el último parámetro '0' es el valor por defecto si falla)
            int savedLevel = (int)config.GetValue("Progreso", "Nivel", 0);
            StartLevel(savedLevel);
        }
        else // Si el archivo NO existe (es un jugador nuevo)
        {
            StartLevel(0);
        }
    }
}

public class LevelData 
{
    public string FamilyName { get; set; }
    public List<string> Objects { get; set; }

    public LevelData(string familyName, List<string> objects) 
    {
        FamilyName = familyName;
        Objects = objects;
    }
}