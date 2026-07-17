using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
    private Label _levelLabel;

    private PanelContainer _levelCard; // NUEVO NODO
    private Button _backButton; // NUEVO NODO
    private GridContainer _imagesGrid;
    private HBoxContainer _lettersBox;
    private VBoxContainer _keyboardLayout;

    // Eventos (Delegados)
    public Action<string> OnLetterPressedAction;
    public Action OnDeletePressedAction;
    public Action OnBackButtonPressedAction;

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
        
        // --- RUTAS ACTUALIZADAS ---
        // El LevelCard está suelto arriba de todo en el MainLayout
        _levelCard = GetNode<PanelContainer>("MainLayout/LevelCard");
        _levelLabel = GetNode<Label>("MainLayout/LevelCard/MarginContainer/LevelLabel");
        
        // El BackButton ahora es hijo directo de MainUI
        _backButton = GetNode<Button>("BackButton");

        SetupTopUI();
    }

    private void SetupTopUI()
    {
        // 1. Estilo de la Tarjeta del Nivel (Rectángulo grande colgante)
        
        // Agrandamos la tarjeta (Ancho, Alto). Podés subir estos números si la querés aún más gigante.
        _levelCard.CustomMinimumSize = new Vector2(400, 110); 
        _levelCard.AddThemeStyleboxOverride("panel", CreateTopCardStyle(new Color("#A8E6CF"))); // Verde menta
        
        // Configuramos la fuente
        _levelLabel.AddThemeColorOverride("font_color", _textColor);
        _levelLabel.AddThemeFontSizeOverride("font_size", 34);

        // --- MAGIA DE ALINEACIÓN CENTER BOTTOM ---
        // Le decimos al texto que ocupe toda la tarjeta
        _levelLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _levelLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        // Alineamos la física del texto abajo y al centro
        _levelLabel.VerticalAlignment = VerticalAlignment.Bottom;
        _levelLabel.HorizontalAlignment = HorizontalAlignment.Center;

        // Agarramos el MarginContainer que está adentro de la tarjeta para darle el margen inferior
        MarginContainer cardMargin = _levelCard.GetNode<MarginContainer>("MarginContainer");
        if (cardMargin != null)
        {
            cardMargin.AddThemeConstantOverride("margin_bottom", 15); // Espacio entre el texto y el borde
            cardMargin.AddThemeConstantOverride("margin_top", 0);
        }
        // -----------------------------------------

        // 2. Estilo del Botón de Atrás
        _backButton.Text = "◄"; 
        _backButton.CustomMinimumSize = new Vector2(60, 60); 
        
        Color backColor = new Color("#fe877c"); // Coral
        
        _backButton.AddThemeStyleboxOverride("normal", CreateCandyStyle(backColor));
        _backButton.AddThemeStyleboxOverride("hover", CreateCandyStyle(backColor.Lightened(0.1f)));
        _backButton.AddThemeStyleboxOverride("pressed", CreateCandyStyle(backColor, true));
        _backButton.AddThemeStyleboxOverride("focus", CreateCandyStyle(backColor));
        
        _backButton.AddThemeColorOverride("font_color", new Color("#FFFFFF"));
        _backButton.AddThemeFontSizeOverride("font_size", 28);
        
        _backButton.Pressed += () => OnBackButtonPressedAction?.Invoke();
    }

    // --- NUEVO ESTILO: TARJETA COLGANTE (Sin borde negro, con efecto Jelly) ---
    private StyleBoxFlat CreateTopCardStyle(Color bgColor)
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = bgColor;
        
        // Arriba recto, abajo súper redondo
        style.CornerRadiusTopLeft = 0;
        style.CornerRadiusTopRight = 0;
        style.CornerRadiusBottomRight = 24;
        style.CornerRadiusBottomLeft = 24;
        
        // 1. Usamos el mismo color de fondo pero oscurecido para la profundidad
        // El método Darkened calcula automáticamente un tono más oscuro
        style.BorderColor = bgColor.Darkened(0.15f); 
        
        // 2. Eliminamos los bordes negros finos perimetrales (Top, Left, Right en 0)
        style.BorderWidthTop = 0; 
        style.BorderWidthLeft = 0;
        style.BorderWidthRight = 0;
        
        // 3. Dejamos solo el relieve inferior bien grueso
        style.BorderWidthBottom = 7; 
        
        // 4. Agregamos la pequeña sombra exterior suave hacia abajo
        style.ShadowColor = new Color(0, 0, 0, 0.15f); // Negro al 15% de opacidad
        style.ShadowSize = 6; // Difuminado suave
        style.ShadowOffset = new Vector2(0, 4); // Desplazada hacia abajo
        
        return style;
    }

    private StyleBoxFlat CreatePillStyle(Color bgColor, bool isPressed = false)
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = bgColor;
        
        // Bordes súper redondeados
        style.CornerRadiusTopLeft = 30;
        style.CornerRadiusTopRight = 30;
        style.CornerRadiusBottomRight = 30;
        style.CornerRadiusBottomLeft = 30;
        
        style.BorderColor = _borderColor; // Borde negro exterior
        style.BorderWidthTop = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        
        if (isPressed)
        {
            style.BorderWidthBottom = 2;
            style.ContentMarginTop = 6; // Empuja el contenido al presionar
        }
        else
        {
            style.BorderWidthBottom = 6; // Relieve 3D grueso
            style.ContentMarginTop = 0;
        }

        return style;
    }

    public void UpdateLevelLabel(int levelNumber)
    {
        _levelLabel.Text = $"NIVEL {levelNumber}";
    }

    public void GenerateLetterSlots(int wordLength, char[] userAnswer)
    {
        foreach (Node child in _lettersBox.GetChildren()) child.QueueFree();

        // --- CÁLCULO DINÁMICO DE TAMAÑO ---
        // 1. Obtenemos el ancho real de la pantalla y usamos el 90% para dejar márgenes a los costados
        float screenWidth = GetViewportRect().Size.X;
        float safeWidth = screenWidth * 0.9f; 
        
        // 2. Definimos la separación entre cuadraditos (ej. 8 píxeles)
        int separation = 8;
        float totalSeparation = separation * (wordLength - 1);
        
        // 3. Calculamos el ancho ideal para que todas las letras entren perfecto
        float calculatedWidth = (safeWidth - totalSeparation) / wordLength;
        
        // 4. Ponemos un tope máximo de 80px para que las palabras cortas no queden gigantes
        int finalSizeX = (int)Mathf.Min(calculatedWidth, 80);
        int finalSizeY = finalSizeX + 5; // Lo hacemos apenitas más alto que ancho (como tu 80x85 original)
        
        // 5. La fuente también se achica proporcionalmente al cuadrado
        int fontSize = (int)(finalSizeX * 0.55f);

        // Forzamos la separación en el contenedor
        _lettersBox.AddThemeConstantOverride("separation", separation);
        _lettersBox.Alignment = BoxContainer.AlignmentMode.Center;

        for (int i = 0; i < wordLength; i++)
        {
            Button slotButton = new Button();
            
            // Le aplicamos nuestra nueva matemática al tamaño
            slotButton.CustomMinimumSize = new Vector2(finalSizeX, finalSizeY); 
            
            slotButton.Text = userAnswer[i] == '\0' ? " " : userAnswer[i].ToString();
            slotButton.Disabled = true; 

            StyleBoxFlat slotStyle = new StyleBoxFlat();
            slotStyle.CornerRadiusTopLeft = 16;
            slotStyle.CornerRadiusTopRight = 16;
            slotStyle.CornerRadiusBottomRight = 16;
            slotStyle.CornerRadiusBottomLeft = 16;

            if (userAnswer[i] != '\0')
            {
                // Letra ingresada
                slotStyle.BgColor = new Color("#FFFFFF");
                slotStyle.BorderColor = new Color("#D1D1D1"); 
                slotStyle.BorderWidthTop = 2;
                slotStyle.BorderWidthLeft = 2;
                slotStyle.BorderWidthRight = 2;
                
                // Si la tecla se achicó mucho, bajamos un poco el relieve para que no se deforme
                slotStyle.BorderWidthBottom = finalSizeX < 50 ? 2 : 4; 
            }
            else
            {
                // Hueco vacío
                slotStyle.BgColor = _disabledSlotColor;
                slotStyle.BorderColor = new Color("#C5C5C5"); 
                slotStyle.BorderWidthTop = 2;
                slotStyle.BorderWidthLeft = 2;
                slotStyle.BorderWidthRight = 2;
                slotStyle.BorderWidthBottom = finalSizeX < 50 ? 2 : 4;
            }

            slotButton.AddThemeStyleboxOverride("disabled", slotStyle);
            
            slotButton.AddThemeColorOverride("font_disabled_color", _textColor);
            
            // Asignamos el tamaño de fuente inteligente
            slotButton.AddThemeFontSizeOverride("font_size", fontSize); 

            _lettersBox.AddChild(slotButton);
        }
    }

    public void GenerateObjectHints(List<string> objects, int mistakes)
    {
        foreach (Node child in _imagesGrid.GetChildren()) child.QueueFree();

        int objectsToShow = Mathf.Min(2 + mistakes, 12);
        if (objectsToShow > objects.Count) objectsToShow = objects.Count;

        // --- 1. LÓGICA DE COLUMNAS ---
        // Usamos 3 columnas por defecto (hasta 9 objetos). Si hay 10 o más, pasamos a 4 columnas.
        int columns = objectsToShow > 9 ? 4 : 3;
        _imagesGrid.Columns = columns;

        int separation = 15;
        _imagesGrid.AddThemeConstantOverride("h_separation", separation);
        _imagesGrid.AddThemeConstantOverride("v_separation", separation);

        // --- 2. CÁLCULO DE TAMAÑO (BLOQUEADO A 3x3 o más) ---
        float screenWidth = GetViewportRect().Size.X;
        float screenHeight = GetViewportRect().Size.Y;

        //float safeWidth = screenWidth * 0.94f;
        float safeWidth = columns == 4 ? screenWidth * 0.86f : screenWidth * 0.94f;
        float safeHeight = screenHeight * 0.55f; 

        // Para que las cartas no cambien de tamaño mientras se van sumando del 1 al 9, 
        // obligamos al cálculo a basarse en una grilla llena de 3x3.
        int calcColumns = columns;
        int calcRows = columns == 3 ? 3 : Mathf.CeilToInt((float)objectsToShow / columns);

        // Calculamos cuánto espacio tiene cada "hueco" teórico de esa grilla 3x3 (o 4xX)
        float maxCardWidth = (safeWidth - (separation * (calcColumns - 1))) / calcColumns;
        float maxCardHeight = (safeHeight - (separation * (calcRows - 1))) / calcRows;

        float finalCardWidth = maxCardWidth;
        float finalCardHeight = finalCardWidth * 1.25f;

        if (finalCardHeight > maxCardHeight)
        {
            finalCardHeight = maxCardHeight;
            finalCardWidth = finalCardHeight * 0.8f; 
        }

        int innerMargin = (int)(finalCardWidth * 0.1f); 

        for (int i = 0; i < objectsToShow; i++)
        {
            PanelContainer card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(finalCardWidth, finalCardHeight); 
            card.AddThemeStyleboxOverride("panel", CreateHintCardStyle());

            MarginContainer margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_top", innerMargin);
            margin.AddThemeConstantOverride("margin_bottom", innerMargin);
            margin.AddThemeConstantOverride("margin_left", innerMargin);
            margin.AddThemeConstantOverride("margin_right", innerMargin);
            card.AddChild(margin);

            VBoxContainer vbox = new VBoxContainer();
            vbox.Alignment = BoxContainer.AlignmentMode.Center;
            vbox.AddThemeConstantOverride("separation", 5);
            margin.AddChild(vbox);

            TextureRect imageRect = new TextureRect();
            imageRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            imageRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            imageRect.SizeFlagsVertical = Control.SizeFlags.ExpandFill; 
            
            //imageRect.Texture = GD.Load<Texture2D>("res://icon.svg"); 
                        // NUEVO: Carga dinámica de imágenes
            string objectName = objects[i]; // Ej: "Cuchillo"
            string imagePath = $"res://Assets/Images/{objectName}.webp";

            if (ResourceLoader.Exists(imagePath))
            {
                imageRect.Texture = GD.Load<Texture2D>(imagePath);
            }
            else
            {
                // Si te olvidaste de agregar un WEBP, pone el logo de Godot para que no crashee
                imageRect.Texture = GD.Load<Texture2D>("res://icon.svg");
                GD.PrintErr($"FALTA IMAGEN: No se encontró {imagePath}");
            }
            vbox.AddChild(imageRect);

            Label nameLabel = new Label();
            nameLabel.Text = objects[i].ToUpper(); 
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            nameLabel.AddThemeColorOverride("font_color", _textColor);
            
            int fontSize = (int)Mathf.Max(finalCardWidth * 0.16f, 12); 
            nameLabel.AddThemeFontSizeOverride("font_size", fontSize); 
            
            vbox.AddChild(nameLabel);

            _imagesGrid.AddChild(card);
        }
    }

    private StyleBoxFlat CreateHintCardStyle()
    {
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = new Color("#FFFFFF"); // Blanco puro para resaltar la imagen
        
        style.CornerRadiusTopLeft = 24;
        style.CornerRadiusTopRight = 24;
        style.CornerRadiusBottomRight = 24;
        style.CornerRadiusBottomLeft = 24;

        style.BorderColor = new Color("#D1D1D1"); // Borde gris perla
        style.BorderWidthTop = 2;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthBottom = 6; // Relieve inferior estilo jelly

        return style;
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