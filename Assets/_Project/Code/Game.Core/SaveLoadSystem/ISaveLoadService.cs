namespace Game.Scripts.SaveLoadSystem
{
    public interface ISaveLoadService
    {
        bool HasSave(string slotName);
        void Save(string slotName, GameSaveData gameSaveData);
        GameSaveData Load(string slotName);
    }
}