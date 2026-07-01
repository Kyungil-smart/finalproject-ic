using System.Collections.Generic;

public class ProjectDetailRenderData
{
    public int index;
    public string title;
    public string projectName;
    public string genre;
    public string theme;
    public string grade;
    public string rewards;
    public uint cost;
    public uint income;
    public List<ReviewResult> reviews = new();
}