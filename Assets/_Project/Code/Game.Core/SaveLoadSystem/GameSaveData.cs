using System;


namespace Game.Scripts.SaveLoadSystem
{
    [Serializable]
    public class GameSaveData
    {
        public const int CurrentVersion = 1;

        public int Version;


        public static GameSaveData Migrate(GameSaveData saveData)
        {
            return saveData;
        }
    }

    public interface IPersistent
    {
        void Save(GameSaveData gameSaveData);
        void Load(GameSaveData gameSaveData);
    }
}