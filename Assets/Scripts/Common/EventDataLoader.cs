using System.Collections.Generic;
using UnityEngine;

public class EventDataLoader
{
    public List<string> LoadEventIds(GSheetManager gsheet)
    {
        var allIds = new List<string>();
        var data = gsheet.GetData();

        foreach (var id in data)
        {
            
        }
        return allIds;
    }
}
