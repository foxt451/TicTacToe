using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour
{
    public static DataManager manager;

    private const string SlotFileStart = "slot";
    private const string SlotFileExtenstion = ".save";

    private string ConstructSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, SlotFileStart + slot + SlotFileExtenstion);
    }

    private void Awake()
    {
        manager = this;
    }

    private const string optionsSerializeName = "options";
    private const string fieldSerializeName = "field";

    public void LoadFromSlot(int slot)
    {
        string path = ConstructSlotPath(slot);
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = File.OpenRead(path))
        {
            var options = formatter.Deserialize(stream) as Dictionary<string, object>;
            GameController.controller.StartNewGame((GameOptions)options[optionsSerializeName],
                (FieldOptions)options[fieldSerializeName]);
        }
    }

    public void SaveIntoSlot(int slot)
    {
        string path = ConstructSlotPath(slot);
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = File.Create(path))
        {
            (GameOptions options, Field field) = GameController.controller.GetGameData();
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { optionsSerializeName, options},
                { fieldSerializeName, field.GetFieldData() }
            };
            formatter.Serialize(stream, data);
        }
    }
}
