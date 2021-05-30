using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

// class working with serialization and deserialization of different objects
public class DataManager : MonoBehaviour
{
    // static reference to itself, so that other classes can easily address the manager
    public static DataManager manager;

    private const string SlotFileStart = "slot";
    private const string SlotFileExtenstion = ".save";

    // when saving, we need to get the data about the camera pos
    [SerializeField]
    private MousePanner panner;

    // returns file path for the specified slot
    private string ConstructSlotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, SlotFileStart + slot + SlotFileExtenstion);
    }

    private void Awake()
    {
        manager = this;
    }

    // const string values for serialization of all possible objects
    private const string optionsSerializeName = "options";
    private const string fieldSerializeName = "field";
    private const string timedAnalyzerSerializeName = "timedAnalyzer";
    private const string cameraSerializeName = "camera";

    // loads the game from the specified slot
    public bool LoadFromSlot(int slot)
    {
        string path = ConstructSlotPath(slot);
        if (!File.Exists(path))
        {
            return false;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = File.OpenRead(path))
        {
            var options = formatter.Deserialize(stream) as Dictionary<string, object>;
            GameController.controller.StartNewGame(((float x, float y, float z))options[cameraSerializeName], 
                (GameOptions)options[optionsSerializeName],
                (FieldOptions)options[fieldSerializeName], (TimedGameAnalyzerInfo)options[timedAnalyzerSerializeName]);
        }
        return true;
    }

    // whether the specified slot has saves in it
    public bool HasSavesInSlot(int slot)
    {
        return File.Exists(ConstructSlotPath(slot));
    }

    // save the game into the specified slot
    public void SaveIntoSlot(int slot)
    {
        string path = ConstructSlotPath(slot);
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = File.Create(path))
        {
            (GameOptions options, Field field, TimedGameAnalyzer timedAnalyzer) = GameController.controller.GetGameData();
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { optionsSerializeName, options},
                { fieldSerializeName, field.GetFieldData() },
                { timedAnalyzerSerializeName, timedAnalyzer.GetSerializableInfo()},
                { cameraSerializeName,  panner.GetCurPos()},
            };
            formatter.Serialize(stream, data);
        }
    }

    // returns a DateTime object telling when we saved into the slot last time
    public DateTime GetLastModified(int slot)
    {
        string path = ConstructSlotPath(slot);
        return File.GetLastWriteTime(path);
    }
}
