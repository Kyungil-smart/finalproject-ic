using System.Collections.Generic;
using UnityEngine;

public interface IProjectListViewer
{
    public IReadOnlyList<ProjectData> Projects { get; }
}
