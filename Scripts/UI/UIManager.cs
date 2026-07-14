using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
    private Label _levelLabel;
    private GridContainer _imagesGrid;
    private HBoxContainer _lettersBox;
    private VBoxContainer _keyboardLayout;

    // Eventos (Delegados)
    public Action<string> OnLetterPressedAction;
    public Action OnDeletePressedAction;

    // --- PALETA DE COLORES "ALGODÓN DE AZÚCAR" ---
    private readonly Color[] _vividColors = new Color[]
    {
        new Color("#72cbe7"), // Amarillo
        new Color("#fad66e"), // Cian
        new Color("#8dd4ac"), // Rosa/Rojo
        new Color("#fe877c") // Verde // Azul
    };
    
    private readonly Color _borderColor = new Color("#2B2B2B");
    private readonly Color _textColor = new Color("#4A4E69"); // Violeta/Gris oscuro (Reemplaza al negro)
    private readonly Color _shadowColor = new Color("#4A4E69"); // Sombra para el efecto 3D
    private readonly Color _disabledSlotColor = new Color("#E2E2E2"); // Gris claro y cálido para ranuras vacías

    public override void _Ready()
    {
        _imagesGrid = GetNode<GridContainer>("MainLayout/ImagesGrid");
        _lettersBox = GetNode<HBoxContainer>("MainLayout/BottomUI/LettersBox");
        _keyboardLayout = GetNode<VBoxContainer>("MainLayout/BottomUI/KeyboardCard/KeyboardLayout");
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
            
            // Cuadrados grandes
            slotButton.CustomMinimumSize = new Vector2(80, 85); 
            
            slotButton.Text = userAnswer[i] == '\0' ? " " : userAnswer[i].ToString();
            slotButton.Disabled = true; 

            StyleBoxFlat slotStyle = new StyleBoxFlat();
            slotStyle.CornerRadiusTopLeft = 16;
            slotStyle.CornerRadiusTopRight = 16;
            slotStyle.CornerRadiusBottomRight = 16;
            slotStyle.CornerRadiusBottomLeft = 16;

            if (userAnswer[i] != '\0')
            {
                // Letra ingresada: Blanco puro con borde gris claro muy sutil
                slotStyle.BgColor = new Color("#FFFFFF");
                slotStyle.BorderColor = new Color("#D1D1D1"); // Gris perlado suave
                slotStyle.BorderWidthTop = 2;
                slotStyle.BorderWidthLeft = 2;
                slotStyle.BorderWidthRight = 2;
                slotStyle.BorderWidthBottom = 4; // Rebajamos el relieve de 6 a 4 para mayor sutileza
            }
            else
            {
                // Hueco vacío: Gris cálido con borde sutil
                slotStyle.BgColor = _disabledSlotColor;
                slotStyle.BorderColor = new Color("#C5C5C5"); 
                slotStyle.BorderWidthTop = 2;
                slotStyle.BorderWidthLeft = 2;
                slotStyle.BorderWidthRight = 2;
                slotStyle.BorderWidthBottom = 4;
            }

            slotButton.AddThemeStyleboxOverride("disabled", slotStyle);
            
            // Letra oscura y grande para que resalte sobre el blanco
            slotButton.AddThemeColorOverride("font_disabled_color", _textColor);
            slotButton.AddThemeFontSizeOverride("font_size", 45); 

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
            
            // Le damos color oscuro a la fuente de las pistas también
            hintLabel.AddThemeColorOverride("font_color", _textColor);
            
            _imagesGrid.AddChild(hintLabel);
        }
    }

    public void GenerateKeyboard()
    {
        foreach (Node child in _keyboardLayout.GetChildren()) child.QueueFree();

        string[] keyboardRows = { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" };
        Random random = new Random();

        for (int r = 0; r < keyboardRows.Length; r++)
        {
            HBoxContainer rowContainer = new HBoxContainer();
            rowContainer.Alignment = BoxContainer.AlignmentMode.Center;
            rowContainer.AddThemeConstantOverride("separation", 6);
            _keyboardLayout.AddChild(rowContainer);

            foreach (char letter in keyboardRows[r])
            {
                // 1. EL ENVOLTORIO NEGRO
                PanelContainer keyWrapper = new PanelContainer();
                keyWrapper.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                keyWrapper.CustomMinimumSize = new Vector2(0, 65); 
                
                StyleBoxFlat wrapperStyle = new StyleBoxFlat();
                wrapperStyle.BgColor = _borderColor; 
                wrapperStyle.CornerRadiusTopLeft = 14;
                wrapperStyle.CornerRadiusTopRight = 14;
                wrapperStyle.CornerRadiusBottomRight = 14;
                wrapperStyle.CornerRadiusBottomLeft = 14;

                // --- NUEVO: Sombra exterior suave ---
                wrapperStyle.ShadowColor = new Color(0, 0, 0, 0.2f); // Negro al 20% de opacidad
                wrapperStyle.ShadowSize = 4; // Difuminado sutil
                wrapperStyle.ShadowOffset = new Vector2(0, 4); // Desplazada hacia abajo
                // ------------------------------------

                keyWrapper.AddThemeStyleboxOverride("panel", wrapperStyle);

                // 2. EL MARGEN
                MarginContainer margin = new MarginContainer();
                margin.AddThemeConstantOverride("margin_top", 2);
                margin.AddThemeConstantOverride("margin_left", 2);
                margin.AddThemeConstantOverride("margin_right", 2);
                margin.AddThemeConstantOverride("margin_bottom", 2);
                keyWrapper.AddChild(margin);

                // 3. LA TECLA REAL
                Button keyButton = new Button();
                keyButton.Text = letter.ToString();
                keyButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill; 

                Color randomColor = _vividColors[random.Next(_vividColors.Length)];
                
                keyButton.AddThemeStyleboxOverride("normal", CreateCandyStyle(randomColor));
                keyButton.AddThemeStyleboxOverride("hover", CreateCandyStyle(randomColor.Lightened(0.1f))); 
                keyButton.AddThemeStyleboxOverride("pressed", CreateCandyStyle(randomColor, true));
                keyButton.AddThemeStyleboxOverride("focus", CreateCandyStyle(randomColor)); 
                
                keyButton.AddThemeColorOverride("font_color", new Color("#FFFFFF"));
                keyButton.AddThemeFontSizeOverride("font_size", 34);

                string letterStr = letter.ToString();
                keyButton.Pressed += () => OnLetterPressedAction?.Invoke(letterStr);

                margin.AddChild(keyButton);
                rowContainer.AddChild(keyWrapper);
            }

            // --- TECLA DE BORRAR ---
            if (r == 2)
            {
                PanelContainer deleteWrapper = new PanelContainer();
                deleteWrapper.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                deleteWrapper.CustomMinimumSize = new Vector2(0, 65);
                
                StyleBoxFlat wrapperDelStyle = new StyleBoxFlat();
                wrapperDelStyle.BgColor = _borderColor; 
                wrapperDelStyle.CornerRadiusTopLeft = 14;
                wrapperDelStyle.CornerRadiusTopRight = 14;
                wrapperDelStyle.CornerRadiusBottomRight = 14;
                wrapperDelStyle.CornerRadiusBottomLeft = 14;

                // --- NUEVO: Sombra exterior para la tecla de borrar ---
                wrapperDelStyle.ShadowColor = new Color(0, 0, 0, 0.2f);
                wrapperDelStyle.ShadowSize = 4;
                wrapperDelStyle.ShadowOffset = new Vector2(0, 4);
                // ------------------------------------------------------

                deleteWrapper.AddThemeStyleboxOverride("panel", wrapperDelStyle);

                MarginContainer margin = new MarginContainer();
                margin.AddThemeConstantOverride("margin_top", 2);
                margin.AddThemeConstantOverride("margin_left", 2);
                margin.AddThemeConstantOverride("margin_right", 2);
                margin.AddThemeConstantOverride("margin_bottom", 2);
                deleteWrapper.AddChild(margin);

                Button deleteButton = new Button();
                deleteButton.Text = "⌫"; 
                deleteButton.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                
                Color deleteColor = new Color("#fe877c"); // Coral
                deleteButton.AddThemeStyleboxOverride("normal", CreateCandyStyle(deleteColor));
                deleteButton.AddThemeStyleboxOverride("hover", CreateCandyStyle(deleteColor.Lightened(0.1f)));
                deleteButton.AddThemeStyleboxOverride("pressed", CreateCandyStyle(deleteColor, true));
                deleteButton.AddThemeStyleboxOverride("focus", CreateCandyStyle(deleteColor));
                
                deleteButton.AddThemeColorOverride("font_color", new Color("#FFFFFF"));
                deleteButton.AddThemeFontSizeOverride("font_size", 28);
                
                deleteButton.Pressed += () => OnDeletePressedAction?.Invoke();
                
                margin.AddChild(deleteButton);
                rowContainer.AddChild(deleteWrapper);
            }
        }
    }

    // --- MÉTODO AUXILIAR PARA EL DISEÑO "JUICY" ---
    private StyleBoxFlat CreateCandyStyle(Color backgroundColor, bool isPressed = false)
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = backgroundColor;
        
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomRight = 12;
        style.CornerRadiusBottomLeft = 12;

        // Ya no dependemos de las sombras fallidas. Usamos el Borde interno para la textura oscura
        style.BorderColor = backgroundColor.Darkened(0.35f); 
        
        if (isPressed)
        {
            // Al presionar, la base oscura desaparece y se hunde simulando un click físico
            style.BorderWidthBottom = 0; 
            style.ContentMarginTop = 10; // Empuja la letra blanca hacia abajo
        }
        else
        {
            // Estado normal: 10px de profundidad oscura en la base
            style.BorderWidthBottom = 6; 
            style.ContentMarginTop = 0;
        }

        return style;
    }
}