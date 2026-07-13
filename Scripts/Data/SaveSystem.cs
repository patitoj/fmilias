using Godot;

public static class SaveSystem
{
    private const string SavePath = "user://savegame.cfg";

    // Guarda el estado granular de la jugada actual
    public static void SaveGameState(int levelIndex, int mistakes, string currentAnswer)
    {
        ConfigFile config = new ConfigFile();
        config.SetValue("Progreso", "Nivel", levelIndex);
        config.SetValue("Progreso", "Errores", mistakes);
        config.SetValue("Progreso", "RespuestaActual", currentAnswer);
        config.Save(SavePath);
    }

    // Verifica si existe una partida previa para cambiar el texto del botón
    public static bool HasSavedGame()
    {
        ConfigFile config = new ConfigFile();
        return config.Load(SavePath) == Error.Ok;
    }

    // Devuelve una tupla con los datos detallados de la partida
    public static (int level, int mistakes, string currentAnswer) LoadGameState()
    {
        ConfigFile config = new ConfigFile();
        Error err = config.Load(SavePath);
        
        if (err == Error.Ok)
        {
            int level = (int)config.GetValue("Progreso", "Nivel", 0);
            int mistakes = (int)config.GetValue("Progreso", "Errores", 0);
            string currentAnswer = (string)config.GetValue("Progreso", "RespuestaActual", "");
            return (level, mistakes, currentAnswer);
        }
        return (0, 0, "");
    }

    // Borra el archivo cuando el jugador completa absolutamente todo el juego
    public static void ClearSave()
    {
        using var dir = DirAccess.Open("user://");
        if (dir != null && dir.FileExists("savegame.cfg"))
        {
            dir.Remove("savegame.cfg");
        }
    }
}