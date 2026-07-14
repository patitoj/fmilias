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
        _lettersBox = GetNode<HBoxContainer>("MainLayout/LettersBox");
        _keyboardLayout = GetNode<VBoxContainer>("MainLayout/KeyboardCard/KeyboardLayout");
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
                Button keyButton = new Button();
                keyButton.Text = letter.ToString();
                
                // --- 2. TECLADO GRANDE (Se estira a los costados) ---
                keyButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                keyButton.CustomMinimumSize = new Vector2(0, 65); // Altura fija, ancho dinámico

                Color randomColor = _vividColors[random.Next(_vividColors.Length)];
                
                // --- 4. HOVER Y ANIMACIONES ---
                StyleBoxFlat normalStyle = CreateCandyStyle(randomColor);
                StyleBoxFlat hoverStyle = CreateCandyStyle(randomColor.Lightened(0.1f)); // Más brillante al pasar el dedo
                StyleBoxFlat pressedStyle = CreateCandyStyle(randomColor, true);

                keyButton.AddThemeStyleboxOverride("normal", normalStyle);
                keyButton.AddThemeStyleboxOverride("hover", hoverStyle);
                keyButton.AddThemeStyleboxOverride("pressed", pressedStyle);
                keyButton.AddThemeStyleboxOverride("focus", normalStyle); // Evita el recuadro gris raro al hacer clic
                
                // --- 3. LETRAS BLANCAS, GRUESAS Y SIN BORDE ---
                keyButton.AddThemeColorOverride("font_color", new Color("#FFFFFF"));
                keyButton.AddThemeFontSizeOverride("font_size", 34); // Letra mucho más grande

                string letterStr = letter.ToString();
                keyButton.Pressed += () => OnLetterPressedAction?.Invoke(letterStr);

                rowContainer.AddChild(keyButton);
            }

            // Tecla de borrar
            if (r == 2)
            {
                Button deleteButton = new Button();
                deleteButton.Text = "⌫"; 
                deleteButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                deleteButton.CustomMinimumSize = new Vector2(0, 65);
                
                Color deleteColor = new Color("#FF6B6B"); 
                StyleBoxFlat normalDel = CreateCandyStyle(deleteColor);
                StyleBoxFlat hoverDel = CreateCandyStyle(deleteColor.Lightened(0.1f));
                StyleBoxFlat pressedDel = CreateCandyStyle(deleteColor, true);

                deleteButton.AddThemeStyleboxOverride("normal", normalDel);
                deleteButton.AddThemeStyleboxOverride("hover", hoverDel);
                deleteButton.AddThemeStyleboxOverride("pressed", pressedDel);
                deleteButton.AddThemeStyleboxOverride("focus", normalDel);
                
                deleteButton.AddThemeColorOverride("font_color", new Color("#FFFFFF"));
                deleteButton.AddThemeFontSizeOverride("font_size", 28);
                
                deleteButton.Pressed += () => OnDeletePressedAction?.Invoke();
                rowContainer.AddChild(deleteButton);
            }
        }
    }

    // --- MÉTODO AUXILIAR PARA EL DISEÑO "JUICY" ---
    private StyleBoxFlat CreateCandyStyle(Color backgroundColor, bool isPressed = false)
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = backgroundColor;
        
        // Bordes redondeados
        style.CornerRadiusTopLeft = 12;
        style.CornerRadiusTopRight = 12;
        style.CornerRadiusBottomRight = 12;
        style.CornerRadiusBottomLeft = 12;

        // Borde negro fino perimetral
        style.BorderColor = _borderColor; 
        style.BorderWidthTop = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthBottom = 2;
        
        if (isPressed)
        {
            // Al presionar, se elimina el relieve y la tecla "cae" hasta el fondo
            style.ShadowSize = 0;
            style.ShadowOffset = new Vector2(0, 0);
            
            // Empuja el texto exactamente la misma cantidad de píxeles que mide la base
            style.ContentMarginTop = 12; 
        }
        else
        {
            // 1. Aumentamos el contraste (Darkened 0.35f en lugar de 0.25f) para que sea más marcado
            style.ShadowColor = backgroundColor.Darkened(0.35f); 
            
            style.ShadowSize = 0; // Sólido sin difuminar
            
            // 2. Duplicamos el grosor de la base (Offset Y en 12 en lugar de 6)
            style.ShadowOffset = new Vector2(0, 12); 
            style.ContentMarginTop = 0;
        }

        return style;
    }
}