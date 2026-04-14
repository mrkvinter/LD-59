namespace Game.Scripts.SaveLoadSystem
{
    public class BlankSaveLoadService : ISaveLoadService
    {
        public bool HasSave(string slotName)
        {
            return false;
        }

        public void Save(string slotName, GameSaveData gameSaveData)
        {
        }

        public GameSaveData Load(string slotName)
        {
            return new GameSaveData();
        }
    }
}