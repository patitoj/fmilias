using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
    private Label _levelLabel;
    private GridContainer _imagesGrid;
    private HBoxContainer _lettersBox;
    private VBoxContainer _keyboardLayout;

    // Eventos (Delegados) para avisarle al GameManager qué botón tocó el usuario
    public Action<string> OnLetterPressedAction;
    public Action OnDeletePressedAction;

    public override void _Ready()
    {
        // Al estar atado al nodo MainUI, las rutas ahora son más cortas
        _imagesGrid = GetNode<GridContainer>("MainLayout/ImagesGrid");
        _lettersBox = GetNode<HBoxContainer>("MainLayout/LettersBox");
        _keyboardLayout = GetNode<VBoxContainer>("MainLayout/KeyboardLayout");
        _levelLabel = GetNode<Label>("MainLayout/LevelLabel");
    }

    public void UpdateLevelLabel(int levelNumber)
    {
        _levelLabel.Text = $"NIVEL {levelNumber}";
    }

    public void GenerateLetterSlots(int wordLength, char[] userAnswer)
    {
        foreach (Node child in _lettersBox.GetChildren()) child.QueueFree();

        for (int i = 0; i < wordLength; i++)
        {
            Button slotButton = new Button();
            slotButton.CustomMinimumSize = new Vector2(60, 60);
            slotButton.Text = userAnswer[i] == '\0' ? "_" : userAnswer[i].ToString();
            slotButton.Disabled = true; 
            _lettersBox.AddChild(slotButton);
        }
    }

    public void GenerateObjectHints(List<string> objects, int mistakes)
    {
        foreach (Node child in _imagesGrid.GetChildren()) child.QueueFree();

        int objectsToShow = 2 + mistakes;
        if (objectsToShow > objects.Count) objectsToShow = objects.Count;

        for (int i = 0; i < objectsToShow; i++)
        {
            Label hintLabel = new Label();
            hintLabel.Text = objects[i];
            hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hintLabel.VerticalAlignment = VerticalAlignment.Center;
            hintLabel.CustomMinimumSize = new Vector2(200, 80);
            _imagesGrid.AddChild(hintLabel);
        }
    }

    public void GenerateKeyboard()
    {
        foreach (Node child in _keyboardLayout.GetChildren()) child.QueueFree();

        string[] keyboardRows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };

        for (int r = 0; r < keyboardRows.Length; r++)
        {
            HBoxContainer rowContainer = new HBoxContainer();
            rowContainer.Alignment = BoxContainer.AlignmentMode.Center;
            rowContainer.AddThemeConstantOverride("separation", 4);
            _keyboardLayout.AddChild(rowContainer);

            foreach (char letter in keyboardRows[r])
            {
                Button keyButton = new Button();
                keyButton.Text = letter.ToString();
                keyButton.CustomMinimumSize = new Vector2(45, 60); 

                string letterStr = letter.ToString();
                // Al presionar la tecla, disparamos el evento para que el GameManager lo escuche
                keyButton.Pressed += () => OnLetterPressedAction?.Invoke(letterStr);

                rowContainer.AddChild(keyButton);
            }

            if (r == 2)
            {
                Button deleteButton = new Button();
                deleteButton.Text = "⌫"; 
                deleteButton.CustomMinimumSize = new Vector2(60, 60);
                deleteButton.Pressed += () => OnDeletePressedAction?.Invoke();
                rowContainer.AddChild(deleteButton);
            }
        }
    }
}
